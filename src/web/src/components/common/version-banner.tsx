import { CircleDot } from "lucide-react";
import { useSyncExternalStore } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { hasNewVersion, subscribeToNewVersion } from "@/lib/app-version";

export type VersionBannerContentProps = {
  onReload: () => void;
};

// A newer release is out (api §12): the banner stays until the user reloads; the page is never
// reloaded for them, so they can finish a form or a scan first (ui §9.5).
export function VersionBanner() {
  const isNewVersionAvailable = useSyncExternalStore(subscribeToNewVersion, hasNewVersion);

  if (!isNewVersionAvailable) {
    return null;
  }

  return (
    <VersionBannerContent
      onReload={() => {
        window.location.reload();
      }}
    />
  );
}

export function VersionBannerContent({ onReload }: VersionBannerContentProps) {
  const { t } = useTranslation();

  return (
    <div
      role="status"
      className="flex w-full items-center justify-center gap-3 border-b border-status-info-border bg-status-info-surface px-4 py-2 text-sm text-status-info"
    >
      <CircleDot aria-hidden="true" className="size-4 shrink-0" />
      <p>{t("versionBanner.message")}</p>
      <Button size="sm" variant="outline" onClick={onReload}>
        {t("versionBanner.reload")}
      </Button>
    </div>
  );
}
