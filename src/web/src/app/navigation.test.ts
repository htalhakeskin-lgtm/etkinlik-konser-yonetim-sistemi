import { describe, expect, it } from "vitest";

import { type MenuGroup, visibleMenu } from "./navigation";

const groups: MenuGroup[] = [
  {
    label: "app.name",
    items: [
      { label: "app.name", to: "/", permission: "Identity.Users.View" },
      { label: "home.placeholder", to: "/", permission: "Identity.Roles.View" },
    ],
  },
  {
    label: "home.welcome",
    items: [{ label: "app.name", to: "/", permission: "Audit.Entries.View" }],
  },
];

describe("visibleMenu", () => {
  it("shows only the screens the user may open and hides a heading left empty", () => {
    const menu = visibleMenu(groups, ["Identity.Users.View"]);

    expect(menu).toHaveLength(1);
    expect(menu[0]?.items.map((item) => item.permission)).toEqual(["Identity.Users.View"]);
  });
});
