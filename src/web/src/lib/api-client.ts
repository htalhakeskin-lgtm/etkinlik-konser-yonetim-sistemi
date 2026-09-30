import { ApiError } from "./api-error";
import { reportServerVersion } from "./app-version";

// The only place the front end calls the API (api §14.2); every generated request goes through it as
// Orval's "mutator". It adds what the server requires of changing requests, compares the server's
// version and turns Problem Details into ApiError.

const antiforgeryHeader = "X-XSRF-TOKEN";
const antiforgeryPath = "/api/v1/antiforgery";
// The request token cookie; development serves plain HTTP, where it has no __Host- prefix (BB-11).
const tokenCookies = ["__Host-festos_xsrf", "festos_xsrf"];
const idempotencyHeader = "Idempotency-Key";
const changingMethods = new Set(["POST", "PUT", "PATCH", "DELETE"]);
// A changing request that got no answer is sent again with the same key, so it never runs twice
// (api §10). Queries are retried by TanStack Query.
const retryDelaysMs = [1000, 3000];

/**
 * Sends a request and returns its body. A changing request gets the antiforgery token and an
 * idempotency key; a caller that retries a user action itself passes the same key in the headers.
 * Throws {@link ApiError} for an error response and for a request that got no answer.
 */
export async function apiClient<T>(url: string, init: RequestInit = {}): Promise<T> {
  const method = (init.method ?? "GET").toUpperCase();
  const headers = new Headers(init.headers);
  const isChanging = changingMethods.has(method);

  if (isChanging) {
    if (!headers.has(idempotencyHeader)) {
      headers.set(idempotencyHeader, crypto.randomUUID());
    }
    headers.set(antiforgeryHeader, await requestToken(false));
  }

  let response = await send(url, { ...init, method, headers }, isChanging);
  if (isChanging && (await isForgeryRejection(response))) {
    // The token expired or belongs to an earlier session: take a new one and send once more.
    headers.set(antiforgeryHeader, await requestToken(true));
    response = await send(url, { ...init, method, headers }, isChanging);
  }

  reportServerVersion(response.headers.get("X-App-Version"));
  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function send(url: string, init: RequestInit, canRetry: boolean): Promise<Response> {
  for (let attempt = 0; ; attempt++) {
    try {
      return await fetch(url, { ...init, credentials: "same-origin" });
    } catch (error) {
      const delayMs = retryDelaysMs[attempt];
      if (isAbort(error)) {
        throw error;
      }

      if (!canRetry || delayMs === undefined || init.signal?.aborted === true) {
        throw ApiError.network(error);
      }

      await new Promise((resolve) => setTimeout(resolve, delayMs));
    }
  }
}

async function requestToken(isRefresh: boolean): Promise<string> {
  const current = isRefresh ? undefined : readTokenCookie();
  if (current !== undefined) {
    return current;
  }

  const response = await send(antiforgeryPath, { method: "GET" }, false);
  if (!response.ok) {
    throw await ApiError.fromResponse(response);
  }

  return readTokenCookie() ?? "";
}

function readTokenCookie(): string | undefined {
  for (const cookie of document.cookie.split(";")) {
    const separator = cookie.indexOf("=");
    const name = cookie.slice(0, separator).trim();
    if (separator > 0 && tokenCookies.includes(name)) {
      return decodeURIComponent(cookie.slice(separator + 1).trim());
    }
  }

  return undefined;
}

async function isForgeryRejection(response: Response): Promise<boolean> {
  if (response.status !== 403) {
    return false;
  }

  const error = await ApiError.fromResponse(response.clone());
  return error.code === "csrfRejected";
}

function isAbort(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
}
