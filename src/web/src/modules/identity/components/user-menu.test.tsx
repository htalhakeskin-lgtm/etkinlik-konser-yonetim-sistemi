import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { logout } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { aUser } from "@/test/identity-fixtures";

import { UserMenu } from "./user-menu";

vi.mock("@/api/endpoints/identity/identity", () => ({
  logout: vi.fn(),
  changeMyPassword: vi.fn(),
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
const navigate = vi.fn();
vi.mock("@tanstack/react-router", () => ({ useNavigate: () => navigate }));

function renderMenu(user = aUser()) {
  render(
    <QueryClientProvider client={createQueryClient()}>
      <UserMenu user={user} />
    </QueryClientProvider>,
  );
}

async function openMenu() {
  const user = userEvent.setup();
  await user.click(screen.getByRole("button", { name: "Kullanıcı menüsü: Ayşe Kaya" }));
  return user;
}

describe("UserMenu", () => {
  beforeEach(() => {
    navigate.mockReset();
    vi.mocked(logout).mockReset();
  });

  it("shows the name and the roles in Turkish", async () => {
    renderMenu(aUser({ roles: ["bookingManager", "technicalManager"] }));

    await openMenu();

    expect(await screen.findByText("Booking müdürü, Teknik müdür")).toBeInTheDocument();
    expect(screen.queryByText("Salt okunur erişim")).not.toBeInTheDocument();
  });

  it("tells a general manager the access is read-only", async () => {
    renderMenu(aUser({ roles: ["generalManager"] }));

    await openMenu();

    expect(await screen.findByText("Salt okunur erişim")).toBeInTheDocument();
  });

  it("opens the change password dialog", async () => {
    renderMenu();

    const user = await openMenu();
    await user.click(await screen.findByRole("menuitem", { name: "Şifreyi değiştir" }));

    expect(await screen.findByRole("dialog", { name: "Şifreyi değiştir" })).toBeInTheDocument();
  });

  it("signs out and goes to the sign-in screen", async () => {
    vi.mocked(logout).mockResolvedValue();
    renderMenu();

    const user = await openMenu();
    await user.click(await screen.findByRole("menuitem", { name: "Çıkış yap" }));

    await vi.waitFor(() => {
      expect(navigate).toHaveBeenCalledWith({ to: "/login", replace: true });
    });
    expect(logout).toHaveBeenCalledOnce();
  });
});
