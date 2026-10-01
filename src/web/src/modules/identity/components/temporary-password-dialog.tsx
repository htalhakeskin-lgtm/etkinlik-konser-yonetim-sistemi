import { Check, Copy } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export type IssuedPassword = {
  fullName: string;
  temporaryPassword: string;
};

export type TemporaryPasswordDialogProps = {
  issued: IssuedPassword | undefined;
  onClose: () => void;
};

// The temporary password, shown once (BR-SYS-006): the administrator copies it and hands it over; the
// user sets their own at the first sign-in.
export function TemporaryPasswordDialog({ issued, onClose }: TemporaryPasswordDialogProps) {
  const { t } = useTranslation("identity");
  const [isCopied, setIsCopied] = useState(false);

  function close() {
    setIsCopied(false);
    onClose();
  }

  return (
    <Dialog
      open={issued !== undefined}
      disablePointerDismissal
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          close();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("temporaryPassword.title", { name: issued?.fullName })}</DialogTitle>
          <DialogDescription>{t("temporaryPassword.description")}</DialogDescription>
        </DialogHeader>
        <div className="flex items-center gap-2">
          <code
            aria-label={t("temporaryPassword.label")}
            className="flex-1 rounded-md bg-muted px-3 py-2 text-center font-mono text-lg tracking-wider select-all"
          >
            {issued?.temporaryPassword}
          </code>
          <Button
            variant="outline"
            onClick={() => {
              void navigator.clipboard.writeText(issued?.temporaryPassword ?? "").then(() => {
                setIsCopied(true);
              });
            }}
          >
            {isCopied ? <Check aria-hidden="true" /> : <Copy aria-hidden="true" />}
            {isCopied ? t("temporaryPassword.copied") : t("temporaryPassword.copy")}
          </Button>
        </div>
        <DialogFooter>
          <Button onClick={close}>{t("temporaryPassword.done")}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
