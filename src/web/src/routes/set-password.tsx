import { createFileRoute, redirect } from "@tanstack/react-router";

import { ApiError } from "@/lib/api-error";
import { meQuery, SetPasswordPage } from "@/modules/identity";

export const Route = createFileRoute("/set-password")({
  // Only a temporary password leads here; everyone else changes it from the user menu.
  beforeLoad: async ({ context }) => {
    const user = await context.queryClient
      .query({ ...meQuery, staleTime: "static" })
      .catch((error: unknown) => {
        if (error instanceof ApiError && error.status === 401) {
          // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects by throwing
          throw redirect({ to: "/login" });
        }

        throw error;
      });
    if (!user.mustChangePassword) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects by throwing
      throw redirect({ to: "/" });
    }
  },
  component: SetPasswordPage,
});
