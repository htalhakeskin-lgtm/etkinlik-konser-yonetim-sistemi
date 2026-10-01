import { QueryClientProvider } from "@tanstack/react-query";
import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { login } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { reportSessionExpired } from "@/lib/session-expiry";
import { aUser } from "@/test/identity-fixtures";

import { meQuery } from "../session";
import { ReauthDialog } from "./reauth-dialog";

vi.mock("@/api/endpoints/identity/identity", () => ({
  login: vi.fn(),
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
const navigate = vi.fn();
vi.mock("@tanstack/react-router", () => ({ useNavigate: () => navigate }));

const loginMock = vi.mocked(login);

function renderDialog(signedIn = true) {
  const client = createQueryClient();
  if (signedIn) {
    client.setQueryData(meQuery.queryKey, aUser());
  }

  render(
    <QueryClientProvider client={client}>
      <ReauthDialog />
    </QueryClientProvider>,
  );
  return client;
}

async function signInAgain() {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText("Şifre"), "doğru-at-pil-zımba");
  await user.click(screen.getByRole("button", { name: "Giriş yap" }));
}

describe("ReauthDialog", () => {
  beforeEach(() => {
    loginMock.mockReset();
    navigate.mockReset();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("opens over the screen when the session ends, with the email filled in", async () => {
    renderDialog();

    act(() => {
      reportSessionExpired();
    });

    expect(await screen.findByRole("dialog", { name: "Oturumunuz sona erdi" })).toBeInTheDocument();
    expect(screen.getByLabelText("E-posta")).toHaveValue("ayse@example.com");
  });

  it("stays closed for a visitor who never signed in", () => {
    renderDialog(false);

    act(() => {
      reportSessionExpired();
    });

    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("closes when the same user signs in again, keeping the screen", async () => {
    loginMock.mockResolvedValue(aUser());
    renderDialog();
    act(() => {
      reportSessionExpired();
    });

    await signInAgain();

    await vi.waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
    expect(navigate).not.toHaveBeenCalled();
  });

  it("starts over with a fresh page for another user", async () => {
    const assign = vi.fn();
    vi.stubGlobal("location", { assign });
    loginMock.mockResolvedValue(aUser({ id: "0190f0c4-0000-7000-8000-000000000002" }));
    renderDialog();
    act(() => {
      reportSessionExpired();
    });

    await signInAgain();

    await vi.waitFor(() => {
      expect(assign).toHaveBeenCalledWith("/");
    });
  });
});
