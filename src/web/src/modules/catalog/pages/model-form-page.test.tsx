import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  createEquipmentModel,
  getEquipmentModel,
  listEquipmentCategories,
} from "@/api/endpoints/catalog/catalog";
import type { EquipmentModelDetails } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { ModelFormPage } from "./model-form-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/catalog/catalog", () => ({
  getEquipmentModel: vi.fn(),
  getGetEquipmentModelQueryKey: (id: string) => [`/api/v1/equipment-models/${id}`],
  getListEquipmentModelsQueryKey: (params?: unknown) => ["/api/v1/equipment-models", params],
  listEquipmentCategories: vi.fn(),
  getListEquipmentCategoriesQueryKey: (params?: unknown) => [
    "/api/v1/equipment-categories",
    params,
  ],
  createEquipmentModel: vi.fn(),
  editEquipmentModel: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));

const createMock = vi.mocked(createEquipmentModel);
const manager = ["Catalog.Models.View", "Catalog.Models.Create", "Catalog.Models.Edit"];

function renderPage(modelId?: string) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions: manager }));
  const onSaved = vi.fn();
  render(
    <QueryClientProvider client={client}>
      <ModelFormPage modelId={modelId} onSaved={onSaved} onCancel={vi.fn()} />
    </QueryClientProvider>,
  );
  return { onSaved };
}

// The form has its actions both in the header and at its end (11 §2.4); either one submits.
function submitButton(): HTMLElement {
  const button = screen.getAllByRole("button", { name: "Oluştur" }).at(-1);
  if (button === undefined) {
    throw new Error("The form has no submit button.");
  }

  return button;
}

describe("ModelFormPage", () => {
  beforeEach(() => {
    createMock.mockReset();
    vi.mocked(listEquipmentCategories).mockResolvedValue([
      {
        id: "c-2",
        name: "Mikrofon",
        parentId: "c-1",
        path: ["Ses", "Mikrofon"],
        activeModelCount: 0,
        isActive: true,
        version: 1,
      },
    ]);
  });

  it("creates a model with its decimals as the API's text", async () => {
    createMock.mockResolvedValue({ id: "m-1" } as EquipmentModelDetails);
    const user = userEvent.setup();
    const { onSaved } = renderPage();

    await user.type(await screen.findByLabelText("Marka *"), "Shure");
    await user.type(screen.getByLabelText("Model adı *"), "SM58");
    await user.click(screen.getByLabelText("Kategori *"));
    await user.click(await screen.findByRole("option", { name: "Ses › Mikrofon" }));
    await user.type(screen.getByLabelText("Ağırlık (kg)"), "0,298");
    await user.click(submitButton());

    expect(createMock).toHaveBeenCalledWith({
      brand: "Shure",
      name: "SM58",
      categoryId: "c-2",
      trackingType: "serialized",
      weightKilograms: "0.298",
      powerWatts: null,
      transportVolumeCubicMeters: null,
    });
    expect(onSaved).toHaveBeenCalledWith({ id: "m-1" });
  });

  it("keeps the tracking type of a model with stock and shows a taken name under the name", async () => {
    vi.mocked(getEquipmentModel).mockResolvedValue({
      id: "m-1",
      brand: "Shure",
      name: "SM58",
      categoryId: "c-2",
      categoryPath: ["Ses", "Mikrofon"],
      trackingType: "serialized",
      weightKilograms: null,
      powerWatts: null,
      transportVolumeCubicMeters: null,
      hasStock: true,
      kits: [],
      deactivatedAt: null,
      createdAt: "2027-01-04T06:00:00+00:00",
      updatedAt: "2027-01-04T06:00:00+00:00",
      version: 2,
    });

    renderPage("m-1");

    expect(
      await screen.findByText(/stoğu oluştuğu için takip tipi değiştirilemez/u),
    ).toBeInTheDocument();
    expect(screen.getByLabelText("Takip tipi *")).toBeDisabled();
  });

  it("shows the server's refusal of a taken brand and name under the name", async () => {
    createMock.mockRejectedValue(anApiError(422, "BR-EQP-012"));
    const user = userEvent.setup();
    renderPage();

    await user.type(await screen.findByLabelText("Marka *"), "Shure");
    await user.type(screen.getByLabelText("Model adı *"), "SM58");
    await user.click(screen.getByLabelText("Kategori *"));
    await user.click(await screen.findByRole("option", { name: "Ses › Mikrofon" }));
    await user.click(submitButton());

    expect(
      await screen.findByText(/marka ve model adıyla bir model zaten var/u),
    ).toBeInTheDocument();
  });
});
