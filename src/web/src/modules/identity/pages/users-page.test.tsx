import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { deactivateUser, listUsers, resetUserPassword } from "@/api/endpoints/identity/identity";
import type { UserListItem } from "@/api/model";
import { createQueryClient } from "@/lib/query-client";
import { aUser } from "@/test/identity-fixtures";

import { meQuery } from "../session";
import { UsersPage, type UsersSearch } from "./users-page";

vi.mock("@/api/endpoints/identity/identity", () => ({
  listUsers: vi.fn(),
  getListUsersQueryKey: (params: unknown) => ["/api/v1/users", params],
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
  getUser: vi.fn(),
  getGetUserQueryKey: (id: string) => [`/api/v1/users/${id}`],
  createUser: vi.fn(),
  editUser: vi.fn(),
  activateUser: vi.fn(),
  deactivateUser: vi.fn(),
  resetUserPassword: vi.fn(),
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));

const listUsersMock = vi.mocked(listUsers);

function aListedUser(overrides: Partial<UserListItem>): UserListItem {
  return {
    id: crypto.randomUUID(),
    fullName: "Ali Bal",
    email: "ali@example.com",
    roles: ["bookingManager"],
    isActive: true,
    lockedUntil: null,
    version: 1,
    ...overrides,
  };
}

const allPermissions = [
  "Identity.Users.View",
  "Identity.Users.Create",
  "Identity.Users.Edit",
  "Identity.Users.Deactivate",
  "Identity.Users.ResetPassword",
];

function renderPage(search: UsersSearch = {}, permissions = ["Identity.Users.View"]) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  const onSearchChange = vi.fn();
  render(
    <QueryClientProvider client={client}>
      <UsersPage search={search} onSearchChange={onSearchChange} />
    </QueryClientProvider>,
  );
  return onSearchChange;
}

describe("UsersPage", () => {
  beforeEach(() => {
    listUsersMock.mockReset();
  });

  it("lists the users with their roles in Turkish and their status", async () => {
    listUsersMock.mockResolvedValue({
      items: [
        aListedUser({ fullName: "Ali Bal", roles: ["bookingManager", "technicalManager"] }),
        aListedUser({ fullName: "Banu Can", isActive: false }),
        aListedUser({ fullName: "Cem Dal", lockedUntil: "2027-01-04T11:30:00+00:00" }),
      ],
      page: 1,
      pageSize: 25,
      totalCount: 3,
    });

    renderPage({ role: "bookingManager" });

    expect(await screen.findByText("Booking müdürü, Teknik müdür")).toBeInTheDocument();
    expect(screen.getByText("Pasif")).toBeInTheDocument();
    expect(screen.getByText("Kilitli (bitiş: 14:30)")).toBeInTheDocument();
    expect(listUsersMock).toHaveBeenCalledWith(
      { role: "bookingManager", page: 1, pageSize: 25 },
      expect.anything(),
    );
  });

  it("searches once typing pauses, starting from the first page", async () => {
    listUsersMock.mockResolvedValue({ items: [], page: 2, pageSize: 25, totalCount: 0 });
    const onSearchChange = renderPage({ page: 2 });

    await userEvent
      .setup()
      .type(screen.getByRole("searchbox", { name: "Ad ya da e-posta ara" }), "işık");

    await vi.waitFor(() => {
      expect(onSearchChange).toHaveBeenCalledWith({ q: "işık", page: undefined });
    });
    expect(await screen.findByText("Aramanıza uyan kullanıcı yok.")).toBeInTheDocument();
  });

  it("sorts on the server through the address", async () => {
    listUsersMock.mockResolvedValue({
      items: [aListedUser({})],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    const onSearchChange = renderPage({ sort: "fullName" });

    await userEvent.setup().click(await screen.findByRole("button", { name: /Ad soyad/u }));

    expect(onSearchChange).toHaveBeenCalledWith({ sort: "-fullName", page: undefined });
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage({}, ["Booking.Events.View"]);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listUsersMock).not.toHaveBeenCalled();
  });

  it("deactivates a user after asking, with the version the row showed", async () => {
    listUsersMock.mockResolvedValue({
      items: [aListedUser({ id: "u-1", fullName: "Ali Bal", version: 4 })],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    vi.mocked(deactivateUser).mockResolvedValue({} as never);
    renderPage({}, allPermissions);
    const user = userEvent.setup();

    await user.click(await screen.findByRole("button", { name: "Ali Bal için işlemler" }));
    await user.click(await screen.findByRole("menuitem", { name: "Pasifleştir" }));
    await user.click(await screen.findByRole("button", { name: "Pasifleştir" }));

    await vi.waitFor(() => {
      expect(toast.success).toHaveBeenCalledWith("Kullanıcı pasifleştirildi.");
    });
    expect(deactivateUser).toHaveBeenCalledWith("u-1", { headers: { "If-Match": '"4"' } });
  });

  it("shows the new temporary password once after a reset", async () => {
    listUsersMock.mockResolvedValue({
      items: [aListedUser({ id: "u-1", fullName: "Ali Bal" })],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    vi.mocked(resetUserPassword).mockResolvedValue({
      user: {} as never,
      temporaryPassword: "AAAA-BBBB-CCCC-DDDD",
    });
    renderPage({}, allPermissions);
    const user = userEvent.setup();

    await user.click(await screen.findByRole("button", { name: "Ali Bal için işlemler" }));
    await user.click(await screen.findByRole("menuitem", { name: "Şifreyi sıfırla" }));
    await user.click(await screen.findByRole("button", { name: "Şifreyi sıfırla" }));

    expect(await screen.findByText("AAAA-BBBB-CCCC-DDDD")).toBeInTheDocument();
    expect(screen.getByRole("dialog", { name: "Ali Bal için geçici şifre" })).toBeInTheDocument();
  });

  it("keeps the actions a user may not take out of the row's menu", async () => {
    listUsersMock.mockResolvedValue({
      items: [aListedUser({ fullName: "Ali Bal" })],
      page: 1,
      pageSize: 25,
      totalCount: 1,
    });
    renderPage({}, ["Identity.Users.View"]);

    expect(await screen.findByText("Ali Bal")).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Ali Bal için işlemler" })).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Kullanıcı ekle" })).not.toBeInTheDocument();
  });
});
