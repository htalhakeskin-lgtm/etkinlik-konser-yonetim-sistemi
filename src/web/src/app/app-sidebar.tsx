import { Link, useMatchRoute } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarRail,
} from "@/components/ui/sidebar";

import { menu, visibleMenu } from "./navigation";

export type AppSidebarProps = {
  permissions: readonly string[];
};

// The collapsible side menu (ui §4): grouped by module, only the screens the user may open.
export function AppSidebar({ permissions }: AppSidebarProps) {
  const { t } = useTranslation();
  const matchRoute = useMatchRoute();

  return (
    <Sidebar collapsible="icon">
      <SidebarHeader>
        <span className="px-2 py-1 text-lg font-semibold text-primary group-data-[collapsible=icon]:hidden">
          {t("app.name")}
        </span>
      </SidebarHeader>
      <SidebarContent>
        <nav aria-label={t("shell.menu")}>
          {visibleMenu(menu, permissions).map((group) => (
            <SidebarGroup key={group.label}>
              <SidebarGroupLabel>{t(group.label)}</SidebarGroupLabel>
              <SidebarGroupContent>
                <SidebarMenu>
                  {group.items.map((item) => (
                    <SidebarMenuItem key={item.to}>
                      <SidebarMenuButton
                        render={<Link to={item.to} />}
                        isActive={matchRoute({ to: item.to, fuzzy: true }) !== false}
                        tooltip={t(item.label)}
                      >
                        <item.icon aria-hidden="true" />
                        <span>{t(item.label)}</span>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ))}
        </nav>
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  );
}
