import { describe, expect, it } from "vitest";

import { startPage } from "./start-page";

describe("startPage", () => {
  it("sends a system administrator to the users, even with other roles", () => {
    expect(startPage(["bookingManager", "systemAdministrator"])).toBe("/admin/users");
  });

  it("sends a booking manager to the parties until the events screen arrives", () => {
    expect(startPage(["bookingManager"])).toBe("/parties");
  });

  it("keeps roles whose start screen has not arrived on the start page", () => {
    expect(startPage(["technicalManager"])).toBeUndefined();
  });
});
