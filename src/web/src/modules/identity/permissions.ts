import { useSuspenseQuery } from "@tanstack/react-query";

import type { SignedInUserDetails } from "@/api/model";

import { meQuery } from "./session";

/** The permissions Identity's screens ask for (identity §5.1). */
export const identityPermissions = {
  viewUsers: "Identity.Users.View",
  createUsers: "Identity.Users.Create",
  editUsers: "Identity.Users.Edit",
  deactivateUsers: "Identity.Users.Deactivate",
  resetUserPasswords: "Identity.Users.ResetPassword",
  viewRoles: "Identity.Roles.View",
} as const;

/**
 * Whether the user holds the permission. The screen decides by permission, never by role (ui §12); the
 * server checks it again (BR-SYS-002).
 */
export function hasPermission(user: SignedInUserDetails, permission: string): boolean {
  return user.permissions.includes(permission);
}

/** The signed-in user; the route guard has loaded them before any screen behind sign-in renders. */
export function useSignedInUser(): SignedInUserDetails {
  return useSuspenseQuery(meQuery).data;
}
