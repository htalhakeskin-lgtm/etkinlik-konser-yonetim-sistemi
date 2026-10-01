import { QueryClient } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RealtimeClient, type RealtimeConnection, type ResourceChangedMessage } from "./realtime";

type Handler = (...args: never[]) => unknown;

// A connection whose server side the test plays.
function fakeConnection() {
  const handlers = new Map<string, Handler>();
  let reconnected: Handler = () => undefined;
  let reconnecting: Handler = () => undefined;
  let closed: Handler = () => undefined;
  const invoke = vi.fn(() => Promise.resolve());
  const connection = {
    start: vi.fn(() => Promise.resolve()),
    stop: vi.fn(() => Promise.resolve()),
    invoke,
    on: (name: string, handler: Handler) => {
      handlers.set(name, handler);
    },
    onreconnecting: (handler: Handler) => {
      reconnecting = handler;
    },
    onreconnected: (handler: Handler) => {
      reconnected = handler;
    },
    onclose: (handler: Handler) => {
      closed = handler;
    },
  } as unknown as RealtimeConnection;

  return {
    connection,
    invoke,
    notify: (message: ResourceChangedMessage) => {
      (handlers.get("resourceChanged") as (m: ResourceChangedMessage) => void)(message);
    },
    reconnecting: () => reconnecting(),
    reconnected: () => reconnected(),
    close: () => closed(),
  };
}

function invalidated(queryClient: QueryClient): string[] {
  return queryClient
    .getQueryCache()
    .getAll()
    .filter((query) => query.state.isInvalidated)
    .map((query) => String(query.queryKey[0]));
}

describe("RealtimeClient", () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient();
    queryClient.setQueryData(["/api/v1/events"], []);
    queryClient.setQueryData(["/api/v1/events/e1"], { version: 7 });
    queryClient.setQueryData(["/api/v1/events?status=confirmed"], []);
    queryClient.setQueryData(["/api/v1/event-types"], []);
    queryClient.setQueryData(["/api/v1/warehouses/w1"], { version: 2 });
  });

  it("joins the groups asked for before the connection opened, once each", async () => {
    const fake = fakeConnection();
    const client = new RealtimeClient(queryClient, fake.connection);
    client.join("events:e1");
    client.join("events:e1");

    await client.start();

    expect(client.getState()).toBe("connected");
    expect(fake.invoke.mock.calls).toEqual([["JoinGroup", "events:e1"]]);
  });

  it("leaves a group only when the last screen showing it closes", async () => {
    const fake = fakeConnection();
    const client = new RealtimeClient(queryClient, fake.connection);
    await client.start();
    const leaveFirst = client.join("events:e1");
    const leaveSecond = client.join("events:e1");

    leaveFirst();
    leaveFirst();
    expect(fake.invoke).not.toHaveBeenCalledWith("LeaveGroup", "events:e1");

    leaveSecond();
    expect(fake.invoke).toHaveBeenCalledWith("LeaveGroup", "events:e1");
  });

  it("invalidates the lists and records under the changed resource's address only", async () => {
    const fake = fakeConnection();
    await new RealtimeClient(queryClient, fake.connection).start();

    fake.notify({ resource: "events", id: "e1", version: 8 });

    expect(invalidated(queryClient).sort()).toEqual([
      "/api/v1/events",
      "/api/v1/events/e1",
      "/api/v1/events?status=confirmed",
    ]);
  });

  it("reads nothing again when the page already shows the announced version", async () => {
    const fake = fakeConnection();
    await new RealtimeClient(queryClient, fake.connection).start();

    fake.notify({ resource: "events", id: "e1", version: 7 });

    expect(invalidated(queryClient)).toEqual([]);
  });

  it("joins the groups again and reads everything after a reconnect", async () => {
    const fake = fakeConnection();
    const client = new RealtimeClient(queryClient, fake.connection);
    await client.start();
    client.join("warehouses:w1");
    fake.invoke.mockClear();

    fake.reconnecting();
    expect(client.getState()).toBe("reconnecting");
    fake.reconnected();

    expect(client.getState()).toBe("connected");
    await vi.waitFor(() => {
      expect(invalidated(queryClient)).toHaveLength(5);
    });
    expect(fake.invoke).toHaveBeenCalledWith("JoinGroup", "warehouses:w1");
  });

  it("tells subscribers about every state change", async () => {
    const fake = fakeConnection();
    const client = new RealtimeClient(queryClient, fake.connection);
    const states: string[] = [];
    client.subscribe(() => states.push(client.getState()));

    await client.start();
    fake.close();

    expect(states).toEqual(["connecting", "connected", "disconnected"]);
  });

  it("stays disconnected when the connection cannot open", async () => {
    const fake = fakeConnection();
    vi.mocked(fake.connection.start).mockRejectedValueOnce(new Error("offline"));
    const client = new RealtimeClient(queryClient, fake.connection);

    await expect(client.start()).rejects.toThrow("offline");
    expect(client.getState()).toBe("disconnected");
  });
});
