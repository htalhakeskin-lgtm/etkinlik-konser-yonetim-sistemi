import { cn } from "cn";
import type { LucideIcon } from "lucide-react";
import { CircleCheck, OctagonAlert, TriangleAlert } from "lucide-react";
import { useEffect, useRef, useState, useSyncExternalStore } from "react";
import { useTranslation } from "react-i18next";

import type { ConnectionState } from "@/lib/realtime";
import { useConnectionState } from "@/lib/realtime-context";

export type ConnectionIndicatorContentProps = {
  state: ConnectionState;
  /** How long reconnecting may take before the banner shows (ui §11.4). */
  warningDelayMs?: number;
};

const restoredNoticeMs = 4000;

const toneClasses = {
  danger: "border-status-danger-border bg-status-danger-surface text-status-danger",
  warning: "border-status-warning-border bg-status-warning-surface text-status-warning",
  success: "border-status-success-border bg-status-success-surface text-status-success",
};

// The connection banners of ui §9.5 and §11.4, for both the notification connection and the
// browser's own offline state (US-WHS-005). The signed-in shell shows it (1.2).
export function ConnectionIndicator() {
  const state = useConnectionState();
  const isOnline = useSyncExternalStore(subscribeToOnline, () => navigator.onLine);
  return <ConnectionIndicatorContent state={isOnline ? state : "disconnected"} />;
}

export function ConnectionIndicatorContent({
  state,
  warningDelayMs = 2000,
}: ConnectionIndicatorContentProps) {
  const { t } = useTranslation();
  const isReconnecting = state === "reconnecting";
  const hasWaited = useHasLasted(isReconnecting, warningDelayMs);
  const isRestored = useIsRestored(state);

  if (state === "disconnected") {
    return <Banner tone="danger" role="alert" icon={OctagonAlert} text={t("connection.offline")} />;
  }

  if (isReconnecting && hasWaited) {
    return <Banner tone="warning" icon={TriangleAlert} text={t("connection.reconnecting")} />;
  }

  return isRestored ? (
    <Banner tone="success" icon={CircleCheck} text={t("connection.restored")} />
  ) : null;
}

type BannerProps = {
  tone: keyof typeof toneClasses;
  icon: LucideIcon;
  text: string;
  role?: "status" | "alert";
};

function Banner({ tone, icon: Icon, text, role = "status" }: BannerProps) {
  return (
    <div
      role={role}
      data-tone={tone}
      className={cn(
        "flex w-full items-center justify-center gap-2 border-b px-4 py-2 text-sm",
        toneClasses[tone],
      )}
    >
      <Icon aria-hidden="true" className="size-4 shrink-0" />
      <p>{text}</p>
    </div>
  );
}

// Whether the condition has held for the whole delay.
function useHasLasted(isActive: boolean, delayMs: number): boolean {
  const [hasLasted, setHasLasted] = useState(false);

  useEffect(() => {
    if (!isActive) {
      return undefined;
    }

    const timer = setTimeout(() => {
      setHasLasted(true);
    }, delayMs);
    return () => {
      clearTimeout(timer);
      setHasLasted(false);
    };
  }, [isActive, delayMs]);

  return isActive && hasLasted;
}

// True for a moment after the connection comes back from a drop, not after the first connect.
function useIsRestored(state: ConnectionState): boolean {
  const wasInterrupted = useRef(false);
  const [isShown, setIsShown] = useState(false);

  useEffect(() => {
    if (state === "reconnecting" || state === "disconnected") {
      wasInterrupted.current = true;
      return undefined;
    }

    if (state !== "connected" || !wasInterrupted.current) {
      return undefined;
    }

    wasInterrupted.current = false;
    const shown = setTimeout(() => {
      setIsShown(true);
    }, 0);
    const hidden = setTimeout(() => {
      setIsShown(false);
    }, restoredNoticeMs);
    return () => {
      clearTimeout(shown);
      clearTimeout(hidden);
    };
  }, [state]);

  return state === "connected" && isShown;
}

function subscribeToOnline(listener: () => void): () => void {
  window.addEventListener("online", listener);
  window.addEventListener("offline", listener);
  return () => {
    window.removeEventListener("online", listener);
    window.removeEventListener("offline", listener);
  };
}
