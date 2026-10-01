import { UsersRound } from "lucide-react";
import { describe, expect, it } from "vitest";

import { menu, type MenuGroup, visibleMenu } from "./navigation";

const groups: MenuGroup[] = [
  {
    label: "app.name",
    items: [
      { label: "app.name", to: "/", permission: "Identity.Users.View", icon: UsersRound },
      { label: "home.placeholder", to: "/", permission: "Identity.Roles.View", icon: UsersRound },
    ],
  },
  {
    label: "home.welcome",
    items: [{ label: "app.name", to: "/", permission: "Audit.Entries.View", icon: UsersRound }],
  },
];

describe("visibleMenu", () => {
  it("shows only the screens the user may open and hides a heading left empty", () => {
    const visible = visibleMenu(groups, ["Identity.Users.View"]);

    expect(visible).toHaveLength(1);
    expect(visible[0]?.items.map((item) => item.permission)).toEqual(["Identity.Users.View"]);
  });

  it("lists the users screen under administration for whoever may view users", () => {
    expect(visibleMenu(menu, ["Identity.Users.View"])[0]?.items.map((item) => item.to)).toEqual([
      "/admin/users",
    ]);
    expect(visibleMenu(menu, ["Booking.Events.View"])).toEqual([]);
  });
});
