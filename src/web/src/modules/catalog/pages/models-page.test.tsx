import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { listEquipmentCategories, listEquipmentModels } from "@/api/endpoints/catalog/catalog";
import type { EquipmentModelListItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { ModelsPage, type ModelsSearch } from "./models-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/catalog/catalog", () => ({
  listEquipmentModels: vi.fn(),
  getListEquipmentModelsQueryKey: (params?: unknown) => ["/api/v1/equipment-models", params],
  listEquipmentCategories: vi.fn(),
  getListEquipmentCategoriesQueryKey: (params?: unknown) => [
    "/api/v1/equipment-categories",
    params,
  ],
  activateEquipmentModel: vi.fn(),
  deactivateEquipmentModel: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listMock = vi.mocked(listEquipmentModels);
const manager = ["Catalog.Models.View", "Catalog.Models.Create", "Catalog.Models.Edit"];

function aModel(overrides: Partial<EquipmentModelListItem> = {}): EquipmentModelListItem {
  return {
    id: "m-1",
    brand: "Shure",
    name: "SM58",
    categoryId: "c-2",
    categoryPath: ["Ses", "Mikrofon"],
    trackingType: "serialized",
    weightKilograms: "0.298",
    powerWatts: null,
    isActive: true,
    version: 1,
    ...overrides,
  };
}

function renderPage(search: ModelsSearch = {}, permissions = manager) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <ModelsPage search={search} onSearchChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("ModelsPage", () => {
  beforeEach(() => {
    listMock.mockReset();
    vi.mocked(listEquipmentCategories).mockResolvedValue([]);
  });

  it("lists models with their category path, tracking type and values in Turkish", async () => {
    listMock.mockResolvedValue({ items: [aModel()], page: 1, pageSize: 25, totalCount: 1 });

    renderPage({ categoryId: "c-1" });

    const row = await screen.findByRole("row", { name: /Shure SM58/u });
    expect(within(row).getByRole("link", { name: "Shure SM58" })).toHaveAttribute(
      "href",
      "/catalog/models/m-1",
    );
    expect(within(row).getByText("Ses › Mikrofon")).toBeInTheDocument();
    expect(within(row).getByText("Seri no'lu")).toBeInTheDocument();
    expect(within(row).getByText("0,298")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Model ekle" })).toHaveAttribute(
      "href",
      "/catalog/models/new",
    );
    expect(listMock).toHaveBeenCalledWith(
      expect.objectContaining({ categoryId: "c-1", page: 1 }),
      expect.anything(),
    );
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, []);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
