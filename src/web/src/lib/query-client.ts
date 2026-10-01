import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";

import { ApiError } from "./api-error";
import { reportSessionExpired } from "./session-expiry";

declare module "@tanstack/react-query" {
  // eslint-disable-next-line @typescript-eslint/consistent-type-definitions -- module augmentation merges into an interface
  interface Register {
    mutationMeta: {
      /** The call answers 401 for its own reasons, e.g. signing in with a wrong password. */
      expectsUnauthorized?: boolean;
    };
  }
}

function isUnauthorized(error: unknown): boolean {
  return error instanceof ApiError && error.status === 401;
}

// Query defaults (retries, stale times, error handling) are set together with the generated API client.
export function createQueryClient(): QueryClient {
  return new QueryClient({
    queryCache: new QueryCache({
      onError: (error) => {
        if (isUnauthorized(error)) {
          reportSessionExpired();
        }
      },
    }),
    mutationCache: new MutationCache({
      onError: (error, _variables, _context, mutation) => {
        if (isUnauthorized(error) && mutation.meta?.expectsUnauthorized !== true) {
          reportSessionExpired();
        }
      },
    }),
    defaultOptions: {
      // A 401 or 403 does not get better by asking again.
      queries: {
        retry: (failureCount, error) =>
          failureCount < 3 &&
          !(error instanceof ApiError && (error.status === 401 || error.status === 403)),
      },
    },
  });
}
