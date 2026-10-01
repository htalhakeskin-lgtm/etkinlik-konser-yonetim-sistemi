const timeFormat = new Intl.DateTimeFormat("tr-TR", {
  hour: "2-digit",
  minute: "2-digit",
  timeZone: "Europe/Istanbul",
});

/** A time of day in Istanbul, e.g. `14:30` (ui §13). */
export function formatTime(value: Date | string): string {
  return timeFormat.format(typeof value === "string" ? new Date(value) : value);
}
