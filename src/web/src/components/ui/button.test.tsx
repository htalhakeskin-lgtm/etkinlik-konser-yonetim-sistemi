import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { Button } from "./button";

// Behaviour added to the copied shadcn/ui button (docs/standards/ui.md §6.3).
describe("Button", () => {
  it("runs the action when clicked", async () => {
    const handleClick = vi.fn();
    render(<Button onClick={handleClick}>Kaydet</Button>);

    await userEvent.click(screen.getByRole("button", { name: "Kaydet" }));

    expect(handleClick).toHaveBeenCalledOnce();
  });

  it("blocks a second submit while loading and keeps its name and focus", async () => {
    const handleClick = vi.fn();
    render(
      <Button isLoading onClick={handleClick}>
        Kaydet
      </Button>,
    );
    const button = screen.getByRole("button", { name: "Kaydet" });

    await userEvent.click(button);
    button.focus();

    expect(handleClick).not.toHaveBeenCalled();
    expect(button).toHaveAttribute("aria-busy", "true");
    expect(button).toHaveAttribute("aria-disabled", "true");
    expect(button).toHaveFocus();
  });
});
