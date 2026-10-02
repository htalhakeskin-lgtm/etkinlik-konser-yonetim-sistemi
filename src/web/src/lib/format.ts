const timeFormat = new Intl.DateTimeFormat("tr-TR", {
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "Europe/Istanbul",
});

const dateTimeFormat = new Intl.DateTimeFormat("tr-TR", {
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "Europe/Istanbul",
});

const decimalFormat = new Intl.NumberFormat("tr-TR", { maximumFractionDigits: 3 });

/** A moment in Istanbul time, e.g. `04.01.2027 09:00` (ui §13). */
export function formatDateTime(value: Date | string): string {
  return dateTimeFormat.format(typeof value === "string" ? new Date(value) : value);
}

/**
 * A decimal the API sends as text (api A-11), in Turkish, e.g. `"12.500"` → `12,5`. Formatting the text
 * directly keeps every digit; a missing value is a dash.
 */
export function formatDecimal(value: string | null | undefined): string {
  return value === null || value === undefined
    ? "—"
    : decimalFormat.format(value as Intl.StringNumericLiteral);
}

/** A time of day in Istanbul, e.g. `14:30` (ui §13). */
export function formatTime(value: Date | string): string {
  return timeFormat.format(typeof value === "string" ? new Date(value) : value);
}
