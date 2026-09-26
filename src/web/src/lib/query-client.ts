import { QueryClient } from "@tanstack/react-query";

// Query defaults (retries, stale times, error handling) are set together with the generated API client.
export function createQueryClient(): QueryClient {
  return new QueryClient();
}
