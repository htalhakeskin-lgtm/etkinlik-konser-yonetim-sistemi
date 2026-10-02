import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createEquipmentCategory, listEquipmentCategories } from "@/api/endpoints/catalog/catalog";
import type { EquipmentCategoryItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { CategoriesPage, type CategoriesSearch } from "./categories-page";

vi.mock("@/api/endpoints/catalog/catalog", () => ({
  listEquipmentCategories: vi.fn(),
  getListEquipmentCategoriesQueryKey: (params?: unknown) => [
    "/api/v1/equipment-categories",
    params,
  ],
  createEquipmentCategory: vi.fn(),
  editEquipmentCategory: vi.fn(),
  activateEquipmentCategory: vi.fn(),
  deactivateEquipmentCategory: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listMock = vi.mocked(listEquipmentCategories);
const createMock = vi.mocked(createEquipmentCategory);
const manager = [
  "Catalog.Categories.View",
  "Catalog.Categories.Create",
  "Catalog.Categories.Edit",
  "Catalog.Categories.Deactivate",
];

function aCategory(overrides: Partial<EquipmentCategoryItem>): EquipmentCategoryItem {
  return {
    id: "s",
    name: "Ses",
    parentId: null,
    path: ["Ses"],
    activeModelCount: 0,
    isActive: true,
    version: 1,
    ...overrides,
  };
}

const tree = [
  aCategory({}),
  aCategory({
    id: "m",
    name: "Mikrofon",
    parentId: "s",
    path: ["Ses", "Mikrofon"],
    activeModelCount: 4,
  }),
  aCategory({ id: "old", name: "Eski", parentId: null, path: ["Eski"], isActive: false }),
];

function renderPage(search: CategoriesSearch = {}, permissions = manager) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <CategoriesPage search={search} onSearchChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("CategoriesPage", () => {
  beforeEach(() => {
    listMock.mockReset();
    createMock.mockReset();
  });

  it("shows the active tree indented, with each category's active models", async () => {
    listMock.mockResolvedValue(tree);

    renderPage();

    const row = await screen.findByRole("row", { name: /Mikrofon/u });
    expect(within(row).getByText("4")).toBeInTheDocument();
    expect(within(row).getByText("Mikrofon")).toHaveAttribute("data-depth", "1");
    expect(screen.queryByText("Eski")).not.toBeInTheDocument();
    expect(listMock).toHaveBeenCalledWith({ status: "all" }, expect.anything());
  });

  it("adds a subcategory under the chosen parent and shows a taken name under the name", async () => {
    listMock.mockResolvedValue(tree);
    createMock.mockRejectedValue(anApiError(422, "BR-EQP-011"));
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Ses için işlemler" }));
    await user.click(await screen.findByRole("menuitem", { name: "Alt kategori ekle" }));
    await user.type(screen.getByLabelText("Ad *"), "Mikrofon");
    await user.click(screen.getByRole("button", { name: "Oluştur" }));

    expect(createMock).toHaveBeenCalledWith({ name: "Mikrofon", parentId: "s" });
    expect(await screen.findByText(/aynı adlı bir kategori zaten var/u)).toBeInTheDocument();
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, []);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
