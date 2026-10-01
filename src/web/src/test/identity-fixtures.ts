import type { SignedInUserDetails } from "@/api/model";
import { ApiError } from "@/lib/api-error";

/** A signed-in user for component tests. */
export function aUser(overrides: Partial<SignedInUserDetails> = {}): SignedInUserDetails {
  return {
    id: "0190f0c4-0000-7000-8000-000000000001",
    fullName: "Ayşe Kaya",
    email: "ayse@example.com",
    roles: ["bookingManager"],
    permissions: ["Identity.Users.View"],
    warehouseIds: [],
    mustChangePassword: false,
    ...overrides,
  };
}

/** A failed call as the API client reports it. */
export function anApiError(
  status: number,
  code: string,
  params: Record<string, unknown> = {},
): ApiError {
  return new ApiError({ status, code, params });
}
