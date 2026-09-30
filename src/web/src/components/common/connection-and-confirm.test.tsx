import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";

import { ConfirmDialog } from "./confirm-dialog";
import { ConnectionIndicator, ConnectionIndicatorContent } from "./connection-indicator";

describe("ConnectionIndicatorContent", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it("shows nothing while connected", () => {
    const { container } = render(<ConnectionIndicatorContent state="connected" />);

    expect(container).toBeEmptyDOMElement();
  });

  it("warns about reconnecting only after two seconds", () => {
    vi.useFakeTimers();
    render(<ConnectionIndicatorContent state="reconnecting" />);
    expect(screen.queryByRole("status")).not.toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(2000);
    });
    expect(screen.getByRole("status")).toHaveTextContent("Bağlantı yeniden kuruluyor…");
  });

  it("says at once that the connection is gone", () => {
    render(<ConnectionIndicatorContent state="disconnected" />);

    expect(screen.getByRole("alert")).toHaveTextContent("Bağlantı yok.");
  });

  it("tells for a moment that the connection is back after a drop, not after the first connect", () => {
    vi.useFakeTimers();
    const { rerender } = render(<ConnectionIndicatorContent state="connecting" />);
    rerender(<ConnectionIndicatorContent state="connected" />);
    act(() => {
      vi.advanceTimersByTime(10);
    });
    expect(screen.queryByRole("status")).not.toBeInTheDocument();

    rerender(<ConnectionIndicatorContent state="reconnecting" />);
    rerender(<ConnectionIndicatorContent state="connected" />);
    act(() => {
      vi.advanceTimersByTime(10);
    });
    expect(screen.getByRole("status")).toHaveTextContent("Bağlantı yeniden kuruldu.");

    act(() => {
      vi.advanceTimersByTime(4000);
    });
    expect(screen.queryByRole("status")).not.toBeInTheDocument();
  });

  it("treats a browser that is offline as disconnected", () => {
    vi.spyOn(navigator, "onLine", "get").mockReturnValue(false);

    render(<ConnectionIndicator />);

    expect(screen.getByRole("alert")).toHaveTextContent("Bağlantı yok.");
  });
});

describe("ConfirmDialog", () => {
  const baseProps = {
    open: true,
    title: "Yaz Festivali 2027 iptal edilsin mi?",
    description: "Onaylı 42 rezervasyon serbest bırakılacak; bu geri alınamaz.",
    confirmLabel: "Etkinliği iptal et",
  };

  it("starts with focus on cancel and runs the named action only when asked", async () => {
    const onConfirm = vi.fn();
    render(<ConfirmDialog {...baseProps} onConfirm={onConfirm} onOpenChange={vi.fn()} />);

    expect(await screen.findByRole("alertdialog", { name: baseProps.title })).toBeInTheDocument();
    await vi.waitFor(() => {
      expect(screen.getByRole("button", { name: "Vazgeç" })).toHaveFocus();
    });

    await userEvent.click(screen.getByRole("button", { name: "Etkinliği iptal et" }));
    expect(onConfirm).toHaveBeenCalledWith(undefined);
  });

  it("needs the reason before confirming and passes it on", async () => {
    const onConfirm = vi.fn();
    render(
      <ConfirmDialog
        {...baseProps}
        onConfirm={onConfirm}
        onOpenChange={vi.fn()}
        reason={{ label: "İptal nedeni" }}
      />,
    );
    const confirm = await screen.findByRole("button", { name: "Etkinliği iptal et" });
    expect(confirm).toBeDisabled();

    await userEvent.type(screen.getByLabelText("İptal nedeni"), "  Sanatçı gelemiyor  ");
    await userEvent.click(confirm);

    expect(onConfirm).toHaveBeenCalledWith("Sanatçı gelemiyor");
  });
});
