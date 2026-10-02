import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createProduction, listProductions } from "@/api/endpoints/riders/riders";
import type { ProductionDetails } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { ArtistProductions } from "./artist-productions";

const navigateMock = vi.fn();

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
  useNavigate: () => navigateMock,
}));
vi.mock("@/api/endpoints/riders/riders", () => ({
  listProductions: vi.fn(),
  getListProductionsQueryKey: (params?: unknown) => ["/api/v1/productions", params],
  getGetProductionQueryKey: (id: string) => [`/api/v1/productions/${id}`],
  createProduction: vi.fn(),
  editProduction: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const booking = ["Riders.Productions.View", "Riders.Productions.Create"];

const created: ProductionDetails = {
  id: "p-3",
  artistPartyId: "a-1",
  artistName: "Gece Yolcuları",
  isArtistActive: true,
  name: "Kış Turnesi",
  description: null,
  riderId: "r-3",
  riderVersion: 1,
  latestVersionNumber: 0,
  deactivatedAt: null,
  createdAt: "2027-01-04T06:00:00+00:00",
  updatedAt: "2027-01-04T06:00:00+00:00",
  version: 1,
};

function renderSection(permissions = booking, isArtistActive = true) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <ArtistProductions
        artistId="a-1"
        artistName="Gece Yolcuları"
        isArtistActive={isArtistActive}
      />
    </QueryClientProvider>,
  );
}

describe("ArtistProductions", () => {
  beforeEach(() => {
    navigateMock.mockReset();
    vi.mocked(createProduction).mockReset();
    vi.mocked(listProductions).mockResolvedValue({
      items: [
        {
          id: "p-1",
          artistPartyId: "a-1",
          artistName: "Gece Yolcuları",
          name: "Gece Turnesi 2027",
          latestVersionNumber: 2,
          latestVersionAt: "2027-01-05T09:00:00+00:00",
          isActive: true,
          version: 1,
        },
        {
          id: "p-2",
          artistPartyId: "a-1",
          artistName: "Gece Yolcuları",
          name: "Akustik",
          latestVersionNumber: 0,
          latestVersionAt: null,
          isActive: false,
          version: 2,
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 2,
    });
  });

  it("lists the artist's productions, inactive ones included, with their newest rider version", async () => {
    renderSection();

    expect(await screen.findByRole("link", { name: "Gece Turnesi 2027" })).toHaveAttribute(
      "href",
      "/productions/p-1",
    );
    expect(screen.getByText("v2")).toBeInTheDocument();
    expect(screen.getByText("Henüz rider girilmedi")).toBeInTheDocument();
    expect(screen.getByText("Pasif")).toBeInTheDocument();
    expect(listProductions).toHaveBeenCalledWith(
      { artistId: "a-1", status: "all", pageSize: 100 },
      expect.anything(),
    );
  });

  it("creates a production for the artist and opens its page", async () => {
    vi.mocked(createProduction).mockResolvedValue(created);
    const user = userEvent.setup();
    renderSection();

    await user.click(await screen.findByRole("button", { name: "Prodüksiyon ekle" }));
    const dialog = screen.getByRole("dialog");
    expect(within(dialog).getByText("Sanatçı: Gece Yolcuları")).toBeInTheDocument();
    await user.type(within(dialog).getByLabelText("Ad *"), " Kış Turnesi ");
    await user.click(within(dialog).getByRole("button", { name: "Oluştur" }));

    expect(createProduction).toHaveBeenCalledWith({
      artistPartyId: "a-1",
      name: "Kış Turnesi",
      description: null,
    });
    expect(navigateMock).toHaveBeenCalledWith({
      to: "/productions/$productionId",
      params: { productionId: "p-3" },
    });
  });

  it("offers no new production for an inactive artist", async () => {
    renderSection(booking, false);

    expect(await screen.findByText("Gece Turnesi 2027")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Prodüksiyon ekle" })).not.toBeInTheDocument();
  });
});
