import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import { Ellipsis, Lock, Package, Plus, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateEquipmentModel,
  deactivateEquipmentModel,
  getListEquipmentCategoriesQueryKey,
  getListEquipmentModelsQueryKey,
  listEquipmentCategories,
  listEquipmentModels,
} from "@/api/endpoints/catalog/catalog";
import {
  CategoryStatusFilter,
  type EquipmentModelListItem,
  type ListEquipmentModelsParams,
  ModelStatusFilter,
  TrackingType,
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
import { formatDecimal } from "@/lib/format";
import { auditPermissions, HistoryDialog, type HistoryRecord } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { pathLabel } from "../category-tree";
import { catalogPermissions } from "../permissions";

/** The models list's state in the address (ui §5.1). */
export type ModelsSearch = Omit<ListEquipmentModelsParams, "pageSize"> & { pageSize?: PageSize };

export type ModelsPageProps = {
  search: ModelsSearch;
  onSearchChange: (search: ModelsSearch) => void;
};

type Change = { action: "deactivate" | "activate"; model: EquipmentModelListItem };

const searchDelayMs = 300;
const allValues = "all";
const everyCategory = { status: CategoryStatusFilter.all };
const helper = createColumnHelper<typeof dataTableFeatures, EquipmentModelListItem>();

// The technical manager's models (US-EQP-002, 11 §3): searched by brand and name, filtered by category
// (its subtree included), tracking type and status; the form is a page of its own (catalog CT-05).
export function ModelsPage({ search, onSearchChange }: ModelsPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, catalogPermissions.viewModels);
  const canCreate = hasPermission(user, catalogPermissions.createModels);
  const canEdit = hasPermission(user, catalogPermissions.editModels);
  const canDeactivate = hasPermission(user, catalogPermissions.deactivateModels);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [text, setText] = useState(search.q ?? "");
  const [confirming, setConfirming] = useState<EquipmentModelListItem | undefined>(undefined);
  const [history, setHistory] = useState<HistoryRecord | undefined>(undefined);
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListEquipmentModelsParams = { ...search, page, pageSize };
  const models = useQuery({
    queryKey: getListEquipmentModelsQueryKey(params),
    queryFn: ({ signal }) => listEquipmentModels(params, { signal }),
    placeholderData: keepPreviousData,
    enabled: canView,
  });
  const categories = useQuery({
    queryKey: getListEquipmentCategoriesQueryKey(everyCategory),
    queryFn: ({ signal }) => listEquipmentCategories(everyCategory, { signal }),
    enabled: canView,
  });
  const change = useMutation({
    mutationFn: ({ action, model }: Change) =>
      (action === "deactivate" ? deactivateEquipmentModel : activateEquipmentModel)(
        model.id,
        ifMatch(model.version),
      ),
    onSuccess: (_saved, { action }) => {
      setConfirming(undefined);
      void queryClient.invalidateQueries({ queryKey: getListEquipmentModelsQueryKey() });
      toast.success(
        t(action === "deactivate" ? "models.done.deactivated" : "models.done.activated"),
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
        <EmptyState icon={Lock} title={t("models.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<EquipmentModelListItem> = helper.columns([
    helper.accessor("name", {
      header: t("models.columns.model"),
      cell: ({ row }) => (
        <Link
          to="/catalog/models/$modelId"
          params={{ modelId: row.original.id }}
          className="font-medium hover:underline"
        >
          {row.original.brand} {row.original.name}
        </Link>
      ),
    }),
    helper.display({
      id: "category",
      header: t("models.columns.category"),
      cell: ({ row }) => row.original.categoryPath.join(" › "),
    }),
    helper.display({
      id: "trackingType",
      header: t("models.columns.trackingType"),
      cell: ({ row }) => t(`trackingTypes.${row.original.trackingType}`),
    }),
    helper.display({
      id: "weight",
      header: t("models.columns.weight"),
      cell: ({ row }) => (
        <span className="tabular-nums">{formatDecimal(row.original.weightKilograms)}</span>
      ),
    }),
    helper.display({
      id: "power",
      header: t("models.columns.power"),
      cell: ({ row }) => (
        <span className="tabular-nums">
          {row.original.powerWatts === null ? "—" : row.original.powerWatts.toLocaleString("tr-TR")}
        </span>
      ),
    }),
    helper.display({
      id: "status",
      header: t("models.columns.status"),
      cell: ({ row }) =>
        row.original.isActive ? (
          <StatusBadge tone="success" label={t("models.status.active")} />
        ) : (
          <StatusBadge tone="muted" label={t("models.status.inactive")} />
        ),
    }),
    helper.display({
      id: "actions",
      header: () => <span className="sr-only">{t("models.columns.actions")}</span>,
      cell: ({ row }) =>
        canEdit || canDeactivate || canViewHistory ? (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("models.actions.open", {
                    name: `${row.original.brand} ${row.original.name}`,
                  })}
                />
              }
            >
              <Ellipsis aria-hidden="true" />
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {canEdit && (
                <DropdownMenuItem
                  render={
                    <Link
                      to="/catalog/models/$modelId/edit"
                      params={{ modelId: row.original.id }}
                    />
                  }
                >
                  {t("models.actions.edit")}
                </DropdownMenuItem>
              )}
              {canDeactivate &&
                (row.original.isActive ? (
                  <DropdownMenuItem
                    onClick={() => {
                      setConfirming(row.original);
                    }}
                  >
                    {t("models.actions.deactivate")}
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={() => {
                      change.mutate({ action: "activate", model: row.original });
                    }}
                  >
                    {t("models.actions.activate")}
                  </DropdownMenuItem>
                ))}
              {canViewHistory && (
                <DropdownMenuItem
                  onClick={() => {
                    setHistory({
                      rootType: "EquipmentModel",
                      rootId: row.original.id,
                      name: `${row.original.brand} ${row.original.name}`,
                    });
                  }}
                >
                  {t("models.actions.history")}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        ) : null,
    }),
  ]);
  const categoryOptions = [
    { value: allValues, label: t("models.filters.allCategories") },
    ...(categories.data ?? [])
      .map((category) => ({ value: category.id, label: pathLabel(category) }))
      .sort((left, right) => left.label.localeCompare(right.label, "tr")),
  ];
  const trackingTypes = [
    { value: allValues, label: t("models.filters.allTrackingTypes") },
    ...Object.values(TrackingType).map((value) => ({ value, label: t(`trackingTypes.${value}`) })),
  ];
  const statuses = Object.values(ModelStatusFilter).map((value) => ({
    value,
    label: t(`models.filters.status.${value}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t("models.title")}
        description={t("models.description")}
        actions={
          canCreate ? (
            <Button render={<Link to="/catalog/models/new" />}>
              <Plus aria-hidden="true" />
              {t("models.add")}
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
            aria-label={t("models.search")}
            placeholder={t("models.search")}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        <FilterSelect
          label={t("models.filters.categoryLabel")}
          options={categoryOptions}
          value={search.categoryId ?? allValues}
          onChange={(value) => {
            onSearchChange({
              ...search,
              categoryId: value === allValues ? undefined : value,
              page: undefined,
            });
          }}
        />
        <FilterSelect
          label={t("models.filters.trackingTypeLabel")}
          options={trackingTypes}
          value={search.trackingType ?? allValues}
          onChange={(value) => {
            onSearchChange({
              ...search,
              trackingType: value === allValues ? undefined : (value as TrackingType),
              page: undefined,
            });
          }}
        />
        <FilterSelect
          label={t("models.filters.statusLabel")}
          options={statuses}
          value={search.status ?? ModelStatusFilter.active}
          onChange={(value) => {
            onSearchChange({
              ...search,
              status: value === ModelStatusFilter.active ? undefined : (value as ModelStatusFilter),
              page: undefined,
            });
          }}
        />
      </div>
      <DataTable
        label={t("models.title")}
        columns={columns}
        rows={models.data?.items}
        getRowId={(row) => row.id}
        isLoading={models.isPending}
        error={models.error}
        onRetry={() => {
          void models.refetch();
        }}
        empty={<EmptyState icon={Package} title={t("models.empty")} />}
        sort={search.sort}
        sortable={["name"]}
        onSortChange={(sort) => {
          onSearchChange({ ...search, sort, page: undefined });
        }}
        page={page}
        pageSize={pageSize}
        totalCount={models.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
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
        title={t("models.confirm.deactivateTitle", {
          name: confirming === undefined ? "" : `${confirming.brand} ${confirming.name}`,
        })}
        description={t("models.confirm.deactivateDescription")}
        confirmLabel={t("models.actions.deactivate")}
        isPending={change.isPending}
        onConfirm={() => {
          if (confirming !== undefined) {
            change.mutate({ action: "deactivate", model: confirming });
          }
        }}
      />
    </div>
  );
}

type FilterSelectProps = {
  label: string;
  options: readonly { value: string; label: string }[];
  value: string;
  onChange: (value: string) => void;
};

function FilterSelect({ label, options, value, onChange }: FilterSelectProps) {
  return (
    <Select
      items={options}
      value={value}
      onValueChange={(next) => {
        if (next !== null) {
          onChange(next);
        }
      }}
    >
      <SelectTrigger aria-label={label}>
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {options.map((option) => (
          <SelectItem key={option.value} value={option.value}>
            {option.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
