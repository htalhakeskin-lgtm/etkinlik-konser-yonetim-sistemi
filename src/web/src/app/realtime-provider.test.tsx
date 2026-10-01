import { QueryClientProvider } from "@tanstack/react-query";
import { act, render } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";

import { createQueryClient } from "@/lib/query-client";
import type { RealtimeClient } from "@/lib/realtime";

import { RealtimeProvider } from "./realtime-provider";

function fakeClient(connect: () => Promise<void>) {
  const start = vi.fn(connect);
  const stop = vi.fn(() => Promise.resolve());
  return { client: { start, stop } as unknown as RealtimeClient, start, stop };
}

function renderProvider(client: RealtimeClient) {
  return render(
    <QueryClientProvider client={createQueryClient()}>
      <RealtimeProvider createClient={() => client}>
        <p>ekran</p>
      </RealtimeProvider>
    </QueryClientProvider>,
  );
}

describe("RealtimeProvider", () => {
  afterEach(() => {
    vi.useRealTimers();
  });

  it("connects with the shell and disconnects when the shell closes", () => {
    const { client, start, stop } = fakeClient(() => Promise.resolve());

    const { unmount } = renderProvider(client);
    unmount();

    expect(start).toHaveBeenCalledOnce();
    expect(stop).toHaveBeenCalledOnce();
  });

  it("tries a failed first connection again, later", async () => {
    vi.useFakeTimers();
    const { client, start } = fakeClient(() => Promise.reject(new Error("offline")));

    renderProvider(client);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(2000);
    });

    expect(start).toHaveBeenCalledTimes(2);
  });
});
