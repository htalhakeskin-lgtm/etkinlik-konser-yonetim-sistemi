import { describe, expect, it } from "vitest";

import type { AuditEntryItem } from "@/api/model";

import { describeChanges, entityTypeName } from "./changes";

function anEntry(changes: unknown, overrides: Partial<AuditEntryItem> = {}): AuditEntryItem {
  return {
    id: "e-1",
    occurredAt: "2027-01-04T06:00:00+00:00",
    actorId: "a-1",
    actorName: "Zeynep Ak",
    module: "identity",
    entityType: "User",
    entityId: "u-1",
    rootType: "User",
    rootId: "u-1",
    action: "updated",
    changes: changes as AuditEntryItem["changes"],
    traceId: null,
    ...overrides,
  };
}

describe("describeChanges", () => {
  it("names the fields and values in Turkish, by the record's module", () => {
    const changes = describeChanges(
      anEntry({
        roles: { old: ["bookingManager"], new: ["bookingManager", "systemAdministrator"] },
        mustChangePassword: { old: true, new: false },
        deactivatedAt: { old: null, new: "2027-01-04T06:00:00+00:00" },
      }),
    );

    expect(changes).toEqual([
      {
        field: "roles",
        label: "Roller",
        old: "Booking müdürü",
        new: "Booking müdürü, Sistem yöneticisi",
      },
      { field: "mustChangePassword", label: "Yeni şifre bekleniyor", old: "Evet", new: "Hayır" },
      {
        field: "deactivatedAt",
        label: "Pasifleştirildiği zaman",
        old: "—",
        new: "04.01.2027 09:00",
      },
    ]);
  });

  it("shows a field without a Turkish name as it is, and only the new value of a new record", () => {
    const changes = describeChanges(
      anEntry(
        { shelfCode: { new: "A-12" } },
        { action: "created", module: "inventory", entityType: "Warehouse" },
      ),
    );

    expect(changes).toEqual([
      { field: "shelfCode", label: "shelfCode", old: undefined, new: "A-12" },
    ]);
  });

  it("names the record types", () => {
    expect(entityTypeName("Warehouse")).toBe("Depo");
    expect(entityTypeName("Rider")).toBe("Rider");
  });
});
