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

/** A moment in Istanbul time, e.g. `04.01.2027 09:00` (ui §13). */
export function formatDateTime(value: Date | string): string {
  return dateTimeFormat.format(typeof value === "string" ? new Date(value) : value);
}

/** A time of day in Istanbul, e.g. `14:30` (ui §13). */
export function formatTime(value: Date | string): string {
  return timeFormat.format(typeof value === "string" ? new Date(value) : value);
}
