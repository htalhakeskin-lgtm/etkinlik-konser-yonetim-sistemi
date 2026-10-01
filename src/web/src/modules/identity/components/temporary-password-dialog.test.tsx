import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { TemporaryPasswordDialog } from "./temporary-password-dialog";

describe("TemporaryPasswordDialog", () => {
  it("shows the temporary password once and copies it", async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    render(
      <TemporaryPasswordDialog
        issued={{ fullName: "Ali Bal", temporaryPassword: "AAAA-BBBB-CCCC-DDDD" }}
        onClose={onClose}
      />,
    );

    await user.click(screen.getByRole("button", { name: "Kopyala" }));
    await user.click(screen.getByRole("button", { name: "Tamam" }));

    expect(await navigator.clipboard.readText()).toBe("AAAA-BBBB-CCCC-DDDD");
    expect(screen.getByText(/yalnızca şimdi gösterilir/u)).toBeInTheDocument();
    expect(onClose).toHaveBeenCalledOnce();
  });
});
