import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  addContactPerson,
  getParty,
  listParties,
  removeRepresentation,
} from "@/api/endpoints/parties/parties";
import type { PartyDetails } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { PartyDetailPage, type PartyDetailTab } from "./party-detail-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/parties/parties", () => ({
  getParty: vi.fn(),
  getGetPartyQueryKey: (id: string) => [`/api/v1/parties/${id}`],
  getListPartiesQueryKey: (params?: unknown) => ["/api/v1/parties", params],
  listParties: vi.fn(),
  editParty: vi.fn(),
  activateParty: vi.fn(),
  deactivateParty: vi.fn(),
  addContactPerson: vi.fn(),
  editContactPerson: vi.fn(),
  removeContactPerson: vi.fn(),
  addRepresentation: vi.fn(),
  editRepresentation: vi.fn(),
  removeRepresentation: vi.fn(),
}));
vi.mock("@/api/endpoints/audit/audit", () => ({
  listAuditEntries: vi.fn(() => Promise.resolve({ items: [], nextCursor: null })),
  getListAuditEntriesQueryKey: (params: unknown) => ["/api/v1/audit-entries", params],
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const getMock = vi.mocked(getParty);
const listMock = vi.mocked(listParties);
const addContactMock = vi.mocked(addContactPerson);
const removeRepresentationMock = vi.mocked(removeRepresentation);
const manager = ["Parties.Parties.View", "Parties.Parties.Edit", "Parties.Parties.Deactivate"];

function aParty(overrides: Partial<PartyDetails> = {}): PartyDetails {
  return {
    id: "p-1",
    kind: "organization",
    name: "Açıkhava İşletme",
    firstName: null,
    lastName: null,
    legalName: "Açıkhava İşletme A.Ş.",
    roles: ["venueOperator"],
    contactPoints: [
      { id: "c-1", kind: "phone", value: "0212 111 22 33", label: "Santral", isPrimary: true },
    ],
    contactPersons: [],
    representations: [],
    representedArtists: [],
    employers: [],
    deactivatedAt: null,
    createdAt: "2027-01-04T06:00:00+00:00",
    updatedAt: "2027-01-04T06:00:00+00:00",
    version: 3,
    ...overrides,
  };
}

function renderPage(tab: PartyDetailTab = "general", permissions = manager) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <PartyDetailPage partyId="p-1" tab={tab} />
    </QueryClientProvider>,
  );
}

describe("PartyDetailPage", () => {
  beforeEach(() => {
    getMock.mockReset();
    listMock.mockReset();
    addContactMock.mockReset();
    removeRepresentationMock.mockReset();
  });

  it("shows the party's names, roles and contact points", async () => {
    getMock.mockResolvedValue(aParty());

    renderPage();

    expect(await screen.findByRole("heading", { name: "Açıkhava İşletme" })).toBeInTheDocument();
    expect(screen.getByText("Firma · Mekan işletmecisi")).toBeInTheDocument();
    expect(screen.getByText("0212 111 22 33")).toBeInTheDocument();
    expect(screen.getByText("(Santral)")).toBeInTheDocument();
    expect(screen.getByText("Bu firmaya bağlı iletişim kişisi yok.")).toBeInTheDocument();
  });

  it("ties a person picked by search to the organization on its version", async () => {
    getMock.mockResolvedValue(aParty());
    listMock.mockResolvedValue({
      items: [
        {
          id: "p-2",
          kind: "person",
          name: "Şebnem Ilgaz",
          roles: ["contact"],
          primaryPhone: "0532 111 22 33",
          primaryEmail: null,
          isActive: true,
          version: 1,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
    });
    addContactMock.mockResolvedValue(
      aParty({
        contactPersons: [
          {
            id: "oc-1",
            personId: "p-2",
            name: "Şebnem Ilgaz",
            title: "Teknik sorumlu",
            primaryPhone: "0532 111 22 33",
            primaryEmail: null,
            isActive: true,
          },
        ],
        version: 4,
      }),
    );
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "İletişim kişisi ekle" }));
    await user.click(screen.getByRole("combobox", { name: "Kişi *" }));
    await user.click(await screen.findByRole("option", { name: /Şebnem Ilgaz/u }));
    await user.type(screen.getByLabelText("Görevi"), "Teknik sorumlu");
    await user.click(screen.getByRole("button", { name: "Ekle" }));

    expect(addContactMock).toHaveBeenCalledWith(
      "p-1",
      { personId: "p-2", title: "Teknik sorumlu" },
      { headers: { "If-Match": '"3"' } },
    );
    expect(listMock).toHaveBeenCalledWith(
      expect.objectContaining({ kind: "person", status: "active" }),
      expect.anything(),
    );
    const row = await screen.findByRole("row", { name: /Teknik sorumlu/u });
    expect(within(row).getByRole("link", { name: "Şebnem Ilgaz" })).toHaveAttribute(
      "href",
      "/parties/p-2",
    );
  });

  it("removes an agency after a confirmation and shows the server's refusal", async () => {
    getMock.mockResolvedValue(
      aParty({
        kind: "person",
        name: "Tarkan",
        roles: ["artist"],
        representations: [
          {
            id: "r-1",
            agencyId: "p-3",
            agencyName: "Sahne Ajans",
            description: "Avrupa",
            isActive: true,
          },
        ],
      }),
    );
    removeRepresentationMock.mockRejectedValue(anApiError(412, "versionMismatch"));
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Kaldır" }));
    await user.click(
      within(await screen.findByRole("alertdialog")).getByRole("button", { name: "Kaldır" }),
    );

    expect(removeRepresentationMock).toHaveBeenCalledWith("p-1", "r-1", {
      headers: { "If-Match": '"3"' },
    });
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage("general", []);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(getMock).not.toHaveBeenCalled();
  });
});
