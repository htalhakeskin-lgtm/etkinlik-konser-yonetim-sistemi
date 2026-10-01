import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";
import { z } from "zod";

import { changeMyPassword } from "@/api/endpoints/identity/identity";
import { ChangeMyPasswordBody } from "@/api/zod/identity/identity.zod";
import { FormAlert } from "@/components/common/form-alert";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { errorMessage } from "@/lib/api-error-messages";

import { meQuery } from "../session";
import { PasswordInput } from "./password-input";
import { currentPasswordError, newPasswordError } from "./password-problems";

const schema = z.object({
  currentPassword: z.string().min(1),
  newPassword: ChangeMyPasswordBody.shape.newPassword.min(1),
});

type ChangePasswordValues = z.infer<typeof schema>;

export type ChangePasswordDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

// Changing one's password from the user menu (US-SYS-011): the current password again, then the new
// one. The user's other sessions end; this one stays (BR-SYS-007).
export function ChangePasswordDialog({ open, onOpenChange }: ChangePasswordDialogProps) {
  const { t } = useTranslation("identity");
  const queryClient = useQueryClient();
  const { control, handleSubmit, setError, reset } = useForm<ChangePasswordValues>({
    resolver: zodResolver(schema),
    defaultValues: { currentPassword: "", newPassword: "" },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const change = useMutation({
    mutationFn: (values: ChangePasswordValues) => changeMyPassword(values),
    onSuccess: (user) => {
      queryClient.setQueryData(meQuery.queryKey, user);
      toast.success(t("changePassword.done"));
      close();
    },
    onError: (error) => {
      const current = currentPasswordError(error);
      const next = newPasswordError(error, t);
      if (current !== undefined) {
        setError("currentPassword", { message: current }, { shouldFocus: true });
      } else if (next !== undefined) {
        setError("newPassword", { message: next }, { shouldFocus: true });
      }
    },
  });
  const isFieldError =
    change.isError &&
    (currentPasswordError(change.error) ?? newPasswordError(change.error, t)) !== undefined;

  function close() {
    reset();
    change.reset();
    onOpenChange(false);
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          close();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("changePassword.title")}</DialogTitle>
          <DialogDescription>{t("changePassword.description")}</DialogDescription>
        </DialogHeader>
        <form
          noValidate
          className="flex flex-col gap-6"
          onSubmit={(event) =>
            void handleSubmit((values) => {
              change.mutate(values);
            })(event)
          }
        >
          <FieldGroup>
            <Controller
              control={control}
              name="currentPassword"
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor="current-password">
                    {t("changePassword.currentPassword")}
                  </FieldLabel>
                  <PasswordInput
                    {...field}
                    id="current-password"
                    autoComplete="current-password"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
            <Controller
              control={control}
              name="newPassword"
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor="changed-password">{t("setPassword.newPassword")}</FieldLabel>
                  <FieldDescription>{t("setPassword.rules")}</FieldDescription>
                  <PasswordInput
                    {...field}
                    id="changed-password"
                    autoComplete="new-password"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
          </FieldGroup>
          {change.isError && !isFieldError && <FormAlert>{errorMessage(change.error)}</FormAlert>}
          <DialogFooter>
            <Button type="button" variant="outline" onClick={close}>
              {t("changePassword.cancel")}
            </Button>
            <Button type="submit" isLoading={change.isPending}>
              {t("setPassword.submit")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
