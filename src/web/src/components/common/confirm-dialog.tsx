import { useId, useRef, useState } from "react";
import { useTranslation } from "react-i18next";

import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";

export type ConfirmDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** Names the action and the record: "Yaz Festivali 2027 iptal edilsin mi?" */
  title: string;
  /** Says what will happen: "Onaylı 42 rezervasyon serbest bırakılacak; bu geri alınamaz." */
  description: string;
  /** The action's name, e.g. "Etkinliği iptal et". */
  confirmLabel: string;
  onConfirm: (reason: string | undefined) => void;
  isPending?: boolean;
  /** A reason field, when the rule asks for one. */
  reason?: { label: string };
};

// Confirms an action that cannot be undone or that affects others (ui §9.4). Focus starts on
// "Vazgeç", so Enter never starts the action by accident.
export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel,
  onConfirm,
  isPending = false,
  reason,
}: ConfirmDialogProps) {
  const { t } = useTranslation();
  const cancelRef = useRef<HTMLButtonElement>(null);
  const reasonId = useId();
  const [reasonText, setReasonText] = useState("");
  const trimmedReason = reasonText.trim();

  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent initialFocus={cancelRef}>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        {reason !== undefined && (
          <div className="flex flex-col gap-1.5">
            <label htmlFor={reasonId} className="text-sm font-medium">
              {reason.label}
            </label>
            <textarea
              id={reasonId}
              required
              rows={3}
              value={reasonText}
              onChange={(event) => {
                setReasonText(event.target.value);
              }}
              className="rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:ring-3 focus-visible:ring-ring focus-visible:outline-none"
            />
          </div>
        )}
        <AlertDialogFooter>
          <AlertDialogCancel ref={cancelRef} disabled={isPending}>
            {t("confirmDialog.cancel")}
          </AlertDialogCancel>
          <Button
            variant="destructive"
            isLoading={isPending}
            disabled={reason !== undefined && trimmedReason === ""}
            onClick={() => {
              onConfirm(reason === undefined ? undefined : trimmedReason);
            }}
          >
            {confirmLabel}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
