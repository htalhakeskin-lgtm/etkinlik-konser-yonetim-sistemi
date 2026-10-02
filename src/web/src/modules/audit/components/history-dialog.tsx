import { useTranslation } from "react-i18next";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { HistoryList } from "./history-list";

export type HistoryRecord = {
  /** The record's type, e.g. `Party`; its parts' changes (contact points) show with it. */
  rootType: string;
  rootId: string;
  name: string;
};

export type HistoryDialogProps = {
  record: HistoryRecord | undefined;
  onClose: () => void;
};

// A record's own history (US-SYS-004 criterion 1), for screens whose records open in a dialog rather
// than on a detail page; a detail page puts HistoryTab in its tab instead.
export function HistoryDialog({ record, onClose }: HistoryDialogProps) {
  const { t } = useTranslation("audit");

  return (
    <Dialog
      open={record !== undefined}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          onClose();
        }
      }}
    >
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{t("dialog.title", { name: record?.name })}</DialogTitle>
          <DialogDescription>{t("description")}</DialogDescription>
        </DialogHeader>
        <div className="max-h-[60vh] overflow-auto">
          {record !== undefined && <HistoryTab rootType={record.rootType} rootId={record.rootId} />}
        </div>
      </DialogContent>
    </Dialog>
  );
}

export type HistoryTabProps = {
  rootType: string;
  rootId: string;
};

/** The history tab of a detail page (audit §4): the record's and its parts' changes, newest first. */
export function HistoryTab({ rootType, rootId }: HistoryTabProps) {
  return <HistoryList filter={{ rootType, rootId }} showRecord={false} />;
}
