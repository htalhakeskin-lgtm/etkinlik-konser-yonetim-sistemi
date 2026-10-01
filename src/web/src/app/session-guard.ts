import type { QueryClient } from "@tanstack/react-query";
import { redirect } from "@tanstack/react-router";

import type { SignedInUserDetails } from "@/api/model";
import { ApiError } from "@/lib/api-error";
import { meQuery } from "@/modules/identity";

/**
 * Loads the signed-in user before a screen opens. Without a session it goes to the sign-in screen and
 * comes back afterwards; with a temporary password, to the new password screen (BR-SYS-006).
 */
export async function requireSession(
  queryClient: QueryClient,
  returnTo: string,
): Promise<SignedInUserDetails> {
  let user: SignedInUserDetails;
  try {
    user = await queryClient.query({ ...meQuery, staleTime: "static" });
  } catch (error) {
    if (error instanceof ApiError && error.status === 401) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects by throwing
      throw redirect({ to: "/login", search: { redirect: returnTo } });
    }

    throw error;
  }

  if (user.mustChangePassword) {
    // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects by throwing
    throw redirect({ to: "/set-password" });
  }

  return user;
}
