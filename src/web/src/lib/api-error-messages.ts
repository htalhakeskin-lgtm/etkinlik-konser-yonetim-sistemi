import errors from "@/locales/tr/errors.json";
import validation from "@/locales/tr/validation.json";

import { ApiError } from "./api-error";
import { i18n } from "./i18n";

type ErrorCode = keyof typeof errors;
type ValidationCode = keyof typeof validation;

/** A validation message for one form field; the path uses the form's dotted notation. */
export type FieldError = { path: string; message: string };

/**
 * The text for a failed call: the rule's own text for a business rule (`errors:{code}`, filled with the
 * server's values), otherwise the text of the technical code (api §8, ui §7.4).
 */
export function errorMessage(error: unknown): string {
  const code = error instanceof ApiError ? error.code : "internalError";
  const params = error instanceof ApiError ? error.params : {};
  return i18n.t(`errors:${isErrorCode(code) ? code : "internalError"}`, { ...params });
}

/**
 * The field messages of a validation problem (ui §7.4). A pointer the form has no field for is still
 * returned; the form shows those above its fields.
 */
export function fieldErrors(error: ApiError): FieldError[] {
  return error.errors.map((problem) => ({
    path: pointerToPath(problem.pointer),
    message: i18n.t(`validation:${isValidationCode(problem.code) ? problem.code : "invalid"}`, {
      ...problem.params,
    }),
  }));
}

/** Turns a JSON Pointer into a form path: `/units/3/serialNumber` → `units.3.serialNumber`. */
export function pointerToPath(pointer: string): string {
  return pointer
    .split("/")
    .slice(1)
    .map((segment) => segment.replaceAll("~1", "/").replaceAll("~0", "~"))
    .join(".");
}

function isErrorCode(code: string): code is ErrorCode {
  return Object.hasOwn(errors, code);
}

function isValidationCode(code: string): code is ValidationCode {
  return Object.hasOwn(validation, code);
}
