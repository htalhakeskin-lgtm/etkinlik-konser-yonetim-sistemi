import { type HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import type { QueryClient } from "@tanstack/react-query";

// Real-time notifications (ADR-0012, api §13): the server says what changed, the client reads it again
// through the API. One connection per page; screens join the groups of what they show.

/** The connection as the connection indicator shows it (ui §11.4). */
export type ConnectionState = "connecting" | "connected" | "reconnecting" | "disconnected";

/** The only message the server sends: what changed, never the data itself. */
export type ResourceChangedMessage = { resource: string; id: string; version: number | null };

/** The part of a SignalR connection the client uses; tests pass a fake. */
export type RealtimeConnection = Pick<
  HubConnection,
  "start" | "invoke" | "on" | "onreconnecting" | "onreconnected" | "onclose"
>;

const hubPath = "/hubs/notifications";
const apiPrefix = "/api/v1/";

/** The notification hub's connection, which reconnects by itself after a drop. */
export function connectToHub(): RealtimeConnection {
  return new HubConnectionBuilder().withUrl(hubPath).withAutomaticReconnect().build();
}

/**
 * Keeps the page's connection: joins and leaves groups as screens open and close, joins them again
 * after a reconnect (the server forgets them), and turns every notification into invalidated queries.
 */
export class RealtimeClient {
  readonly #connection: RealtimeConnection;
  readonly #queryClient: QueryClient;
  readonly #groups = new Map<string, number>();
  readonly #listeners = new Set<() => void>();
  #state: ConnectionState = "disconnected";

  constructor(queryClient: QueryClient, connection: RealtimeConnection = connectToHub()) {
    this.#queryClient = queryClient;
    this.#connection = connection;
    connection.on("resourceChanged", (message: ResourceChangedMessage) => {
      this.#invalidate(message);
    });
    connection.onreconnecting(() => {
      this.#setState("reconnecting");
    });
    connection.onreconnected(() => {
      void this.#resume();
    });
    connection.onclose(() => {
      this.#setState("disconnected");
    });
  }

  /** Opens the connection and joins the groups screens asked for meanwhile. */
  async start(): Promise<void> {
    this.#setState("connecting");
    try {
      await this.#connection.start();
    } catch (error) {
      this.#setState("disconnected");
      throw error;
    }

    this.#setState("connected");
    await this.#joinAll();
  }

  /** Joins a group for a screen; returns the function that leaves it when the screen closes. */
  join(group: string): () => void {
    const count = this.#groups.get(group) ?? 0;
    this.#groups.set(group, count + 1);
    if (count === 0 && this.#state === "connected") {
      void this.#connection.invoke("JoinGroup", group);
    }

    let hasLeft = false;
    return () => {
      if (hasLeft) {
        return;
      }

      hasLeft = true;
      const remaining = (this.#groups.get(group) ?? 1) - 1;
      if (remaining > 0) {
        this.#groups.set(group, remaining);
        return;
      }

      this.#groups.delete(group);
      if (this.#state === "connected") {
        void this.#connection.invoke("LeaveGroup", group);
      }
    };
  }

  /** The current connection state. */
  getState(): ConnectionState {
    return this.#state;
  }

  /** Calls the listener whenever the state changes; returns the unsubscribe function. */
  subscribe(listener: () => void): () => void {
    this.#listeners.add(listener);
    return () => {
      this.#listeners.delete(listener);
    };
  }

  async #resume(): Promise<void> {
    this.#setState("connected");
    await this.#joinAll();
    // Changes made while the connection was down were never announced (ui §11.4).
    await this.#queryClient.invalidateQueries({ refetchType: "active" });
  }

  async #joinAll(): Promise<void> {
    await Promise.all(
      [...this.#groups.keys()].map((group) => this.#connection.invoke("JoinGroup", group)),
    );
  }

  // Query keys start with the request's address, and a group's resource is the address's resource
  // (naming §8.3), so the resource's lists and records are the queries under its address.
  #invalidate(message: ResourceChangedMessage): void {
    const address = `${apiPrefix}${message.resource}`;
    const record = this.#queryClient.getQueryData<{ version?: unknown }>([
      `${address}/${message.id}`,
    ]);
    if (message.version !== null && record?.version === message.version) {
      // This page already shows that version, e.g. because it made the change.
      return;
    }

    void this.#queryClient.invalidateQueries({
      predicate: (query) => {
        const key = query.queryKey[0];
        return (
          typeof key === "string" &&
          (key === address || key.startsWith(`${address}/`) || key.startsWith(`${address}?`))
        );
      },
    });
  }

  #setState(state: ConnectionState): void {
    if (this.#state === state) {
      return;
    }

    this.#state = state;
    for (const listener of this.#listeners) {
      listener();
    }
  }
}
