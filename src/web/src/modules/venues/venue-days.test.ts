import { describe, expect, it } from "vitest";

import { endForApi, formatDays, lastDayOf } from "./venue-days";

describe("venue days", () => {
  it("sends the day after the last day entered, and reads it back", () => {
    expect(endForApi("2027-02-28")).toBe("2027-03-01");
    expect(endForApi("2028-02-28")).toBe("2028-02-29");
    expect(endForApi("2027-12-31")).toBe("2028-01-01");
    expect(lastDayOf("2027-03-01")).toBe("2027-02-28");
  });

  it("leaves an empty end open", () => {
    expect(endForApi("")).toBeNull();
    expect(lastDayOf(null)).toBe("");
  });

  it("shows a range with its last day included", () => {
    expect(formatDays("2027-03-10", "2027-03-13")).toBe("10.03.2027 – 12.03.2027");
    expect(formatDays(null, "2027-04-01")).toBe("— – 31.03.2027");
  });
});
