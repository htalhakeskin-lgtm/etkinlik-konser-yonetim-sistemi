import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { editProduction, getProduction } from "@/api/endpoints/riders/riders";
import type { ProductionDetails } from "@/api/model";
import { ifMatch } from "@/lib/api-client";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { ProductionDetailPage } from "./production-detail-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/riders/riders", () => ({
  getProduction: vi.fn(),
  getGetProductionQueryKey: (id: string) => [`/api/v1/productions/${id}`],
  getListProductionsQueryKey: (params?: unknown) => ["/api/v1/productions", params],
  createProduction: vi.fn(),
  editProduction: vi.fn(),
  activateProduction: vi.fn(),
  deactivateProduction: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const booking = [
  "Riders.Productions.View",
  "Riders.Productions.Edit",
  "Riders.Productions.Deactivate",
];

const production: ProductionDetails = {
  id: "p-1",
  artistPartyId: "a-1",
  artistName: "Gece Yolcuları",
  isArtistActive: false,
  name: "Gece Turnesi 2027",
  description: "Büyük salon turnesi",
  riderId: "r-1",
  riderVersion: 1,
  latestVersionNumber: 0,
  deactivatedAt: null,
  createdAt: "2027-01-04T06:00:00+00:00",
  updatedAt: "2027-01-04T06:00:00+00:00",
  version: 4,
};

function renderPage(permissions = booking) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <ProductionDetailPage productionId="p-1" tab="rider" />
    </QueryClientProvider>,
  );
}

describe("ProductionDetailPage", () => {
  beforeEach(() => {
    vi.mocked(editProduction).mockReset();
    vi.mocked(getProduction).mockResolvedValue(production);
  });

  it("shows the production with its artist and an empty rider", async () => {
    renderPage();

    expect(await screen.findByRole("heading", { name: "Gece Turnesi 2027" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Gece Yolcuları" })).toHaveAttribute(
      "href",
      "/artists/a-1",
    );
    expect(screen.getByText("Pasif")).toBeInTheDocument();
    expect(screen.getByText("Henüz rider girilmedi")).toBeInTheDocument();
  });

  it("shows a name taken for the artist under the name field (BR-RDR-009)", async () => {
    vi.mocked(editProduction).mockRejectedValue(anApiError(422, "BR-RDR-009"));
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Düzenle" }));
    const dialog = screen.getByRole("dialog");
    await user.clear(within(dialog).getByLabelText("Ad *"));
    await user.type(within(dialog).getByLabelText("Ad *"), "Akustik");
    await user.click(within(dialog).getByRole("button", { name: "Kaydet" }));

    expect(editProduction).toHaveBeenCalledWith(
      "p-1",
      { name: "Akustik", description: "Büyük salon turnesi" },
      ifMatch(4),
    );
    expect(
      await within(dialog).findByText(/Bu sanatçının aynı adlı bir prodüksiyonu zaten var/u),
    ).toBeInTheDocument();
  });

  it("hides the changing actions from a reader", async () => {
    renderPage(["Riders.Productions.View"]);

    expect(await screen.findByRole("heading", { name: "Gece Turnesi 2027" })).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Düzenle" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Pasifleştir" })).not.toBeInTheDocument();
  });
});
