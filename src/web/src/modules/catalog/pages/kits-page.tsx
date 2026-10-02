import { keepPreviousData, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import { Boxes, Lock, Plus, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import { getListKitsQueryKey, listKits } from "@/api/endpoints/catalog/catalog";
import {
  type KitDetails,
  type KitListItem,
  KitStatusFilter,
  type ListKitsParams,
} from "@/api/model";
import { DataTable } from "@/components/common/data-table";
import {
  type DataTableColumns,
  type dataTableFeatures,
  type PageSize,
} from "@/components/common/data-table-model";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";
import { StatusBadge } from "@/components/common/status-badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { KitNameDialog } from "../components/kit-name-dialog";
import { KitTotal } from "../components/kit-totals";
import { catalogPermissions } from "../permissions";

/** The kits list's state in the address (ui §5.1). */
export type KitsSearch = Omit<ListKitsParams, "pageSize"> & { pageSize?: PageSize };

export type KitsPageProps = {
  search: KitsSearch;
  onSearchChange: (search: KitsSearch) => void;
  /** Opens a new kit's page, where its lines are added. */
  onCreated: (kit: KitDetails) => void;
};

const searchDelayMs = 300;
const helper = createColumnHelper<typeof dataTableFeatures, KitListItem>();

// The technical manager's kits (US-EQP-005, 11 §3): each with its line count and totals; a new kit gets
// its name here and its lines on its own page.
export function KitsPage({ search, onSearchChange, onCreated }: KitsPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, catalogPermissions.viewKits);
  const canCreate = hasPermission(user, catalogPermissions.createKits);
  const [text, setText] = useState(search.q ?? "");
  const [isCreating, setIsCreating] = useState(false);
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListKitsParams = { ...search, page, pageSize };
  const kits = useQuery({
    queryKey: getListKitsQueryKey(params),
    queryFn: ({ signal }) => listKits(params, { signal }),
    placeholderData: keepPreviousData,
    enabled: canView,
  });

  // The search waits until typing pauses, so each key press is not a request.
  useEffect(() => {
    const typed = text.trim();
    if (typed === (search.q ?? "")) {
      return undefined;
    }

    const timer = setTimeout(() => {
      onSearchChange({ ...search, q: typed === "" ? undefined : typed, page: undefined });
    }, searchDelayMs);
    return () => {
      clearTimeout(timer);
    };
  }, [text, search, onSearchChange]);

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("kits.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<KitListItem> = helper.columns([
    helper.accessor("name", {
      header: t("kits.columns.name"),
      cell: ({ row }) => (
        <Link
          to="/catalog/kits/$kitId"
          params={{ kitId: row.original.id }}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    }),
    helper.display({
      id: "lineCount",
      header: t("kits.columns.lines"),
      cell: ({ row }) => <span className="tabular-nums">{row.original.lineCount}</span>,
    }),
    helper.display({
      id: "weight",
      header: t("kits.columns.weight"),
      cell: ({ row }) => <KitTotal totals={row.original.totals} of="weight" />,
    }),
    helper.display({
      id: "power",
      header: t("kits.columns.power"),
      cell: ({ row }) => <KitTotal totals={row.original.totals} of="power" />,
    }),
    helper.display({
      id: "status",
      header: t("kits.columns.status"),
      cell: ({ row }) =>
        row.original.isActive ? (
          <StatusBadge tone="success" label={t("kits.status.active")} />
        ) : (
          <StatusBadge tone="muted" label={t("kits.status.inactive")} />
        ),
    }),
  ]);
  const statuses = Object.values(KitStatusFilter).map((value) => ({
    value,
    label: t(`kits.filters.status.${value}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t("kits.title")}
        description={t("kits.description")}
        actions={
          canCreate ? (
            <Button
              onClick={() => {
                setIsCreating(true);
              }}
            >
              <Plus aria-hidden="true" />
              {t("kits.add")}
            </Button>
          ) : undefined
        }
      />
      <div className="flex flex-wrap items-center gap-2">
        <div className="relative w-full max-w-xs">
          <Search
            aria-hidden="true"
            className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
          />
          <Input
            type="search"
            className="pl-8"
            aria-label={t("kits.search")}
            placeholder={t("kits.search")}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        <Select
          items={statuses}
          value={search.status ?? KitStatusFilter.active}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              status: value === KitStatusFilter.active ? undefined : (value as KitStatusFilter),
              page: undefined,
            });
          }}
        >
          <SelectTrigger aria-label={t("kits.filters.statusLabel")}>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {statuses.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <DataTable
        label={t("kits.title")}
        columns={columns}
        rows={kits.data?.items}
        getRowId={(row) => row.id}
        isLoading={kits.isPending}
        error={kits.error}
        onRetry={() => {
          void kits.refetch();
        }}
        empty={<EmptyState icon={Boxes} title={t("kits.empty")} />}
        // Kits are listed in Turkish name order only.
        sort={undefined}
        sortable={[]}
        onSortChange={() => undefined}
        page={page}
        pageSize={pageSize}
        totalCount={kits.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
        }}
      />
      <KitNameDialog
        open={isCreating}
        onClose={() => {
          setIsCreating(false);
        }}
        onSaved={(saved) => {
          setIsCreating(false);
          void queryClient.invalidateQueries({ queryKey: getListKitsQueryKey() });
          toast.success(t("kits.done.created"));
          onCreated(saved);
        }}
      />
    </div>
  );
}
