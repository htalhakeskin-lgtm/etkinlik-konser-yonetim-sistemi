import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createColumnHelper } from "@tanstack/react-table";
import { Ellipsis, Lock, Plus, Search, Warehouse } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateWarehouse,
  deactivateWarehouse,
  getListWarehousesQueryKey,
  listWarehouses,
} from "@/api/endpoints/inventory/inventory";
import {
  type ListWarehousesParams,
  type WarehouseListItem,
  WarehouseStatusFilter,
} from "@/api/model";
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

import { WarehouseFormDialog } from "../components/warehouse-form-dialog";
import { inventoryPermissions } from "../permissions";

/** The warehouses list's state in the address (ui §5.1). */
export type WarehousesSearch = Omit<ListWarehousesParams, "pageSize"> & { pageSize?: PageSize };

export type WarehousesPageProps = {
  search: WarehousesSearch;
  onSearchChange: (search: WarehousesSearch) => void;
};

type Change = { action: "deactivate" | "activate"; warehouse: WarehouseListItem };

const searchDelayMs = 300;
const helper = createColumnHelper<typeof dataTableFeatures, WarehouseListItem>();

// The system administrator's warehouses (US-SYS-005, 11 §3): the list with a search and a status
// filter, a dialog to create and edit, and deactivation after a confirmation.
export function WarehousesPage({ search, onSearchChange }: WarehousesPageProps) {
  const { t } = useTranslation("inventory");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, inventoryPermissions.viewWarehouses);
  const canCreate = hasPermission(user, inventoryPermissions.createWarehouses);
  const canEdit = hasPermission(user, inventoryPermissions.editWarehouses);
  const canDeactivate = hasPermission(user, inventoryPermissions.deactivateWarehouses);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [text, setText] = useState(search.q ?? "");
  const [isCreating, setIsCreating] = useState(false);
  const [editing, setEditing] = useState<WarehouseListItem | undefined>(undefined);
  const [confirming, setConfirming] = useState<WarehouseListItem | undefined>(undefined);
  const [history, setHistory] = useState<HistoryRecord | undefined>(undefined);
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListWarehousesParams = { ...search, page, pageSize };
  const warehouses = useQuery({
    queryKey: getListWarehousesQueryKey(params),
    queryFn: ({ signal }) => listWarehouses(params, { signal }),
    placeholderData: keepPreviousData,
    enabled: canView,
  });
  const refreshList = () =>
    queryClient.invalidateQueries({ queryKey: getListWarehousesQueryKey() });
  const change = useMutation({
    mutationFn: ({ action, warehouse }: Change) =>
      (action === "deactivate" ? deactivateWarehouse : activateWarehouse)(
        warehouse.id,
        ifMatch(warehouse.version),
      ),
    onSuccess: (_saved, { action }) => {
      setConfirming(undefined);
      void refreshList();
      toast.success(
        t(action === "deactivate" ? "warehouses.done.deactivated" : "warehouses.done.activated"),
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
        <EmptyState icon={Lock} title={t("warehouses.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<WarehouseListItem> = helper.columns([
    helper.accessor("name", {
      header: t("warehouses.columns.name"),
      cell: (info) => <span className="font-medium">{info.getValue()}</span>,
    }),
    helper.accessor("city", { header: t("warehouses.columns.city") }),
    helper.accessor("address", {
      header: t("warehouses.columns.address"),
      cell: (info) => <span className="block max-w-xs truncate">{info.getValue()}</span>,
    }),
    helper.display({
      id: "status",
      header: t("warehouses.columns.status"),
      cell: ({ row }) =>
        row.original.isActive ? (
          <StatusBadge tone="success" label={t("warehouses.status.active")} />
        ) : (
          <StatusBadge tone="muted" label={t("warehouses.status.inactive")} />
        ),
    }),
    helper.display({
      id: "actions",
      header: () => <span className="sr-only">{t("warehouses.columns.actions")}</span>,
      cell: ({ row }) =>
        canEdit || canDeactivate || canViewHistory ? (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("warehouses.actions.open", { name: row.original.name })}
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
                  {t("warehouses.actions.edit")}
                </DropdownMenuItem>
              )}
              {canDeactivate &&
                (row.original.isActive ? (
                  <DropdownMenuItem
                    onClick={() => {
                      setConfirming(row.original);
                    }}
                  >
                    {t("warehouses.actions.deactivate")}
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={() => {
                      change.mutate({ action: "activate", warehouse: row.original });
                    }}
                  >
                    {t("warehouses.actions.activate")}
                  </DropdownMenuItem>
                ))}
              {canViewHistory && (
                <DropdownMenuItem
                  onClick={() => {
                    setHistory({
                      entityType: "Warehouse",
                      entityId: row.original.id,
                      name: row.original.name,
                    });
                  }}
                >
                  {t("warehouses.actions.history")}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        ) : null,
    }),
  ]);
  const statuses = Object.values(WarehouseStatusFilter).map((status) => ({
    value: status,
    label: t(`warehouses.filters.status.${status}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t("warehouses.title")}
        description={t("warehouses.description")}
        actions={
          canCreate ? (
            <Button
              onClick={() => {
                setIsCreating(true);
              }}
            >
              <Plus aria-hidden="true" />
              {t("warehouses.add")}
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
            aria-label={t("warehouses.search")}
            placeholder={t("warehouses.search")}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        <Select
          items={statuses}
          value={search.status ?? WarehouseStatusFilter.active}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              status:
                value === WarehouseStatusFilter.active
                  ? undefined
                  : (value as WarehouseStatusFilter),
              page: undefined,
            });
          }}
        >
          <SelectTrigger aria-label={t("warehouses.filters.statusLabel")}>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {statuses.map((status) => (
              <SelectItem key={status.value} value={status.value}>
                {status.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <DataTable
        label={t("warehouses.title")}
        columns={columns}
        rows={warehouses.data?.items}
        getRowId={(row) => row.id}
        isLoading={warehouses.isPending}
        error={warehouses.error}
        onRetry={() => {
          void warehouses.refetch();
        }}
        empty={<EmptyState icon={Warehouse} title={t("warehouses.empty")} />}
        sort={search.sort}
        sortable={["name", "city"]}
        onSortChange={(sort) => {
          onSearchChange({ ...search, sort, page: undefined });
        }}
        page={page}
        pageSize={pageSize}
        totalCount={warehouses.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
        }}
      />
      <WarehouseFormDialog
        open={isCreating || editing !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setIsCreating(false);
            setEditing(undefined);
          }
        }}
        warehouseId={editing?.id}
        onSaved={(_saved, isNew) => {
          setIsCreating(false);
          setEditing(undefined);
          void refreshList();
          toast.success(t(isNew ? "warehouses.done.created" : "warehouses.done.saved"));
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
        title={t("warehouses.confirm.deactivateTitle", { name: confirming?.name })}
        description={t("warehouses.confirm.deactivateDescription")}
        confirmLabel={t("warehouses.actions.deactivate")}
        isPending={change.isPending}
        onConfirm={() => {
          if (confirming !== undefined) {
            change.mutate({ action: "deactivate", warehouse: confirming });
          }
        }}
      />
    </div>
  );
}
