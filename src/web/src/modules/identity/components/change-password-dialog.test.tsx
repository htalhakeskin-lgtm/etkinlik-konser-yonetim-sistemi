import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { toast } from "sonner";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { changeMyPassword } from "@/api/endpoints/identity/identity";
import { ApiError } from "@/lib/api-error";
import { createQueryClient } from "@/lib/query-client";
import { aUser } from "@/test/identity-fixtures";

import { meQuery } from "../session";
import { ChangePasswordDialog } from "./change-password-dialog";

vi.mock("@/api/endpoints/identity/identity", () => ({
  changeMyPassword: vi.fn(),
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn() } }));

const changeMock = vi.mocked(changeMyPassword);

function renderDialog(onOpenChange = vi.fn()) {
  const client = createQueryClient();
  render(
    <QueryClientProvider client={client}>
      <ChangePasswordDialog open onOpenChange={onOpenChange} />
    </QueryClientProvider>,
  );
  return { client, onOpenChange };
}

async function submit(current: string, next: string) {
  const user = userEvent.setup();
  await user.type(screen.getByLabelText("Mevcut şifre"), current);
  await user.type(screen.getByLabelText("Yeni şifre"), next);
  await user.click(screen.getByRole("button", { name: "Şifreyi kaydet" }));
}

describe("ChangePasswordDialog", () => {
  beforeEach(() => {
    changeMock.mockReset();
    vi.mocked(toast.success).mockReset();
  });

  it("asks for the current password again", async () => {
    const changed = aUser();
    changeMock.mockResolvedValue(changed);
    const { client, onOpenChange } = renderDialog();

    await submit("doğru-at-pil-zımba", "mavi-kalem-uzun-yol");

    await vi.waitFor(() => {
      expect(onOpenChange).toHaveBeenCalledWith(false);
    });
    expect(changeMock).toHaveBeenCalledWith({
      currentPassword: "doğru-at-pil-zımba",
      newPassword: "mavi-kalem-uzun-yol",
    });
    expect(client.getQueryData(meQuery.queryKey)).toEqual(changed);
    expect(toast.success).toHaveBeenCalledWith("Şifreniz değiştirildi.");
  });

  it("shows a wrong current password under its field", async () => {
    changeMock.mockRejectedValue(
      new ApiError({
        status: 400,
        code: "validation",
        errors: [{ pointer: "/currentPassword", code: "incorrectPassword", params: {} }],
      }),
    );
    renderDialog();

    await submit("yanlış-şifre-yazıldı", "mavi-kalem-uzun-yol");

    expect(await screen.findByText("Şifre hatalı.")).toBeInTheDocument();
    expect(screen.getByLabelText("Mevcut şifre")).toHaveAttribute("aria-invalid", "true");
  });

  it("names the broken rule under the new password", async () => {
    changeMock.mockRejectedValue(
      new ApiError({ status: 422, code: "BR-SYS-007", params: { reason: "whitespace" } }),
    );
    renderDialog();

    await submit("doğru-at-pil-zımba", "boşluklu şifre");

    expect(await screen.findByText("Şifre boşluk içeremez.")).toBeInTheDocument();
  });
});
