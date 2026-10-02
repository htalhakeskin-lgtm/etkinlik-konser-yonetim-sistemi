import { z } from "zod";

import { i18n } from "@/lib/i18n";

/**
 * A decimal field as people type it: digits with a comma or a dot and at most <paramref name="scale"/>
 * decimals, empty when there is no value, above zero and below <paramref name="maximum"/>. The API takes
 * decimals as text (api A-11).
 */
export function decimalField(maximum: number, scale: number) {
  const pattern = new RegExp(`^\\d{1,9}([.,]\\d{1,${String(scale)}})?$`, "u");
  return z
    .string()
    .trim()
    .refine((value) => value === "" || pattern.test(value), {
      message: i18n.t("validation:decimal", { scale }),
    })
    .refine(
      (value) =>
        value === "" ||
        !pattern.test(value) ||
        (Number(toApiDecimal(value)) > 0 && Number(toApiDecimal(value)) < maximum),
      { message: i18n.t("validation:positive") },
    );
}

/** A whole number field, empty when there is no value, from one up to <paramref name="maximum"/>. */
export function wholeField(maximum: number) {
  return z
    .string()
    .trim()
    .refine(
      (value) =>
        value === "" ||
        (/^\d{1,9}$/u.test(value) && Number(value) >= 1 && Number(value) <= maximum),
      {
        message: i18n.t("validation:positiveWhole"),
      },
    );
}

/** The typed decimal as the API's text: `12,5` → `12.5`. */
export function toApiDecimal(value: string): string {
  return value.replace(",", ".");
}

/** The API's decimal text as the field shows it: `12.500` → `12,5`; no value is an empty field. */
export function fromApiDecimal(value: string | null | undefined): string {
  if (value === null || value === undefined) {
    return "";
  }

  const trimmed = value.includes(".") ? value.replace(/0+$/u, "").replace(/\.$/u, "") : value;
  return trimmed.replace(".", ",");
}

/** A field's text for the request: empty is no value, anything else the API's decimal text. */
export function optionalDecimal(value: string): string | null {
  return value === "" ? null : toApiDecimal(value);
}
