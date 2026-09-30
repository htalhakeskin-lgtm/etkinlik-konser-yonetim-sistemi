/**
 * The only place the front end calls the API (api §14.2): every generated request goes through it as
 * Orval's "mutator". The antiforgery token, idempotency keys, the version check and typed errors come
 * with the front-end platform (building-blocks §14, PR 13).
 */
export async function apiClient<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, { ...init, credentials: "same-origin" });
  if (!response.ok) {
    throw new Error(
      `${init?.method ?? "GET"} ${url} failed with status ${String(response.status)}.`,
    );
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
