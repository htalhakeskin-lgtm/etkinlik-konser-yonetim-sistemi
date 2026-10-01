import { z } from "zod";

import { i18n } from "./i18n";

type ValidationKey = "required" | "minLength" | "maxLength" | "email" | "pattern" | "invalid";

function message(key: ValidationKey, params: Record<string, unknown> = {}): string {
  return i18n.t(`validation:${key}`, params);
}

/**
 * Gives the schemas' messages the texts of the server's validation codes (ui §7.3), so a field shows
 * the same message whichever side found the error. Called once at startup.
 */
export function applyServerValidationTexts(): void {
  z.config({
    customError: (issue) => {
      switch (issue.code) {
        case "too_small":
          return issue.origin === "string" && Number(issue.minimum) === 1
            ? message("required")
            : message("minLength", { min: Number(issue.minimum) });
        case "too_big":
          return message("maxLength", { max: Number(issue.maximum) });
        case "invalid_type":
          return issue.input === undefined || issue.input === null
            ? message("required")
            : message("invalid");
        case "invalid_format":
          return issue.format === "email" ? message("email") : message("pattern");
        default:
          return message("invalid");
      }
    },
  });
}
