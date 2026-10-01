import { useSyncExternalStore } from "react";

// Below 1024 px the side menu closes and opens as a panel (ui §4).
const narrowQuery = "(max-width: 1023px)";

function subscribe(listener: () => void): () => void {
  const media = window.matchMedia(narrowQuery);
  media.addEventListener("change", listener);
  return () => {
    media.removeEventListener("change", listener);
  };
}

/** Whether the screen is narrower than the side menu's breakpoint. */
export function useIsMobile(): boolean {
  return useSyncExternalStore(subscribe, () => window.matchMedia(narrowQuery).matches);
}
