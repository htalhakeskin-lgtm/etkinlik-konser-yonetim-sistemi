import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { listAuditEntries } from "@/api/endpoints/audit/audit";
import type { AuditEntryItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { AuditPage, type AuditSearch } from "./audit-page";

vi.mock("@/api/endpoints/audit/audit", () => ({
  listAuditEntries: vi.fn(),
  getListAuditEntriesQueryKey: (params: unknown) => ["/api/v1/audit-entries", params],
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  listUsers: vi.fn(() => Promise.resolve({ items: [], page: 1, pageSize: 100, totalCount: 0 })),
  getListUsersQueryKey: (params: unknown) => ["/api/v1/users", params],
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));

const listMock = vi.mocked(listAuditEntries);

function anEntry(overrides: Partial<AuditEntryItem>): AuditEntryItem {
  return {
    id: crypto.randomUUID(),
    occurredAt: "2027-01-04T06:00:00+00:00",
    actorId: "a-1",
    actorName: "Zeynep Ak",
    module: "inventory",
    entityType: "Warehouse",
    entityId: "w-1",
    action: "updated",
    changes: { name: { old: "Merkez Depo", new: "Kuzey Depo" } },
    traceId: null,
    ...overrides,
  };
}

function renderPage(
  search: AuditSearch = {},
  permissions = ["Audit.Entries.View", "Identity.Users.View"],
) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <AuditPage search={search} onSearchChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("AuditPage", () => {
  beforeEach(() => {
    listMock.mockReset();
  });

  it("shows who changed what and when, with each field's old and new value", async () => {
    listMock.mockResolvedValue({ items: [anEntry({})], nextCursor: null });

    renderPage();

    expect(await screen.findByText("Zeynep Ak")).toBeInTheDocument();
    expect(screen.getByText("04.01.2027 09:00")).toBeInTheDocument();
    expect(screen.getByText("Güncellendi")).toBeInTheDocument();
    await userEvent.setup().click(screen.getByText("Değişiklikleri göster"));
    expect(screen.getByRole("cell", { name: "Merkez Depo" })).toBeInTheDocument();
    expect(screen.getByRole("cell", { name: "Kuzey Depo" })).toBeInTheDocument();
  });

  it("asks for the last day included, as the server stops before its end day", async () => {
    listMock.mockResolvedValue({ items: [], nextCursor: null });

    renderPage({ from: "2027-01-04", to: "2027-01-05", entityType: "Warehouse" });

    expect(await screen.findByText("Bu süzgeçlere uyan değişiklik yok.")).toBeInTheDocument();
    expect(listMock).toHaveBeenCalledWith(
      { actorId: undefined, from: "2027-01-04", to: "2027-01-06", entityType: "Warehouse" },
      expect.anything(),
    );
  });

  it("loads the next slice after the last cursor", async () => {
    listMock
      .mockResolvedValueOnce({ items: [anEntry({ actorName: "Zeynep Ak" })], nextCursor: "c-1" })
      .mockResolvedValueOnce({ items: [anEntry({ actorName: "Ali Bal" })], nextCursor: null });
    renderPage();

    await userEvent.setup().click(await screen.findByRole("button", { name: "Daha fazla yükle" }));

    expect(await screen.findByText("Ali Bal")).toBeInTheDocument();
    expect(listMock).toHaveBeenLastCalledWith(
      expect.objectContaining({ after: "c-1" }),
      expect.anything(),
    );
    expect(screen.queryByRole("button", { name: "Daha fazla yükle" })).not.toBeInTheDocument();
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, ["Identity.Users.View"]);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
