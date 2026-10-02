import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { editKit, getKit, listEquipmentModels } from "@/api/endpoints/catalog/catalog";
import type { KitDetails } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { KitDetailPage } from "./kit-detail-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/catalog/catalog", () => ({
  getKit: vi.fn(),
  getGetKitQueryKey: (id: string) => [`/api/v1/kits/${id}`],
  getListKitsQueryKey: (params?: unknown) => ["/api/v1/kits", params],
  listKits: vi.fn(),
  listEquipmentModels: vi.fn(),
  getListEquipmentModelsQueryKey: (params?: unknown) => ["/api/v1/equipment-models", params],
  editKit: vi.fn(),
  createKit: vi.fn(),
  activateKit: vi.fn(),
  deactivateKit: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const editMock = vi.mocked(editKit);
const manager = ["Catalog.Kits.View", "Catalog.Kits.Edit"];

const kit: KitDetails = {
  id: "k-1",
  name: "Küçük sahne ışık paketi",
  lines: [
    {
      id: "l-1",
      modelId: "m-1",
      subKitId: null,
      name: "Robe Spiider",
      quantity: 4,
      isActive: true,
    },
  ],
  contents: [{ modelId: "m-1", name: "Robe Spiider", quantity: 4 }],
  totals: {
    weightKilograms: "70.000",
    isWeightComplete: true,
    powerWatts: 2400,
    isPowerComplete: false,
  },
  deactivatedAt: null,
  createdAt: "2027-01-04T06:00:00+00:00",
  updatedAt: "2027-01-04T06:00:00+00:00",
  version: 3,
};

function renderPage() {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions: manager }));
  render(
    <QueryClientProvider client={client}>
      <KitDetailPage kitId="k-1" tab="general" />
    </QueryClientProvider>,
  );
}

describe("KitDetailPage", () => {
  beforeEach(() => {
    editMock.mockReset();
    vi.mocked(getKit).mockResolvedValue(kit);
  });

  it("shows the totals, marking the one with missing data", async () => {
    renderPage();

    expect(await screen.findByText("70 kg")).toBeInTheDocument();
    expect(screen.getByText("2.400 W")).toBeInTheDocument();
    expect(screen.getByText("eksik veri")).toBeInTheDocument();
  });

  it("adds a picked model as a line and saves the whole kit on its version", async () => {
    vi.mocked(listEquipmentModels).mockResolvedValue({
      items: [
        {
          id: "m-2",
          brand: "Klotz",
          name: "DMX 10 m",
          categoryId: "c-1",
          categoryPath: ["Işık", "Kablo"],
          trackingType: "bulk",
          weightKilograms: null,
          powerWatts: null,
          isActive: true,
          version: 1,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
    });
    editMock.mockResolvedValue({ ...kit, version: 4 });
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Satır ekle" }));
    await user.click(screen.getAllByRole("combobox").at(-1) ?? document.body);
    await user.click(await screen.findByRole("option", { name: /Klotz DMX 10 m/u }));
    await user.clear(screen.getByLabelText("2. satırın adedi"));
    await user.type(screen.getByLabelText("2. satırın adedi"), "8");
    await user.click(screen.getByRole("button", { name: "Satırları kaydet" }));

    expect(editMock).toHaveBeenCalledWith(
      "k-1",
      {
        name: "Küçük sahne ışık paketi",
        lines: [
          { modelId: "m-1", quantity: 4 },
          { modelId: "m-2", quantity: 8 },
        ],
      },
      { headers: { "If-Match": '"3"' } },
    );
  });

  it("does not send a line without a target", async () => {
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Satır ekle" }));
    await user.click(screen.getByRole("button", { name: "Satırları kaydet" }));

    expect(
      await screen.findByText(/İşaretli satırlarda model ya da kit seçin/u),
    ).toBeInTheDocument();
    expect(editMock).not.toHaveBeenCalled();
  });
});
