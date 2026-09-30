import { OctagonAlert } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";

export type ErrorStateProps = {
  error: unknown;
  onRetry?: () => void;
};

// A section that could not load (ui §10.3): what happened, a way to try again, and the trace id the
// user can copy into a report. The rest of the page keeps working.
export function ErrorState({ error, onRetry }: ErrorStateProps) {
  const { t } = useTranslation();
  const [isCopied, setIsCopied] = useState(false);
  const traceId = error instanceof ApiError ? error.traceId : undefined;

  return (
    <div role="alert" className="flex flex-col items-center gap-3 px-4 py-12 text-center">
      <OctagonAlert aria-hidden="true" className="size-8 text-status-danger" />
      <div className="flex max-w-md flex-col gap-1">
        <p className="font-medium">{t("errorState.title")}</p>
        <p className="text-sm text-muted-foreground">{errorMessage(error)}</p>
      </div>
      {onRetry !== undefined && (
        <Button variant="outline" onClick={onRetry}>
          {t("errorState.retry")}
        </Button>
      )}
      {traceId !== undefined && (
        <details className="text-sm text-muted-foreground">
          <summary className="cursor-pointer">{t("errorState.details")}</summary>
          <div className="mt-2 flex items-center gap-2">
            <span>{t("errorState.traceId")}</span>
            <code className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs select-all">
              {traceId}
            </code>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => {
                void navigator.clipboard.writeText(traceId).then(() => {
                  setIsCopied(true);
                });
              }}
            >
              {isCopied ? t("errorState.copied") : t("errorState.copy")}
            </Button>
          </div>
        </details>
      )}
    </div>
  );
}
