import type { TFunction } from "i18next";

import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";

const passwordProblems = [
  "tooShort",
  "tooLong",
  "whitespace",
  "containsEmail",
  "containsProductName",
  "common",
  "sameAsCurrent",
] as const;

/**
 * The message a refused new password shows under its field: the broken password rule (BR-SYS-007) or
 * the field's validation message. Other errors belong above the form.
 */
export function newPasswordError(error: Error, t: TFunction<"identity">): string | undefined {
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

/** The message a refused current password shows under its field. */
export function currentPasswordError(error: Error): string | undefined {
  return error instanceof ApiError && error.status === 400
    ? fieldErrors(error).find((field) => field.path === "currentPassword")?.message
    : undefined;
}
