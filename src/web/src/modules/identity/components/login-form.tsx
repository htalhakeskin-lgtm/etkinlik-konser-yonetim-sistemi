import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import type { z } from "zod";

import { login } from "@/api/endpoints/identity/identity";
import type { SignedInUserDetails } from "@/api/model";
import { LoginBody } from "@/api/zod/identity/identity.zod";
import { FormAlert } from "@/components/common/form-alert";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";
import { formatTime } from "@/lib/format";

import { PasswordInput } from "./password-input";

const schema = LoginBody.extend({
  email: LoginBody.shape.email.trim().min(1),
  password: LoginBody.shape.password.min(1),
});

type LoginValues = z.infer<typeof schema>;

export type LoginFormProps = {
  defaultEmail?: string;
  onSignedIn: (user: SignedInUserDetails) => void;
};

// Email and password (US-SYS-010). A refusal never says which of the two was wrong; a locked account
// says until when (BR-SYS-005).
export function LoginForm({ defaultEmail = "", onSignedIn }: LoginFormProps) {
  const { t } = useTranslation("identity");
  const { control, handleSubmit, setError } = useForm<LoginValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: defaultEmail, password: "" },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const signIn = useMutation({
    mutationFn: (values: LoginValues) => login(values),
    meta: { expectsUnauthorized: true },
    onSuccess: onSignedIn,
    onError: (error) => {
      if (error instanceof ApiError && error.status === 400) {
        for (const field of fieldErrors(error)) {
          if (field.path === "email" || field.path === "password") {
            setError(field.path, { message: field.message }, { shouldFocus: true });
          }
        }
      }
    },
  });

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          signIn.mutate(values);
        })(event)
      }
    >
      <FieldGroup>
        <Controller
          control={control}
          name="email"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="login-email">{t("signIn.email")}</FieldLabel>
              <Input
                {...field}
                id="login-email"
                type="email"
                autoComplete="username"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="password"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="login-password">{t("signIn.password")}</FieldLabel>
              <PasswordInput
                {...field}
                id="login-password"
                autoComplete="current-password"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
      </FieldGroup>
      {signIn.isError && !(signIn.error instanceof ApiError && signIn.error.status === 400) && (
        <FormAlert>{refusalText(signIn.error, t)}</FormAlert>
      )}
      <Button type="submit" isLoading={signIn.isPending}>
        {t("signIn.submit")}
      </Button>
    </form>
  );
}

function refusalText(error: Error, t: ReturnType<typeof useTranslation<"identity">>["t"]): string {
  if (
    error instanceof ApiError &&
    error.code === "BR-SYS-005" &&
    typeof error.params.lockedUntil === "string"
  ) {
    return t("signIn.locked", { time: formatTime(error.params.lockedUntil) });
  }

  return errorMessage(error);
}
