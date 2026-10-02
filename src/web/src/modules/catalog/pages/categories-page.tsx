import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Ellipsis, FolderTree, Lock, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateEquipmentCategory,
  deactivateEquipmentCategory,
  getListEquipmentCategoriesQueryKey,
  listEquipmentCategories,
} from "@/api/endpoints/catalog/catalog";
import { CategoryStatusFilter, type EquipmentCategoryItem } from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { PageHeader } from "@/components/common/page-header";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ifMatch } from "@/lib/api-client";
import { errorMessage } from "@/lib/api-error-messages";
import { auditPermissions, HistoryDialog, type HistoryRecord } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { categoryRows, pathLabel } from "../category-tree";
import { type CategoryEdit, CategoryFormDialog } from "../components/category-form-dialog";
import { catalogPermissions } from "../permissions";

/** The category tree's state in the address (ui §5.1). */
export type CategoriesSearch = { status?: CategoryStatusFilter | undefined };

export type CategoriesPageProps = {
  search: CategoriesSearch;
  onSearchChange: (search: CategoriesSearch) => void;
};

type Change = { action: "deactivate" | "activate"; category: EquipmentCategoryItem };

// Every category, active or not: the tree is small and the dialog chooses parents from it (catalog CT-02).
const everyCategory = { status: CategoryStatusFilter.all };

// The technical manager's category tree (US-EQP-001, 11 §3): indented rows, a dialog to add, rename and
// move, deactivation after a confirmation (BR-EQP-002, catalog CT-06).
export function CategoriesPage({ search, onSearchChange }: CategoriesPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, catalogPermissions.viewCategories);
  const canCreate = hasPermission(user, catalogPermissions.createCategories);
  const canEdit = hasPermission(user, catalogPermissions.editCategories);
  const canDeactivate = hasPermission(user, catalogPermissions.deactivateCategories);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [edit, setEdit] = useState<CategoryEdit | undefined>(undefined);
  const [confirming, setConfirming] = useState<EquipmentCategoryItem | undefined>(undefined);
  const [history, setHistory] = useState<HistoryRecord | undefined>(undefined);
  const status = search.status ?? CategoryStatusFilter.active;
  const categories = useQuery({
    queryKey: getListEquipmentCategoriesQueryKey(everyCategory),
    queryFn: ({ signal }) => listEquipmentCategories(everyCategory, { signal }),
    enabled: canView,
  });
  const refresh = () =>
    queryClient.invalidateQueries({ queryKey: getListEquipmentCategoriesQueryKey() });
  const change = useMutation({
    mutationFn: ({ action, category }: Change) =>
      (action === "deactivate" ? deactivateEquipmentCategory : activateEquipmentCategory)(
        category.id,
        ifMatch(category.version),
      ),
    onSuccess: (_saved, { action }) => {
      setConfirming(undefined);
      void refresh();
      toast.success(
        t(action === "deactivate" ? "categories.done.deactivated" : "categories.done.activated"),
      );
    },
    onError: (error) => {
      setConfirming(undefined);
      toast.error(errorMessage(error));
    },
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("categories.forbidden")} />
      </div>
    );
  }

  const all = categories.data ?? [];
  const shown = all.filter((category) =>
    status === CategoryStatusFilter.all
      ? true
      : category.isActive === (status === CategoryStatusFilter.active),
  );
  const shownIds = new Set(shown.map((category) => category.id));
  const rows = categoryRows(shown);
  const statuses = Object.values(CategoryStatusFilter).map((value) => ({
    value,
    label: t(`categories.filters.status.${value}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t("categories.title")}
        description={t("categories.description")}
        actions={
          canCreate ? (
            <Button
              onClick={() => {
                setEdit({ mode: "create", parentId: null });
              }}
            >
              <Plus aria-hidden="true" />
              {t("categories.add")}
            </Button>
          ) : undefined
        }
      />
      <div className="flex flex-wrap items-center gap-2">
        <Select
          items={statuses}
          value={status}
          onValueChange={(value) => {
            onSearchChange({
              status:
                value === CategoryStatusFilter.active ? undefined : (value as CategoryStatusFilter),
            });
          }}
        >
          <SelectTrigger aria-label={t("categories.filters.statusLabel")}>
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
      {categories.isError ? (
        <ErrorState
          error={categories.error}
          onRetry={() => {
            void categories.refetch();
          }}
        />
      ) : categories.data === undefined ? (
        <SkeletonBlock className="h-64 w-full" />
      ) : rows.length === 0 ? (
        <EmptyState icon={FolderTree} title={t("categories.empty")} />
      ) : (
        <Table aria-label={t("categories.title")}>
          <TableHeader>
            <TableRow>
              <TableHead>{t("categories.columns.name")}</TableHead>
              <TableHead>{t("categories.columns.models")}</TableHead>
              <TableHead>{t("categories.columns.status")}</TableHead>
              <TableHead>
                <span className="sr-only">{t("categories.columns.actions")}</span>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {rows.map(({ category, depth }) => {
              // A category whose parent is filtered out shows its path, so its place stays clear.
              const isDetached = category.parentId !== null && !shownIds.has(category.parentId);
              return (
                <TableRow key={category.id}>
                  <TableCell>
                    <span
                      data-depth={depth}
                      className="block font-medium"
                      style={{ paddingInlineStart: `${String(depth * 1.5)}rem` }}
                    >
                      {isDetached ? pathLabel(category) : category.name}
                    </span>
                  </TableCell>
                  <TableCell className="tabular-nums">{category.activeModelCount}</TableCell>
                  <TableCell>
                    {category.isActive ? (
                      <StatusBadge tone="success" label={t("categories.status.active")} />
                    ) : (
                      <StatusBadge tone="muted" label={t("categories.status.inactive")} />
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    {(canCreate || canEdit || canDeactivate || canViewHistory) && (
                      <DropdownMenu>
                        <DropdownMenuTrigger
                          render={
                            <Button
                              variant="ghost"
                              size="icon-sm"
                              aria-label={t("categories.actions.open", { name: category.name })}
                            />
                          }
                        >
                          <Ellipsis aria-hidden="true" />
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          {canCreate && category.isActive && (
                            <DropdownMenuItem
                              onClick={() => {
                                setEdit({ mode: "create", parentId: category.id });
                              }}
                            >
                              {t("categories.actions.addChild")}
                            </DropdownMenuItem>
                          )}
                          {canEdit && (
                            <DropdownMenuItem
                              onClick={() => {
                                setEdit({ mode: "edit", category });
                              }}
                            >
                              {t("categories.actions.edit")}
                            </DropdownMenuItem>
                          )}
                          {canDeactivate &&
                            (category.isActive ? (
                              <DropdownMenuItem
                                onClick={() => {
                                  setConfirming(category);
                                }}
                              >
                                {t("categories.actions.deactivate")}
                              </DropdownMenuItem>
                            ) : (
                              <DropdownMenuItem
                                onClick={() => {
                                  change.mutate({ action: "activate", category });
                                }}
                              >
                                {t("categories.actions.activate")}
                              </DropdownMenuItem>
                            ))}
                          {canViewHistory && (
                            <DropdownMenuItem
                              onClick={() => {
                                setHistory({
                                  rootType: "EquipmentCategory",
                                  rootId: category.id,
                                  name: category.name,
                                });
                              }}
                            >
                              {t("categories.actions.history")}
                            </DropdownMenuItem>
                          )}
                        </DropdownMenuContent>
                      </DropdownMenu>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      )}
      <CategoryFormDialog
        edit={edit}
        categories={all}
        onClose={() => {
          setEdit(undefined);
        }}
        onSaved={(_saved, isNew) => {
          setEdit(undefined);
          void refresh();
          toast.success(t(isNew ? "categories.done.created" : "categories.done.saved"));
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
        title={t("categories.confirm.deactivateTitle", { name: confirming?.name })}
        description={t("categories.confirm.deactivateDescription")}
        confirmLabel={t("categories.actions.deactivate")}
        isPending={change.isPending}
        onConfirm={() => {
          if (confirming !== undefined) {
            change.mutate({ action: "deactivate", category: confirming });
          }
        }}
      />
    </div>
  );
}
