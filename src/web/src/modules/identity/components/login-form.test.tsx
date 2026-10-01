import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { login } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { LoginForm } from "./login-form";

vi.mock("@/api/endpoints/identity/identity", () => ({ login: vi.fn() }));

const loginMock = vi.mocked(login);

function renderForm(onSignedIn = vi.fn()) {
  render(
    <QueryClientProvider client={createQueryClient()}>
      <LoginForm onSignedIn={onSignedIn} />
    </QueryClientProvider>,
  );
  return onSignedIn;
}

async function signIn(email: string, password: string) {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText("E-posta"), email);
  await user.type(screen.getByLabelText("Şifre"), password);
  await user.click(screen.getByRole("button", { name: "Giriş yap" }));
}

describe("LoginForm", () => {
  beforeEach(() => {
    loginMock.mockReset();
  });

  it("asks for both fields before sending anything", async () => {
    renderForm();

    await userEvent.setup().click(screen.getByRole("button", { name: "Giriş yap" }));

    expect(await screen.findAllByText("Bu alan zorunludur.")).toHaveLength(2);
    expect(screen.getByLabelText("E-posta")).toHaveFocus();
    expect(loginMock).not.toHaveBeenCalled();
  });

  it("hands over the signed-in user", async () => {
    const user = aUser({ mustChangePassword: true });
    loginMock.mockResolvedValue(user);
    const onSignedIn = renderForm();

    await signIn(" ayse@example.com ", "doğru-at-pil-zımba");

    await vi.waitFor(() => {
      expect(onSignedIn).toHaveBeenCalledWith(
        user,
        expect.anything(),
        undefined,
        expect.anything(),
      );
    });
    expect(loginMock).toHaveBeenCalledWith({
      email: "ayse@example.com",
      password: "doğru-at-pil-zımba",
    });
  });

  it("does not say which of the two was wrong", async () => {
    loginMock.mockRejectedValue(anApiError(401, "invalidCredentials"));
    renderForm();

    await signIn("ayse@example.com", "yanlış");

    expect(await screen.findByRole("alert")).toHaveTextContent("E-posta ya da şifre hatalı.");
  });

  it("says until when a locked account stays locked, in Istanbul time", async () => {
    loginMock.mockRejectedValue(
      anApiError(401, "BR-SYS-005", { lockedUntil: "2027-01-04T11:30:00+00:00" }),
    );
    renderForm();

    await signIn("ayse@example.com", "yanlış");

    expect(await screen.findByRole("alert")).toHaveTextContent("14:30 saatine kadar kilitli");
  });

  it("lets the password be shown to check it", async () => {
    renderForm();
    const password = screen.getByLabelText("Şifre");

    await userEvent.setup().click(screen.getByRole("button", { name: "Şifreyi göster" }));

    expect(password).toHaveAttribute("type", "text");
    expect(screen.getByRole("button", { name: "Şifreyi gizle" })).toHaveAttribute(
      "aria-pressed",
      "true",
    );
  });
});
