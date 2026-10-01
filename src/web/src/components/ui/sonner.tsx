import type { CSSProperties } from "react";
import { useSyncExternalStore } from "react";
import { Toaster as Sonner, type ToasterProps } from "sonner";

import { readThemePreference } from "@/lib/theme";

const phoneQuery = "(max-width: 767px)";

function subscribeToPhone(listener: () => void): () => void {
  const media = window.matchMedia(phoneQuery);
  media.addEventListener("change", listener);
  return () => {
    media.removeEventListener("change", listener);
  };
}

// Short notices (ui §9.3): bottom right on a desktop, at the top on a phone, where the bottom belongs
// to the action bar; at most three at a time, four seconds each. Written without next-themes, which
// the shadcn/ui version needs, since the theme is already ours (ui §3.8).
export function Toaster(props: ToasterProps) {
  const isPhone = useSyncExternalStore(
    subscribeToPhone,
    () => window.matchMedia(phoneQuery).matches,
  );

  return (
    <Sonner
      theme={readThemePreference()}
      position={isPhone ? "top-center" : "bottom-right"}
      visibleToasts={3}
      duration={4000}
      style={
        {
          "--normal-bg": "var(--popover)",
          "--normal-text": "var(--popover-foreground)",
          "--normal-border": "var(--border)",
        } as CSSProperties
      }
      {...props}
    />
  );
}
