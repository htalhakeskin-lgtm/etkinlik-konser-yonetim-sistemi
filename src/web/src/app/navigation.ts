import type { LinkProps } from "@tanstack/react-router";
import type { ParseKeys } from "i18next";

/** One screen in the menu, shown to users who hold its permission (US-SYS-012). */
export type MenuItem = {
  label: ParseKeys;
  to: NonNullable<LinkProps["to"]>;
  permission: string;
};

/** A module's screens under its heading (11 §2.1). */
export type MenuGroup = {
  label: ParseKeys;
  items: readonly MenuItem[];
};

/** The menu; each screen joins it in the same change that adds its route. */
export const menu: readonly MenuGroup[] = [];

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
