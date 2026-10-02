import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { listParties } from "@/api/endpoints/parties/parties";
import { createVenue, listVenues } from "@/api/endpoints/venues/venues";
import type { VenueDetails } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { VenuesPage } from "./venues-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/venues/venues", () => ({
  listVenues: vi.fn(),
  getListVenuesQueryKey: (params?: unknown) => ["/api/v1/venues", params],
  getVenue: vi.fn(),
  getGetVenueQueryKey: (id: string) => [`/api/v1/venues/${id}`],
  createVenue: vi.fn(),
  editVenue: vi.fn(),
  activateVenue: vi.fn(),
  deactivateVenue: vi.fn(),
}));
vi.mock("@/api/endpoints/parties/parties", () => ({
  listParties: vi.fn(),
  getListPartiesQueryKey: (params?: unknown) => ["/api/v1/parties", params],
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listMock = vi.mocked(listVenues);
const createMock = vi.mocked(createVenue);
const manager = ["Venues.Venues.View", "Venues.Venues.Create", "Venues.Venues.Edit"];

function renderPage(permissions = manager) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <VenuesPage search={{}} onSearchChange={vi.fn()} />
    </QueryClientProvider>,
  );
}

describe("VenuesPage", () => {
  beforeEach(() => {
    listMock.mockReset();
    createMock.mockReset();
    vi.mocked(listParties).mockResolvedValue({
      items: [
        {
          id: "p-1",
          kind: "organization",
          name: "Açıkhava İşletme",
          roles: ["venueOperator"],
          primaryPhone: null,
          primaryEmail: null,
          isActive: true,
          version: 1,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
    });
  });

  it("lists venues with their city, capacity and operator", async () => {
    listMock.mockResolvedValue({
      items: [
        {
          id: "v-1",
          name: "Açıkhava Tiyatrosu",
          city: "İstanbul",
          capacity: 4000,
          operatorPartyId: "p-1",
          operatorName: "Açıkhava İşletme",
          isActive: true,
          version: 1,
        },
      ],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });

    renderPage();

    const row = await screen.findByRole("row", { name: /Açıkhava Tiyatrosu/u });
    expect(within(row).getByRole("link", { name: "Açıkhava Tiyatrosu" })).toHaveAttribute(
      "href",
      "/venues/v-1",
    );
    expect(within(row).getByText("4.000")).toBeInTheDocument();
    expect(within(row).getByText("Açıkhava İşletme")).toBeInTheDocument();
  });

  it("creates a venue with a picked operator, decimals as text and the curfew as a time", async () => {
    listMock.mockResolvedValue({ items: [], page: 1, pageSize: 25, totalCount: 0 });
    createMock.mockResolvedValue({ id: "v-2" } as VenueDetails);
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Mekan ekle" }));
    await user.type(screen.getByLabelText("Ad *"), "Açıkhava Tiyatrosu");
    await user.type(screen.getByLabelText("Şehir *"), "İstanbul");
    await user.type(screen.getByLabelText("Adres *"), "Harbiye");
    await user.type(screen.getByLabelText("Kapasite *"), "4000");
    await user.click(screen.getByRole("combobox", { name: "Mekan işletmecisi" }));
    await user.click(await screen.findByRole("option", { name: "Açıkhava İşletme" }));
    await user.type(screen.getByLabelText("Sahne genişliği (m)"), "18,5");
    await user.type(screen.getByLabelText("Sessizlik saati"), "23:00");
    await user.click(screen.getByRole("button", { name: "Oluştur" }));

    expect(createMock).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "Açıkhava Tiyatrosu",
        capacity: 4000,
        operatorPartyId: "p-1",
        stageWidthMeters: "18.5",
        stageDepthMeters: null,
        curfew: "23:00:00",
        timeZone: "Europe/Istanbul",
      }),
    );
  });

  it("shows a name taken in the city under the name", async () => {
    listMock.mockResolvedValue({ items: [], page: 1, pageSize: 25, totalCount: 0 });
    createMock.mockRejectedValue(anApiError(422, "BR-VEN-003"));
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Mekan ekle" }));
    await user.type(screen.getByLabelText("Ad *"), "Jolly Joker");
    await user.type(screen.getByLabelText("Şehir *"), "İstanbul");
    await user.type(screen.getByLabelText("Adres *"), "Kuruçeşme");
    await user.type(screen.getByLabelText("Kapasite *"), "1500");
    await user.click(screen.getByRole("button", { name: "Oluştur" }));

    expect(
      await screen.findByText(/Bu şehirde aynı adlı bir mekan zaten var/u),
    ).toBeInTheDocument();
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage([]);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
