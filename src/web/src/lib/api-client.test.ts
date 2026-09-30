import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { apiClient } from "./api-client";
import { ApiError } from "./api-error";
import { reportServerVersion } from "./app-version";

vi.mock("./app-version", () => ({ reportServerVersion: vi.fn() }));

const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/;

type Handler = (url: string, init: RequestInit) => Response | Promise<Response>;

let fetchMock: ReturnType<typeof vi.fn<Handler>>;
// The headers of each call as they were sent; the client may reuse its Headers object afterwards.
let sentHeaders: Headers[];

function respondWith(...handlers: Handler[]) {
  for (const handler of handlers) {
    fetchMock.mockImplementationOnce(handler);
  }
}

function json(body: unknown, status = 200, contentType = "application/json"): Response {
  return new Response(JSON.stringify(body), { status, headers: { "Content-Type": contentType } });
}

function problem(status: number, code: string, extra: Record<string, unknown> = {}): Response {
  return json({ status, code, traceId: "trace-1", ...extra }, status, "application/problem+json");
}

function setTokenCookie(value: string) {
  document.cookie = `festos_xsrf=${value}; path=/`;
}

function headersOfCall(index: number): Headers {
  return sentHeaders[index] ?? new Headers();
}

describe("apiClient", () => {
  beforeEach(() => {
    fetchMock = vi.fn<Handler>();
    sentHeaders = [];
    vi.stubGlobal("fetch", (url: string, init: RequestInit) => {
      sentHeaders.push(new Headers(init.headers));
      return fetchMock(url, init);
    });
  });

  afterEach(() => {
    document.cookie = "festos_xsrf=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/";
    vi.useRealTimers();
  });

  it("returns the body of a query without the headers of changing requests", async () => {
    respondWith(() => json({ name: "Stage" }));

    const body = await apiClient<{ name: string }>("/api/v1/things/1");

    expect(body).toEqual({ name: "Stage" });
    expect(headersOfCall(0).has("Idempotency-Key")).toBe(false);
    expect(headersOfCall(0).has("X-XSRF-TOKEN")).toBe(false);
  });

  it("adds the antiforgery token and a new idempotency key to a changing request", async () => {
    setTokenCookie("token-1");
    respondWith(() => new Response(null, { status: 204 }));

    const body = await apiClient("/api/v1/things", { method: "POST", body: "{}" });

    expect(body).toBeUndefined();
    expect(headersOfCall(0).get("X-XSRF-TOKEN")).toBe("token-1");
    expect(headersOfCall(0).get("Idempotency-Key")).toMatch(uuidPattern);
  });

  it("keeps the idempotency key the caller passes for a retried action", async () => {
    setTokenCookie("token-1");
    respondWith(() => new Response(null, { status: 204 }));

    await apiClient("/api/v1/things", { method: "POST", headers: { "Idempotency-Key": "key-1" } });

    expect(headersOfCall(0).get("Idempotency-Key")).toBe("key-1");
  });

  it("takes a token first when the page has none", async () => {
    respondWith(
      () => {
        setTokenCookie("token-new");
        return new Response(null, { status: 204 });
      },
      () => new Response(null, { status: 204 }),
    );

    await apiClient("/api/v1/things", { method: "PUT" });

    expect(fetchMock.mock.calls[0]?.[0]).toBe("/api/v1/antiforgery");
    expect(headersOfCall(1).get("X-XSRF-TOKEN")).toBe("token-new");
  });

  it("sends once more with a new token when the server rejects the old one", async () => {
    setTokenCookie("token-old");
    respondWith(
      () => problem(403, "csrfRejected"),
      () => {
        setTokenCookie("token-new");
        return new Response(null, { status: 204 });
      },
      () => new Response(null, { status: 204 }),
    );

    await apiClient("/api/v1/things", { method: "DELETE" });

    expect(headersOfCall(0).get("X-XSRF-TOKEN")).toBe("token-old");
    expect(headersOfCall(2).get("X-XSRF-TOKEN")).toBe("token-new");
    expect(headersOfCall(2).get("Idempotency-Key")).toBe(headersOfCall(0).get("Idempotency-Key"));
  });

  it("sends a changing request that got no answer again with the same key", async () => {
    vi.useFakeTimers();
    setTokenCookie("token-1");
    respondWith(
      () => Promise.reject(new TypeError("Failed to fetch")),
      () => Promise.reject(new TypeError("Failed to fetch")),
      () => json({ id: "1" }, 201),
    );

    const result = apiClient<{ id: string }>("/api/v1/things", { method: "POST" });
    await vi.advanceTimersByTimeAsync(4000);

    await expect(result).resolves.toEqual({ id: "1" });
    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(headersOfCall(2).get("Idempotency-Key")).toBe(headersOfCall(0).get("Idempotency-Key"));
  });

  it("gives up after the retries with a network error", async () => {
    vi.useFakeTimers();
    setTokenCookie("token-1");
    fetchMock.mockRejectedValue(new TypeError("Failed to fetch"));

    const result = apiClient("/api/v1/things", { method: "POST" });
    const failure = expect(result).rejects.toMatchObject({ status: 0, code: "network" });
    await vi.advanceTimersByTimeAsync(4000);

    await failure;
    expect(fetchMock).toHaveBeenCalledTimes(3);
  });

  it("leaves retrying a query to TanStack Query", async () => {
    respondWith(() => Promise.reject(new TypeError("Failed to fetch")));

    await expect(apiClient("/api/v1/things")).rejects.toMatchObject({ code: "network" });
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });

  it("throws the problem's code, values, field errors and trace id", async () => {
    respondWith(() =>
      problem(422, "SAMPLE-001", {
        detail: "Sample rule violated.",
        params: { limit: 3 },
        errors: [{ pointer: "/name", code: "required", params: {} }],
      }),
    );

    const error = await apiClient("/api/v1/things/1").catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiError);
    expect(error).toMatchObject({
      status: 422,
      code: "SAMPLE-001",
      params: { limit: 3 },
      errors: [{ pointer: "/name", code: "required", params: {} }],
      traceId: "trace-1",
    });
  });

  it("reports the server's version from every response", async () => {
    respondWith(() => new Response("{}", { status: 200, headers: { "X-App-Version": "1.2.0" } }));

    await apiClient("/api/v1/things");

    expect(reportServerVersion).toHaveBeenCalledWith("1.2.0");
  });
});
