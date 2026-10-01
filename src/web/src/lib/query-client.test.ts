import { afterEach, describe, expect, it, vi } from "vitest";

import { ApiError } from "./api-error";
import { createQueryClient } from "./query-client";
import { onSessionExpired } from "./session-expiry";

describe("createQueryClient", () => {
  const stops: (() => void)[] = [];

  afterEach(() => {
    stops.splice(0).forEach((stop) => {
      stop();
    });
  });

  function listen() {
    const listener = vi.fn();
    stops.push(onSessionExpired(listener));
    return listener;
  }

  it("reports an ended session when a query is answered 401, without asking again", async () => {
    const listener = listen();
    const queryFn = vi.fn(() =>
      Promise.reject(new ApiError({ status: 401, code: "unauthorized" })),
    );

    await createQueryClient()
      .query({ queryKey: ["things"], queryFn })
      .catch(() => undefined);

    expect(listener).toHaveBeenCalledOnce();
    expect(queryFn).toHaveBeenCalledOnce();
  });

  it("leaves a 401 alone when the call expects it, such as signing in", async () => {
    const listener = listen();
    const client = createQueryClient();
    const mutation = client.getMutationCache().build(client, {
      mutationFn: () => Promise.reject(new ApiError({ status: 401, code: "invalidCredentials" })),
      meta: { expectsUnauthorized: true },
    });

    await mutation.execute(undefined).catch(() => undefined);

    expect(listener).not.toHaveBeenCalled();
  });
});
