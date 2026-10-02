import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createParty, listParties } from "@/api/endpoints/parties/parties";
import type { PartyDetails, PartyListItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { PartiesPage, type PartiesSearch, type PartiesVariant } from "./parties-page";

vi.mock("@tanstack/react-router", async () => ({
  Link: (await import("@/test/router-fakes")).FakeLink,
}));
vi.mock("@/api/endpoints/parties/parties", () => ({
  listParties: vi.fn(),
  getListPartiesQueryKey: (params: unknown) => ["/api/v1/parties", params],
  getParty: vi.fn(),
  getGetPartyQueryKey: (id: string) => [`/api/v1/parties/${id}`],
  createParty: vi.fn(),
  editParty: vi.fn(),
  activateParty: vi.fn(),
  deactivateParty: vi.fn(),
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listMock = vi.mocked(listParties);
const createMock = vi.mocked(createParty);
const manager = [
  "Parties.Parties.View",
  "Parties.Parties.Create",
  "Parties.Parties.Edit",
  "Parties.Parties.Deactivate",
];

function aParty(overrides: Partial<PartyListItem> = {}): PartyListItem {
  return {
    id: "p-1",
    kind: "organization",
    name: "Işık Ses",
    roles: ["supplier", "venueOperator"],
    primaryPhone: "0212 111 22 33",
    primaryEmail: null,
    isActive: true,
    version: 1,
    ...overrides,
  };
}

function renderPage(
  search: PartiesSearch = {},
  permissions = manager,
  variant: PartiesVariant = "parties",
) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  const onSearchChange = vi.fn();
  render(
    <QueryClientProvider client={client}>
      <PartiesPage search={search} onSearchChange={onSearchChange} variant={variant} />
    </QueryClientProvider>,
  );
  return { onSearchChange };
}

describe("PartiesPage", () => {
  beforeEach(() => {
    listMock.mockReset();
    createMock.mockReset();
    vi.mocked(toast.success).mockReset();
  });

  it("lists parties with their kind, roles and primary contact points", async () => {
    listMock.mockResolvedValue({ items: [aParty()], page: 1, pageSize: 25, totalCount: 1 });

    renderPage({ role: "supplier" });

    const row = await screen.findByRole("row", { name: /Işık Ses/u });
    expect(within(row).getByText("Firma")).toBeInTheDocument();
    expect(within(row).getByText("Tedarikçi")).toBeInTheDocument();
    expect(within(row).getByText("Mekan işletmecisi")).toBeInTheDocument();
    expect(within(row).getByText("0212 111 22 33")).toBeInTheDocument();
    expect(listMock).toHaveBeenCalledWith(
      expect.objectContaining({ role: "supplier", page: 1, pageSize: 25 }),
      expect.anything(),
    );
  });

  it("creates a person with a phone, suggesting the shown name from the first and last name", async () => {
    listMock.mockResolvedValue({ items: [], page: 1, pageSize: 25, totalCount: 0 });
    createMock.mockResolvedValue({ id: "p-2" } as PartyDetails);
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole("button", { name: "Taraf ekle" }));
    await user.click(screen.getByLabelText("Tür *"));
    await user.click(await screen.findByRole("option", { name: "Kişi" }));
    await user.type(screen.getByLabelText("Ad *"), "Şebnem");
    await user.type(screen.getByLabelText("Soyad *"), "Ilgaz");
    await user.tab();
    await user.click(screen.getByRole("checkbox", { name: "İletişim kişisi" }));
    await user.click(screen.getByRole("button", { name: "İletişim bilgisi ekle" }));
    await user.type(screen.getByLabelText("1. iletişim bilgisi"), "0532 111 22 33");
    await user.click(screen.getByRole("button", { name: "Oluştur" }));

    expect(createMock).toHaveBeenCalledWith({
      kind: "person",
      name: "Şebnem Ilgaz",
      firstName: "Şebnem",
      lastName: "Ilgaz",
      legalName: null,
      roles: ["contact"],
      contactPoints: [
        { id: null, kind: "phone", value: "0532 111 22 33", label: null, isPrimary: false },
      ],
    });
    expect(toast.success).toHaveBeenCalledWith("Taraf oluşturuldu.");
  });

  it("shows the rule under the roles when the server refuses a party without one", async () => {
    listMock.mockResolvedValue({ items: [], page: 1, pageSize: 25, totalCount: 0 });
    createMock.mockRejectedValue(anApiError(422, "BR-PTY-001"));
    const user = userEvent.setup();
    renderPage({ role: "supplier" });

    await user.click(await screen.findByRole("button", { name: "Taraf ekle" }));
    await user.type(screen.getByLabelText("Ad *"), "Işık Ses");
    await user.click(screen.getByRole("button", { name: "Oluştur" }));

    expect(await screen.findByText(/en az bir rolü olmalıdır/u)).toBeInTheDocument();
    expect(createMock).toHaveBeenCalledWith(expect.objectContaining({ roles: ["supplier"] }));
  });

  it("lists only artists on the artists screen and starts a new one with the artist role", async () => {
    listMock.mockResolvedValue({
      items: [aParty({ id: "p-9", kind: "person", name: "Tarkan", roles: ["artist"] })],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    const user = userEvent.setup();
    renderPage({}, manager, "artists");

    expect(await screen.findByRole("link", { name: "Tarkan" })).toHaveAttribute(
      "href",
      "/artists/p-9",
    );
    expect(screen.getByRole("heading", { name: "Sanatçılar" })).toBeInTheDocument();
    expect(screen.queryByRole("combobox", { name: "Rol" })).not.toBeInTheDocument();
    expect(listMock).toHaveBeenCalledWith(
      expect.objectContaining({ role: "artist" }),
      expect.anything(),
    );
    await user.click(screen.getByRole("button", { name: "Sanatçı ekle" }));
    expect(screen.getByRole("checkbox", { name: "Sanatçı" })).toBeChecked();
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, []);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listMock).not.toHaveBeenCalled();
  });
});
