// The venue's days travel half-open (database §7.3): the API's end day is not included, while people
// enter and read the last day included (venues VN-01). These helpers turn one into the other.

/** The day after the given `yyyy-mm-dd` day. */
export function nextDay(day: string): string {
  const date = new Date(`${day}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + 1);
  return date.toISOString().slice(0, 10);
}

/** The day before the given `yyyy-mm-dd` day. */
export function previousDay(day: string): string {
  const date = new Date(`${day}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() - 1);
  return date.toISOString().slice(0, 10);
}

/** The API's end day (not included) for the last day a person entered; empty is open. */
export function endForApi(lastDay: string): string | null {
  return lastDay === "" ? null : nextDay(lastDay);
}

/** The last day included, for a form, from the API's end day; open is empty. */
export function lastDayOf(end: string | null): string {
  return end === null ? "" : previousDay(end);
}

/** A `yyyy-mm-dd` day as people read it: `10.03.2027`. */
export function formatDay(day: string): string {
  const [year, month, date] = day.split("-");
  return `${date ?? ""}.${month ?? ""}.${year ?? ""}`;
}

/** A half-open range as people read it, the last day included; open ends show as a dash. */
export function formatDays(start: string | null, end: string | null): string {
  const first = start === null ? "—" : formatDay(start);
  const last = end === null ? "—" : formatDay(previousDay(end));
  return `${first} – ${last}`;
}
