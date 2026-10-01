import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { deactivateWarehouse, listWarehouses } from "@/api/endpoints/inventory/inventory";
import type { WarehouseListItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { WarehousesPage, type WarehousesSearch } from "./warehouses-page";

vi.mock("@/api/endpoints/inventory/inventory", () => ({
  listWarehouses: vi.fn(),
  getListWarehousesQueryKey: (params: unknown) => ["/api/v1/warehouses", params],
  getWarehouse: vi.fn(),
  getGetWarehouseQueryKey: (id: string) => [`/api/v1/warehouses/${id}`],
  createWarehouse: vi.fn(),
  editWarehouse: vi.fn(),
  activateWarehouse: vi.fn(),
  deactivateWarehouse: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listMock = vi.mocked(listWarehouses);
const allPermissions = [
  "Inventory.Warehouses.View",
  "Inventory.Warehouses.Create",
  "Inventory.Warehouses.Edit",
  "Inventory.Warehouses.Deactivate",
];

function aListedWarehouse(overrides: Partial<WarehouseListItem>): WarehouseListItem {
  return {
    id: crypto.randomUUID(),
    name: "Merkez Depo",
    city: "İstanbul",
    address: "Depo Sk. 4",
    isActive: true,
    version: 1,
    ...overrides,
  };
}

function renderPage(search: WarehousesSearch = {}, permissions = ["Inventory.Warehouses.View"]) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <WarehousesPage search={search} onSearchChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("WarehousesPage", () => {
  beforeEach(() => {
    listMock.mockReset();
  });

  it("lists the warehouses with their city and status", async () => {
    listMock.mockResolvedValue({
      items: [
        aListedWarehouse({}),
        aListedWarehouse({ name: "Kuzey Depo", city: "Ankara", isActive: false }),
      ],
      page: 1,
      pageSize: 25,
      totalCount: 2,
    });

    renderPage({ status: "all" });

    expect(await screen.findByText("Kuzey Depo")).toBeInTheDocument();
    expect(screen.getByText("Ankara")).toBeInTheDocument();
    expect(screen.getByText("Pasif")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Depo ekle" })).not.toBeInTheDocument();
  });

  it("deactivates a warehouse after asking, on the version the row showed", async () => {
    listMock.mockResolvedValue({
      items: [aListedWarehouse({ id: "w-1", version: 3 })],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    vi.mocked(deactivateWarehouse).mockResolvedValue({} as never);
    renderPage({}, allPermissions);
    const user = userEvent.setup();

    await user.click(await screen.findByRole("button", { name: "Merkez Depo için işlemler" }));
    await user.click(await screen.findByRole("menuitem", { name: "Pasifleştir" }));
    await user.click(await screen.findByRole("button", { name: "Pasifleştir" }));

    await vi.waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Depo pasifleştirildi.");
    });
    expect(deactivateWarehouse).toHaveBeenCalledWith("w-1", { headers: { "If-Match": '"3"' } });
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, ["Identity.Users.View"]);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
