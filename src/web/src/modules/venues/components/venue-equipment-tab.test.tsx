import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  addVenueEquipment,
  listUsableVenueEquipment,
  listVenueEquipment,
} from "@/api/endpoints/venues/venues";
import type { VenueEquipmentList } from "@/api/model";
import { ifMatch } from "@/lib/api-client";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { VenueEquipmentTab } from "./venue-equipment-tab";

vi.mock("@/api/endpoints/venues/venues", () => ({
  getGetVenueQueryKey: (id: string) => [`/api/v1/venues/${id}`],
  getListVenueEquipmentQueryKey: (id: string, params?: unknown) => [
    `/api/v1/venues/${id}/equipment`,
    ...(params === undefined ? [] : [params]),
  ],
  getListUsableVenueEquipmentQueryKey: (id: string, params?: unknown) => [
    `/api/v1/venues/${id}/equipment/usable`,
    ...(params === undefined ? [] : [params]),
  ],
  listVenueEquipment: vi.fn(),
  listUsableVenueEquipment: vi.fn(),
  addVenueEquipment: vi.fn(),
  editVenueEquipment: vi.fn(),
  removeVenueEquipment: vi.fn(),
  addVenueEquipmentUnavailability: vi.fn(),
  editVenueEquipmentUnavailability: vi.fn(),
  removeVenueEquipmentUnavailability: vi.fn(),
}));
vi.mock("@/api/endpoints/catalog/catalog", () => ({
  listEquipmentModels: vi.fn(),
  getListEquipmentModelsQueryKey: (params?: unknown) => ["/api/v1/equipment-models", params],
  listEquipmentCategories: vi.fn(),
  getListEquipmentCategoriesQueryKey: (params?: unknown) => [
    "/api/v1/equipment-categories",
    params,
  ],
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const technicalManager = ["Venues.Venues.View", "Venues.Equipment.Edit"];

const equipment: VenueEquipmentList = {
  lines: [
    {
      id: "e-1",
      modelId: "m-1",
      categoryId: null,
      description: null,
      name: "Shure SM58",
      categoryPath: ["Ses", "Mikrofon"],
      isTargetActive: false,
      isCounted: true,
      quantity: 12,
      validityStart: "2027-01-01",
      validityEnd: "2028-01-01",
      unavailabilities: [
        {
          id: "u-1",
          periodStart: "2027-03-10",
          periodEnd: "2027-03-13",
          quantity: 2,
          reason: "Bakımda",
        },
      ],
    },
    {
      id: "e-2",
      modelId: null,
      categoryId: null,
      description: "Sahne perdesi",
      name: "Sahne perdesi",
      categoryPath: [],
      isTargetActive: true,
      isCounted: false,
      quantity: 1,
      validityStart: null,
      validityEnd: null,
      unavailabilities: [],
    },
  ],
  version: 7,
};

function renderTab(permissions = technicalManager) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <VenueEquipmentTab venueId="v-1" />
    </QueryClientProvider>,
  );
}

describe("VenueEquipmentTab", () => {
  beforeEach(() => {
    vi.mocked(addVenueEquipment).mockReset();
    vi.mocked(listVenueEquipment).mockResolvedValue(equipment);
  });

  it("shows the lines with their last days included and what they mean for counting", async () => {
    renderTab();

    expect(await screen.findByText("Shure SM58")).toBeInTheDocument();
    expect(screen.getByText("01.01.2027 – 31.12.2027")).toBeInTheDocument();
    expect(screen.getByText("10.03.2027 – 12.03.2027: 2 adet, Bakımda")).toBeInTheDocument();
    expect(screen.getByText("Katalogda pasif")).toBeInTheDocument();
    expect(screen.getByText("Hesaba girmez")).toBeInTheDocument();
    expect(screen.getByText("Süresiz")).toBeInTheDocument();
  });

  it("adds equipment the catalog lacks, sending the day after the last day on the venue's version", async () => {
    vi.mocked(addVenueEquipment).mockResolvedValue(equipment);
    const user = userEvent.setup();
    renderTab();

    await user.click(await screen.findByRole("button", { name: "Satır ekle" }));
    await user.click(screen.getByRole("combobox", { name: "Satır türü *" }));
    await user.click(await screen.findByRole("option", { name: "Açıklama" }));
    await user.type(screen.getByLabelText("Açıklama *"), "Yedek kürsü");
    await user.clear(screen.getByLabelText("Adet *"));
    await user.type(screen.getByLabelText("Adet *"), "2");
    const dialog = screen.getByRole("dialog");
    await user.type(within(dialog).getByLabelText("Son gün"), "2027-06-30");
    await user.click(within(dialog).getByRole("button", { name: "Ekle" }));

    expect(addVenueEquipment).toHaveBeenCalledWith(
      "v-1",
      {
        modelId: null,
        categoryId: null,
        description: "Yedek kürsü",
        quantity: 2,
        validityStart: null,
        validityEnd: "2027-07-01",
      },
      ifMatch(7),
    );
  });

  it("asks for the usable quantities with the day after the last day", async () => {
    vi.mocked(listUsableVenueEquipment).mockResolvedValue([
      { id: "e-1", name: "Shure SM58", isCounted: true, quantity: 12, usableQuantity: 10 },
    ]);
    const user = userEvent.setup();
    renderTab();

    await user.type(await screen.findByLabelText("İlk gün"), "2027-03-11");
    await user.type(screen.getByLabelText("Son gün"), "2027-03-12");
    await user.click(screen.getByRole("button", { name: "Göster" }));

    expect(await screen.findByText("10 / 12")).toBeInTheDocument();
    expect(listUsableVenueEquipment).toHaveBeenCalledWith(
      "v-1",
      { from: "2027-03-11", to: "2027-03-13" },
      expect.anything(),
    );
  });

  it("hides the editing actions without the equipment permission", async () => {
    renderTab(["Venues.Venues.View"]);

    expect(await screen.findByText("Shure SM58")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Satır ekle" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Dönem ekle" })).not.toBeInTheDocument();
  });
});
