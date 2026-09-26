import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { HomePage } from "./home-page";

describe("HomePage", () => {
  it("shows the product name from the Turkish translations", () => {
    render(<HomePage />);

    expect(screen.getByRole("heading", { level: 1, name: "FestOS" })).toBeInTheDocument();
  });
});
