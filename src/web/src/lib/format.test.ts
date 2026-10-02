import { describe, expect, it } from "vitest";

import { formatDecimal } from "./format";

describe("formatDecimal", () => {
  it("writes the API's decimal text in Turkish and a dash when there is none", () => {
    expect(formatDecimal("12.500")).toBe("12,5");
    expect(formatDecimal("1234.25")).toBe("1.234,25");
    expect(formatDecimal(null)).toBe("—");
  });
});
