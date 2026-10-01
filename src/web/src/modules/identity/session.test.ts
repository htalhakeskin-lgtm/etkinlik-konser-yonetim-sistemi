import { describe, expect, it } from "vitest";

import { safeRedirect } from "./session";

describe("safeRedirect", () => {
  it.each([
    ["/events?page=2", "/events?page=2"],
    [undefined, "/"],
    ["https://example.com", "/"],
    ["//example.com", "/"],
  ])("goes back to %s only inside the application", (target, expected) => {
    expect(safeRedirect(target)).toBe(expected);
  });
});
