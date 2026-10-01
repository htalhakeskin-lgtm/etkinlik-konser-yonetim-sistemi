import { isRedirect } from "@tanstack/react-router";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { getMe } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { anApiError, aUser } from "@/test/identity-fixtures";

import { requireSession } from "./session-guard";

vi.mock("@/api/endpoints/identity/identity", () => ({
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));

const getMeMock = vi.mocked(getMe);

async function redirectOf(promise: Promise<unknown>) {
  const error: unknown = await promise.catch((caught: unknown) => caught);
  if (!isRedirect(error)) {
    throw new Error("Expected a redirect.");
  }

  return error.options;
}

describe("requireSession", () => {
  beforeEach(() => {
    getMeMock.mockReset();
  });

  it("lets a signed-in user through", async () => {
    const user = aUser();
    getMeMock.mockResolvedValue(user);

    await expect(requireSession(createQueryClient(), "/events")).resolves.toEqual(user);
  });

  it("sends a visitor without a session to sign in, and back afterwards", async () => {
    getMeMock.mockRejectedValue(anApiError(401, "unauthorized"));

    const options = await redirectOf(requireSession(createQueryClient(), "/events?page=2"));

    expect(options).toMatchObject({ to: "/login", search: { redirect: "/events?page=2" } });
  });

  it("sends a user with a temporary password to set a new one first", async () => {
    getMeMock.mockResolvedValue(aUser({ mustChangePassword: true, permissions: [] }));

    const options = await redirectOf(requireSession(createQueryClient(), "/events"));

    expect(options).toMatchObject({ to: "/set-password" });
  });
});
