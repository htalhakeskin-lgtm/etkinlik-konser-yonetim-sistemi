import { Outlet } from "@tanstack/react-router";

import { VersionBanner } from "@/components/common/version-banner";
import { Toaster } from "@/components/ui/sonner";

// Banners sit above everything and push the page down instead of covering it (ui §9.5).
export function RootLayout() {
  return (
    <>
      <VersionBanner />
      <Outlet />
      <Toaster />
    </>
  );
}
