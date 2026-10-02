import type { LinkProps } from "@tanstack/react-router";
import type { ParseKeys } from "i18next";
import {
  BookUser,
  History,
  type LucideIcon,
  ShieldCheck,
  UsersRound,
  Warehouse,
} from "lucide-react";

/** One screen in the menu, shown to users who hold its permission (US-SYS-012). */
export type MenuItem = {
  label: ParseKeys;
  to: NonNullable<LinkProps["to"]>;
  permission: string;
  /** Shown alone when the menu is collapsed. */
  icon: LucideIcon;
};

/** A module's screens under its heading (11 §2.1). */
export type MenuGroup = {
  label: ParseKeys;
  items: readonly MenuItem[];
};

/** The menu; each screen joins it in the same change that adds its route. */
export const menu: readonly MenuGroup[] = [
  {
    label: "menu.masterData",
    items: [
      {
        label: "menu.parties",
        to: "/parties",
        permission: "Parties.Parties.View",
        icon: BookUser,
      },
    ],
  },
  {
    label: "menu.administration",
    items: [
      {
        label: "menu.users",
        to: "/admin/users",
        permission: "Identity.Users.View",
        icon: UsersRound,
      },
      {
        label: "menu.roles",
        to: "/admin/roles",
        permission: "Identity.Roles.View",
        icon: ShieldCheck,
      },
      {
        label: "menu.warehouses",
        to: "/admin/warehouses",
        permission: "Inventory.Warehouses.View",
        icon: Warehouse,
      },
      {
        label: "menu.audit",
        to: "/audit",
        permission: "Audit.Entries.View",
        icon: History,
      },
    ],
  },
];

/** The groups and screens the user may open; a heading with nothing left under it is hidden. */
export function visibleMenu(
  groups: readonly MenuGroup[],
  permissions: readonly string[],
): MenuGroup[] {
  const held = new Set(permissions);
  return groups
    .map((group) => ({ ...group, items: group.items.filter((item) => held.has(item.permission)) }))
    .filter((group) => group.items.length > 0);
}
