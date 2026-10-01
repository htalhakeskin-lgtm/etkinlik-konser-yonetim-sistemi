import { queryOptions } from "@tanstack/react-query";

import { getGetMeQueryKey, getMe } from "@/api/endpoints/identity/identity";

/**
 * The signed-in user (`GET /api/v1/me`). Read once per session: signing in, setting a new password and
 * signing in again replace it, so it never goes stale on its own.
 */
export const meQuery = queryOptions({
  queryKey: getGetMeQueryKey(),
  queryFn: ({ signal }) => getMe({ signal }),
  staleTime: Number.POSITIVE_INFINITY,
  retry: false,
});

/** Where to go after signing in: only an address of this application, never another site. */
export function safeRedirect(target: string | undefined): string {
  return target !== undefined && target.startsWith("/") && !target.startsWith("//") ? target : "/";
}
