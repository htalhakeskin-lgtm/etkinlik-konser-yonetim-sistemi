import { useInfiniteQuery } from "@tanstack/react-query";
import { History } from "lucide-react";
import { useTranslation } from "react-i18next";

import { getListAuditEntriesQueryKey, listAuditEntries } from "@/api/endpoints/audit/audit";
import type { AuditEntryItem, ListAuditEntriesParams } from "@/api/model";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
import type { StatusTone } from "@/components/common/status-tone";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { formatDateTime } from "@/lib/format";

import { describeChanges, entityTypeName } from "../changes";

export type HistoryFilter = Omit<ListAuditEntriesParams, "after" | "limit">;

export type HistoryListProps = {
  filter: HistoryFilter;
  /** Whether each row names its record; a record's own history leaves it out. */
  showRecord?: boolean;
};

const actionTones: Record<AuditEntryItem["action"], StatusTone> = {
  created: "success",
  updated: "info",
  deleted: "danger",
  statusChanged: "active",
};

// The change history from newest to oldest (US-SYS-004): who, when and what kind of change; a row opens
// to each field's old and new value. It grows with "Daha fazla yükle", never by scrolling (ui §8).
export function HistoryList({ filter, showRecord = true }: HistoryListProps) {
  const { t } = useTranslation("audit");
  const entries = useInfiniteQuery({
    queryKey: getListAuditEntriesQueryKey(filter),
    queryFn: ({ pageParam, signal }) =>
      listAuditEntries(pageParam === undefined ? filter : { ...filter, after: pageParam }, {
        signal,
      }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });

  if (entries.isError) {
    return (
      <ErrorState
        error={entries.error}
        onRetry={() => {
          void entries.refetch();
        }}
      />
    );
  }

  if (entries.data === undefined) {
    return <SkeletonBlock className="h-40 w-full" />;
  }

  const items = entries.data.pages.flatMap((page) => page.items);
  if (items.length === 0) {
    return <EmptyState icon={History} title={t("empty")} />;
  }

  return (
    <div className="flex flex-col gap-3">
      <ul className="flex flex-col divide-y rounded-md border">
        {items.map((entry) => {
          const changes = describeChanges(entry);
          return (
            <li key={entry.id} className="flex flex-col gap-2 p-3">
              <div className="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm">
                <time dateTime={entry.occurredAt} className="text-muted-foreground tabular-nums">
                  {formatDateTime(entry.occurredAt)}
                </time>
                <span className="font-medium">{entry.actorName}</span>
                <StatusBadge
                  tone={actionTones[entry.action]}
                  label={t(`actions.${entry.action}`)}
                />
                {showRecord && (
                  <span className="text-muted-foreground">
                    {entityTypeName(entry.rootType)} ·{" "}
                    <code className="text-xs">{entry.rootId}</code>
                  </span>
                )}
                {entry.entityType !== entry.rootType && (
                  <span className="text-muted-foreground">{entityTypeName(entry.entityType)}</span>
                )}
              </div>
              {changes.length > 0 && (
                <details>
                  <summary className="cursor-pointer text-sm text-primary">
                    {t("changes.show")}
                  </summary>
                  <Table className="mt-2">
                    <TableHeader>
                      <TableRow>
                        <TableHead>{t("changes.field")}</TableHead>
                        <TableHead>{t("changes.old")}</TableHead>
                        <TableHead>{t("changes.new")}</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {changes.map((change) => (
                        <TableRow key={change.field}>
                          <TableCell className="font-medium">{change.label}</TableCell>
                          <TableCell className="whitespace-normal text-muted-foreground">
                            {change.old ?? t("changes.none")}
                          </TableCell>
                          <TableCell className="whitespace-normal">
                            {change.new ?? t("changes.none")}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </details>
              )}
            </li>
          );
        })}
      </ul>
      {entries.hasNextPage && (
        <Button
          variant="outline"
          className="self-center"
          isLoading={entries.isFetchingNextPage}
          onClick={() => {
            void entries.fetchNextPage();
          }}
        >
          {t("loadMore")}
        </Button>
      )}
    </div>
  );
}
