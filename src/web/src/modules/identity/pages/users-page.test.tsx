import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { listUsers } from "@/api/endpoints/identity/identity";
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
}));

const listUsersMock = vi.mocked(listUsers);

function aListedUser(overrides: Partial<UserListItem>): UserListItem {
  return {
    id: crypto.randomUUID(),
    fullName: "Ali Bal",
    email: "ali@example.com",
    roles: ["bookingManager"],
    isActive: true,
    lockedUntil: null,
    ...overrides,
  };
}

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
});
