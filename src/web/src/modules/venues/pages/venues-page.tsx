import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import { Building2, Ellipsis, Lock, Plus, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateVenue,
  deactivateVenue,
  getListVenuesQueryKey,
  listVenues,
} from "@/api/endpoints/venues/venues";
import { type ListVenuesParams, type VenueListItem, VenueStatusFilter } from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ifMatch } from "@/lib/api-client";
import { errorMessage } from "@/lib/api-error-messages";
import { auditPermissions, HistoryDialog, type HistoryRecord } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { VenueFormDialog } from "../components/venue-form-dialog";
import { venuesPermissions } from "../permissions";

/** The venues list's state in the address (ui §5.1). */
export type VenuesSearch = Omit<ListVenuesParams, "pageSize"> & { pageSize?: PageSize };

export type VenuesPageProps = {
  search: VenuesSearch;
  onSearchChange: (search: VenuesSearch) => void;
};

type Change = { action: "deactivate" | "activate"; venue: VenueListItem };

const searchDelayMs = 300;
const helper = createColumnHelper<typeof dataTableFeatures, VenueListItem>();

// The venues (US-VEN-001, 11 §3): searched by name and city, filtered by status; booking managers create
// and edit them in a dialog, technical managers open them for their equipment.
export function VenuesPage({ search, onSearchChange }: VenuesPageProps) {
  const { t } = useTranslation("venues");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, venuesPermissions.viewVenues);
  const canCreate = hasPermission(user, venuesPermissions.createVenues);
  const canEdit = hasPermission(user, venuesPermissions.editVenues);
  const canDeactivate = hasPermission(user, venuesPermissions.deactivateVenues);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [text, setText] = useState(search.q ?? "");
  const [isCreating, setIsCreating] = useState(false);
  const [editing, setEditing] = useState<VenueListItem | undefined>(undefined);
  const [confirming, setConfirming] = useState<VenueListItem | undefined>(undefined);
  const [history, setHistory] = useState<HistoryRecord | undefined>(undefined);
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListVenuesParams = { ...search, page, pageSize };
  const venues = useQuery({
    queryKey: getListVenuesQueryKey(params),
    queryFn: ({ signal }) => listVenues(params, { signal }),
    placeholderData: keepPreviousData,
    enabled: canView,
  });
  const refreshList = () => queryClient.invalidateQueries({ queryKey: getListVenuesQueryKey() });
  const change = useMutation({
    mutationFn: ({ action, venue }: Change) =>
      (action === "deactivate" ? deactivateVenue : activateVenue)(venue.id, ifMatch(venue.version)),
    onSuccess: (_saved, { action }) => {
      setConfirming(undefined);
      void refreshList();
      toast.success(
        t(action === "deactivate" ? "venues.done.deactivated" : "venues.done.activated"),
      );
    },
    onError: (error) => {
      setConfirming(undefined);
      toast.error(errorMessage(error));
    },
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
        <EmptyState icon={Lock} title={t("venues.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<VenueListItem> = helper.columns([
    helper.accessor("name", {
      header: t("venues.columns.name"),
      cell: ({ row }) => (
        <Link
          to="/venues/$venueId"
          params={{ venueId: row.original.id }}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    }),
    helper.accessor("city", { header: t("venues.columns.city") }),
    helper.display({
      id: "capacity",
      header: t("venues.columns.capacity"),
      cell: ({ row }) => (
        <span className="tabular-nums">{row.original.capacity.toLocaleString("tr-TR")}</span>
      ),
    }),
    helper.display({
      id: "operator",
      header: t("venues.columns.operator"),
      cell: ({ row }) => row.original.operatorName ?? "—",
    }),
    helper.display({
      id: "status",
      header: t("venues.columns.status"),
      cell: ({ row }) =>
        row.original.isActive ? (
          <StatusBadge tone="success" label={t("venues.status.active")} />
        ) : (
          <StatusBadge tone="muted" label={t("venues.status.inactive")} />
        ),
    }),
    helper.display({
      id: "actions",
      header: () => <span className="sr-only">{t("venues.columns.actions")}</span>,
      cell: ({ row }) =>
        canEdit || canDeactivate || canViewHistory ? (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("venues.actions.open", { name: row.original.name })}
                />
              }
            >
              <Ellipsis aria-hidden="true" />
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {canEdit && (
                <DropdownMenuItem
                  onClick={() => {
                    setEditing(row.original);
                  }}
                >
                  {t("venues.actions.edit")}
                </DropdownMenuItem>
              )}
              {canDeactivate &&
                (row.original.isActive ? (
                  <DropdownMenuItem
                    onClick={() => {
                      setConfirming(row.original);
                    }}
                  >
                    {t("venues.actions.deactivate")}
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={() => {
                      change.mutate({ action: "activate", venue: row.original });
                    }}
                  >
                    {t("venues.actions.activate")}
                  </DropdownMenuItem>
                ))}
              {canViewHistory && (
                <DropdownMenuItem
                  onClick={() => {
                    setHistory({
                      rootType: "Venue",
                      rootId: row.original.id,
                      name: row.original.name,
                    });
                  }}
                >
                  {t("venues.actions.history")}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        ) : null,
    }),
  ]);
  const statuses = Object.values(VenueStatusFilter).map((value) => ({
    value,
    label: t(`venues.filters.status.${value}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t("venues.title")}
        description={t("venues.description")}
        actions={
          canCreate ? (
            <Button
              onClick={() => {
                setIsCreating(true);
              }}
            >
              <Plus aria-hidden="true" />
              {t("venues.add")}
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
            aria-label={t("venues.search")}
            placeholder={t("venues.search")}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        <Select
          items={statuses}
          value={search.status ?? VenueStatusFilter.active}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              status: value === VenueStatusFilter.active ? undefined : (value as VenueStatusFilter),
              page: undefined,
            });
          }}
        >
          <SelectTrigger aria-label={t("venues.filters.statusLabel")}>
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
        label={t("venues.title")}
        columns={columns}
        rows={venues.data?.items}
        getRowId={(row) => row.id}
        isLoading={venues.isPending}
        error={venues.error}
        onRetry={() => {
          void venues.refetch();
        }}
        empty={<EmptyState icon={Building2} title={t("venues.empty")} />}
        sort={search.sort}
        sortable={["name", "city"]}
        onSortChange={(sort) => {
          onSearchChange({ ...search, sort, page: undefined });
        }}
        page={page}
        pageSize={pageSize}
        totalCount={venues.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
        }}
      />
      <VenueFormDialog
        open={isCreating || editing !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setIsCreating(false);
            setEditing(undefined);
          }
        }}
        venueId={editing?.id}
        onSaved={(_saved, isNew) => {
          setIsCreating(false);
          setEditing(undefined);
          void refreshList();
          toast.success(t(isNew ? "venues.done.created" : "venues.done.saved"));
        }}
      />
      <HistoryDialog
        record={history}
        onClose={() => {
          setHistory(undefined);
        }}
      />
      <ConfirmDialog
        open={confirming !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setConfirming(undefined);
          }
        }}
        title={t("venues.confirm.deactivateTitle", { name: confirming?.name })}
        description={t("venues.confirm.deactivateDescription")}
        confirmLabel={t("venues.actions.deactivate")}
        isPending={change.isPending}
        onConfirm={() => {
          if (confirming !== undefined) {
            change.mutate({ action: "deactivate", venue: confirming });
          }
        }}
      />
    </div>
  );
}
