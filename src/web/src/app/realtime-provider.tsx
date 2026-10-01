import { useQueryClient } from "@tanstack/react-query";
import { type ReactNode, useEffect, useState } from "react";

import { RealtimeClient } from "@/lib/realtime";
import { RealtimeContext } from "@/lib/realtime-context";

// A first connection that fails is tried again, slower each time; after a drop SignalR reconnects by
// itself (building-blocks §12).
const retryDelaysMs = [2000, 5000, 10_000, 30_000];

export type RealtimeProviderProps = {
  children: ReactNode;
  createClient?: (queryClient: ReturnType<typeof useQueryClient>) => RealtimeClient;
};

// The signed-in shell's real-time connection: opened with the shell, closed when the user signs out.
export function RealtimeProvider({
  children,
  createClient = (queryClient) => new RealtimeClient(queryClient),
}: RealtimeProviderProps) {
  const queryClient = useQueryClient();
  const [client] = useState(() => createClient(queryClient));

  useEffect(() => {
    let isActive = true;
    let timer: ReturnType<typeof setTimeout> | undefined;
    const connect = (attempt: number) => {
      client.start().catch(() => {
        if (isActive) {
          timer = setTimeout(
            () => {
              connect(attempt + 1);
            },
            retryDelaysMs[Math.min(attempt, retryDelaysMs.length - 1)],
          );
        }
      });
    };

    connect(0);
    return () => {
      isActive = false;
      clearTimeout(timer);
      void client.stop();
    };
  }, [client]);

  return <RealtimeContext value={client}>{children}</RealtimeContext>;
}
