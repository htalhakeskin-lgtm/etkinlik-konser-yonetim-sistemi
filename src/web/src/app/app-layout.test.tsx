import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { AppLayout } from "./app-layout";

function FakeOutlet() {
  return <p>sayfa içeriği</p>;
}

function FakeLink({ to, children }: { to: string; children?: ReactNode }) {
  return <a href={to}>{children}</a>;
}

vi.mock("@tanstack/react-router", () => ({
  Link: FakeLink,
  Outlet: FakeOutlet,
  useMatchRoute: () => () => false,
  useNavigate: () => vi.fn(),
}));
vi.mock("@/lib/realtime", () => ({
  RealtimeClient: class {
    start = () => Promise.resolve();
    stop = () => Promise.resolve();
    subscribe = () => () => undefined;
    getState = () => "connected";
    join = () => () => undefined;
  },
}));

describe("AppLayout", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "matchMedia",
      (query: string) =>
        ({
          matches: false,
          media: query,
          addEventListener: () => undefined,
          removeEventListener: () => undefined,
        }) as unknown as MediaQueryList,
    );
  });

  it("puts the page in the shell with the menu button and the user menu", () => {
    const client = createQueryClient();
    client.setQueryData(meQuery.queryKey, aUser());

    render(
      <QueryClientProvider client={client}>
        <AppLayout />
      </QueryClientProvider>,
    );

    expect(screen.getByText("sayfa içeriği")).toBeInTheDocument();
    // The top bar's button and the menu's edge both open and close the menu.
    expect(screen.getAllByRole("button", { name: "Menüyü aç ya da kapat" })).toHaveLength(2);
    expect(screen.getByRole("button", { name: "Kullanıcı menüsü: Ayşe Kaya" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Kullanıcılar" })).toHaveAttribute(
      "href",
      "/admin/users",
    );
  });
});
