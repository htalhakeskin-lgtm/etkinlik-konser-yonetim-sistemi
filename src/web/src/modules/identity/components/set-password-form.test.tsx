import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { changeMyPassword } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { SetPasswordForm } from "./set-password-form";

vi.mock("@/api/endpoints/identity/identity", () => ({ changeMyPassword: vi.fn() }));

const changeMock = vi.mocked(changeMyPassword);

function renderForm(onChanged = vi.fn()) {
  render(
    <QueryClientProvider client={createQueryClient()}>
      <SetPasswordForm onChanged={onChanged} />
    </QueryClientProvider>,
  );
  return onChanged;
}

async function submit(password: string) {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText("Yeni şifre"), password);
  await user.click(screen.getByRole("button", { name: "Şifreyi kaydet" }));
}

describe("SetPasswordForm", () => {
  beforeEach(() => {
    changeMock.mockReset();
  });

  it("shows the rules under the field", () => {
    renderForm();

    expect(
      screen.getByText(/yaygın şifreler, e-postanız ve ürün adı kabul edilmez/),
    ).toBeInTheDocument();
  });

  it("sets the new password without asking for the temporary one", async () => {
    const user = aUser();
    changeMock.mockResolvedValue(user);
    const onChanged = renderForm();

    await submit("mavi-kalem-uzun-yol");

    await vi.waitFor(() => {
      expect(onChanged).toHaveBeenCalledWith(user, expect.anything(), undefined, expect.anything());
    });
    expect(changeMock).toHaveBeenCalledWith({
      currentPassword: null,
      newPassword: "mavi-kalem-uzun-yol",
    });
  });

  it.each([
    ["tooShort", "Şifre en az 15 karakter olmalıdır."],
    ["common", "Bu şifre çok yaygın; tahmin edilmesi kolay. Başka bir şifre seçin."],
    ["sameAsCurrent", "Yeni şifre mevcut şifrenizle aynı olamaz."],
  ])("names the broken rule %s under the field", async (reason, text) => {
    changeMock.mockRejectedValue(anApiError(422, "BR-SYS-007", { reason, minLength: 15 }));
    renderForm();

    await submit("kısa-bir-şifre");

    expect(await screen.findByText(text)).toBeInTheDocument();
    expect(screen.getByLabelText("Yeni şifre")).toHaveAttribute("aria-invalid", "true");
  });
});
