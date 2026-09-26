import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { StatusBadge } from "./status-badge";
import { type StatusTone, statusToneIcons } from "./status-tone";

const tones = Object.keys(statusToneIcons) as StatusTone[];

describe("StatusBadge", () => {
  it.each(tones)("shows the name next to a decorative icon for the %s tone", (tone) => {
    render(<StatusBadge tone={tone} label="Onaylı" />);

    const badge = screen.getByText("Onaylı");
    expect(badge).toHaveAttribute("data-tone", tone);
    expect(badge.querySelector("svg")).toHaveAttribute("aria-hidden", "true");
  });

  it("gives every tone its own icon", () => {
    expect(new Set(Object.values(statusToneIcons)).size).toBe(tones.length);
  });
});
