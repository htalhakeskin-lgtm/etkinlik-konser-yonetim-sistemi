import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { changeMyPassword } from "@/api/endpoints/identity/identity";
import type { SignedInUserDetails } from "@/api/model";
import { ChangeMyPasswordBody } from "@/api/zod/identity/identity.zod";
import { FormAlert } from "@/components/common/form-alert";
import { Button } from "@/components/ui/button";
import { Field, FieldDescription, FieldError, FieldLabel } from "@/components/ui/field";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";

import { PasswordInput } from "./password-input";

const passwordProblems = [
  "tooShort",
  "tooLong",
  "whitespace",
  "containsEmail",
  "containsProductName",
  "common",
  "sameAsCurrent",
] as const;

const schema = z.object({ newPassword: ChangeMyPasswordBody.shape.newPassword.min(1) });

type SetPasswordValues = z.infer<typeof schema>;

export type SetPasswordFormProps = {
  onChanged: (user: SignedInUserDetails) => void;
};

// The new password that replaces a temporary one (US-SYS-011): the rules sit under the field, and a
// refusal names the rule that was broken (BR-SYS-007).
export function SetPasswordForm({ onChanged }: SetPasswordFormProps) {
  const { t } = useTranslation("identity");
  const { control, handleSubmit, setError } = useForm<SetPasswordValues>({
    resolver: zodResolver(schema),
    defaultValues: { newPassword: "" },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const change = useMutation({
    mutationFn: (values: SetPasswordValues) =>
      changeMyPassword({ currentPassword: null, newPassword: values.newPassword }),
    onSuccess: onChanged,
    onError: (error) => {
      const message = fieldMessage(error, t);
      if (message !== undefined) {
        setError("newPassword", { message }, { shouldFocus: true });
      }
    },
  });

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          change.mutate(values);
        })(event)
      }
    >
      <Controller
        control={control}
        name="newPassword"
        render={({ field, fieldState }) => (
          <Field data-invalid={fieldState.invalid}>
            <FieldLabel htmlFor="new-password">{t("setPassword.newPassword")}</FieldLabel>
            <FieldDescription>{t("setPassword.rules")}</FieldDescription>
            <PasswordInput
              {...field}
              id="new-password"
              autoComplete="new-password"
              aria-invalid={fieldState.invalid}
            />
            <FieldError errors={[fieldState.error]} />
          </Field>
        )}
      />
      {change.isError && fieldMessage(change.error, t) === undefined && (
        <FormAlert>{errorMessage(change.error)}</FormAlert>
      )}
      <Button type="submit" isLoading={change.isPending}>
        {t("setPassword.submit")}
      </Button>
    </form>
  );
}

// The refusals that belong under the field: the broken password rule, or a validation message.
function fieldMessage(
  error: Error,
  t: ReturnType<typeof useTranslation<"identity">>["t"],
): string | undefined {
  if (!(error instanceof ApiError)) {
    return undefined;
  }

  if (error.code === "BR-SYS-007") {
    const reason = passwordProblems.find((problem) => problem === error.params.reason);
    return reason === undefined
      ? errorMessage(error)
      : t(`setPassword.problem.${reason}`, { minLength: error.params.minLength });
  }

  return error.status === 400
    ? fieldErrors(error).find((field) => field.path === "newPassword")?.message
    : undefined;
}
