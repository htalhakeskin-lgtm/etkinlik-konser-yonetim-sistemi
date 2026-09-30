import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { VersionBanner, VersionBannerContent } from "./version-banner";

describe("VersionBanner", () => {
  it("shows nothing while the server runs the same release", () => {
    const { container } = render(<VersionBanner />);

    expect(container).toBeEmptyDOMElement();
  });

  it("offers to reload, and reloads only when asked", async () => {
    const onReload = vi.fn();
    render(<VersionBannerContent onReload={onReload} />);

    expect(screen.getByRole("status")).toHaveTextContent("Yeni sürüm hazır");
    expect(onReload).not.toHaveBeenCalled();

    await userEvent.click(screen.getByRole("button", { name: "Yenile" }));
    expect(onReload).toHaveBeenCalledOnce();
  });
});
