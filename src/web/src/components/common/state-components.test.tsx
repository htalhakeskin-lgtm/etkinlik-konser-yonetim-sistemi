import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";

import { ApiError } from "@/lib/api-error";

import { EmptyState } from "./empty-state";
import { ErrorState } from "./error-state";
import { PageHeader } from "./page-header";
import { SkeletonBlock } from "./skeleton-block";

describe("EmptyState", () => {
  it("says what will appear and offers the next step", () => {
    render(
      <EmptyState
        title="Henüz etkinlik yok."
        description="Oluşturduğunuz etkinlikler burada listelenir."
        action={<button type="button">Etkinlik oluştur</button>}
      />,
    );

    expect(screen.getByText("Henüz etkinlik yok.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Etkinlik oluştur" })).toBeInTheDocument();
  });
});

describe("ErrorState", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it("explains the failure and lets the user try again", async () => {
    const onRetry = vi.fn();
    render(
      <ErrorState error={ApiError.network(new TypeError("Failed to fetch"))} onRetry={onRetry} />,
    );

    expect(screen.getByRole("alert")).toHaveTextContent("Bu bölüm yüklenemedi.");
    expect(screen.getByRole("alert")).toHaveTextContent("Sunucuya ulaşılamadı.");
    await userEvent.click(screen.getByRole("button", { name: "Yeniden dene" }));
    expect(onRetry).toHaveBeenCalledOnce();
  });

  it("lets the user copy the trace id from the details", async () => {
    const user = userEvent.setup();
    const writeText = vi.spyOn(navigator.clipboard, "writeText").mockResolvedValue();
    render(
      <ErrorState
        error={new ApiError({ status: 500, code: "internalError", traceId: "trace-42" })}
      />,
    );

    await user.click(screen.getByText("Ayrıntılar"));
    await user.click(screen.getByRole("button", { name: "Kopyala" }));

    expect(writeText).toHaveBeenCalledWith("trace-42");
    expect(await screen.findByRole("button", { name: "Kopyalandı" })).toBeInTheDocument();
  });

  it("has no details when there is no trace id", () => {
    render(<ErrorState error={new Error("boom")} />);

    expect(screen.queryByText("Ayrıntılar")).not.toBeInTheDocument();
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
  });
});

describe("SkeletonBlock", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it("keeps its place but shows only after 300 ms, hidden from assistive technology", () => {
    vi.useFakeTimers();
    const { container } = render(<SkeletonBlock className="h-6 w-40" />);
    const block = container.firstElementChild;

    expect(block).toHaveAttribute("aria-hidden", "true");
    expect(block).toHaveClass("invisible", "h-6", "w-40");

    act(() => {
      vi.advanceTimersByTime(300);
    });
    expect(block).not.toHaveClass("invisible");
  });
});

describe("PageHeader", () => {
  it("names the page with its only top-level heading, the path and the actions", () => {
    render(
      <PageHeader
        title="Yaz Festivali 2027"
        breadcrumb={<a href="/events">Etkinlikler</a>}
        actions={<button type="button">Onayla</button>}
      />,
    );

    expect(
      screen.getByRole("heading", { level: 1, name: "Yaz Festivali 2027" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("navigation", { name: "Sayfa yolu" })).toContainElement(
      screen.getByRole("link", { name: "Etkinlikler" }),
    );
    expect(screen.getByRole("button", { name: "Onayla" })).toBeInTheDocument();
  });
});
