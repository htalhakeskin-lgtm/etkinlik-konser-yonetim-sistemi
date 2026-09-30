import { createContext, use, useEffect, useSyncExternalStore } from "react";

import type { ConnectionState, RealtimeClient } from "./realtime";

/** The page's real-time client; the signed-in shell provides it and starts the connection (1.2). */
export const RealtimeContext = createContext<RealtimeClient | null>(null);

/** Keeps the screen in a notification group while it is shown, e.g. `events:{eventId}`. */
export function useRealtimeGroup(group: string | undefined): void {
  const client = use(RealtimeContext);
  useEffect(
    () => (client !== null && group !== undefined ? client.join(group) : undefined),
    [client, group],
  );
}

/** The connection state for the connection indicator; without a client the page is not connected. */
export function useConnectionState(): ConnectionState {
  const client = use(RealtimeContext);
  return useSyncExternalStore(
    (listener) => client?.subscribe(listener) ?? noSubscription,
    () => client?.getState() ?? "disconnected",
  );
}

function noSubscription(): void {
  // Nothing to unsubscribe from.
}
