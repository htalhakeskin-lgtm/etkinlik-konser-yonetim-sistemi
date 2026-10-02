import type { LinkProps } from "@tanstack/react-router";

import type { Role } from "@/api/model";

// Where each role starts (11 §4), in the table's order: a user with several roles starts where the
// first of them does. Until the events and conflicts screens arrive (1.4, 1.6) the booking manager starts
// on the parties and the technical manager on the catalog (parties MD-07); roles whose start screen has
// not arrived yet stay on the start page.
const startPages: readonly (readonly [Role, NonNullable<LinkProps["to"]>])[] = [
  ["systemAdministrator", "/admin/users"],
  ["bookingManager", "/parties"],
  ["technicalManager", "/catalog/categories"],
];

/** The screen `/` sends the user to, if their role has one. */
export function startPage(roles: readonly Role[]): NonNullable<LinkProps["to"]> | undefined {
  return startPages.find(([role]) => roles.includes(role))?.[1];
}
