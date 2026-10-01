import { Outlet } from "@tanstack/react-router";

import { ReauthDialog } from "@/modules/identity";

// The frame of every screen behind sign-in. The menu and the top bar arrive with the shell.
export function AppLayout() {
  return (
    <>
      <Outlet />
      <ReauthDialog />
    </>
  );
}
