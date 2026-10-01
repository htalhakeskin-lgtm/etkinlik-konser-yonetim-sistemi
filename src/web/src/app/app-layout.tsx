import { useSuspenseQuery } from "@tanstack/react-query";
import { Outlet } from "@tanstack/react-router";

import { ConnectionIndicator } from "@/components/common/connection-indicator";
import { SidebarInset, SidebarProvider, SidebarTrigger } from "@/components/ui/sidebar";
import { TooltipProvider } from "@/components/ui/tooltip";
import { meQuery, ReauthDialog, UserMenu } from "@/modules/identity";

import { AppSidebar } from "./app-sidebar";
import { RealtimeProvider } from "./realtime-provider";

// The shell of every office screen behind sign-in (11 §2.1): the connection banner across the top,
// the side menu, a thin top bar with the user menu, and the page.
export function AppLayout() {
  const { data: user } = useSuspenseQuery(meQuery);

  return (
    <RealtimeProvider>
      <ConnectionIndicator />
      <TooltipProvider>
        <SidebarProvider>
          <AppSidebar permissions={user.permissions} />
          <SidebarInset>
            <header className="flex h-12 items-center gap-2 border-b px-3">
              <SidebarTrigger />
              <div className="ml-auto">
                <UserMenu user={user} />
              </div>
            </header>
            <Outlet />
          </SidebarInset>
        </SidebarProvider>
      </TooltipProvider>
      <ReauthDialog />
    </RealtimeProvider>
  );
}
