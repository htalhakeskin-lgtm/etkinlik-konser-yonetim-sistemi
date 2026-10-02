import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { createColumnHelper } from "@tanstack/react-table";
import { BookUser, Ellipsis, Lock, Plus, Search } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateParty,
  deactivateParty,
  getListPartiesQueryKey,
  listParties,
} from "@/api/endpoints/parties/parties";
import {
  type ListPartiesParams,
  PartyKind,
  type PartyListItem,
  PartyRole,
  PartyStatusFilter,
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

import { PartyFormDialog } from "../components/party-form-dialog";
import { partiesPermissions } from "../permissions";

/** The parties list's state in the address (ui §5.1). */
export type PartiesSearch = Omit<ListPartiesParams, "pageSize"> & { pageSize?: PageSize };

/**
 * Which list the page is: every party, or the artists only (11 §3, `/artists`), which keep the artist
 * role fixed and open on the artists' pages.
 */
export type PartiesVariant = "parties" | "artists";

export type PartiesPageProps = {
  search: PartiesSearch;
  onSearchChange: (search: PartiesSearch) => void;
  variant?: PartiesVariant;
};

type Change = { action: "deactivate" | "activate"; party: PartyListItem };

const searchDelayMs = 300;
const allValues = "all";
const helper = createColumnHelper<typeof dataTableFeatures, PartyListItem>();

// The booking manager's parties (US-PTY-001, US-PTY-002, 11 §3): searched by name and contact point,
// filtered by role, kind and status; a dialog to create and edit, deactivation after a confirmation.
export function PartiesPage({ search, onSearchChange, variant = "parties" }: PartiesPageProps) {
  const { t } = useTranslation("parties");
  const isArtists = variant === "artists";
  const texts = isArtists ? "artists" : "parties";
  const detailPath = isArtists ? "/artists/$partyId" : "/parties/$partyId";
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canView = hasPermission(user, partiesPermissions.viewParties);
  const canCreate = hasPermission(user, partiesPermissions.createParties);
  const canEdit = hasPermission(user, partiesPermissions.editParties);
  const canDeactivate = hasPermission(user, partiesPermissions.deactivateParties);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [text, setText] = useState(search.q ?? "");
  const [isCreating, setIsCreating] = useState(false);
  const [editing, setEditing] = useState<PartyListItem | undefined>(undefined);
  const [confirming, setConfirming] = useState<PartyListItem | undefined>(undefined);
  const [history, setHistory] = useState<HistoryRecord | undefined>(undefined);
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListPartiesParams = {
    ...search,
    role: isArtists ? PartyRole.artist : search.role,
    page,
    pageSize,
  };
  const parties = useQuery({
    queryKey: getListPartiesQueryKey(params),
    queryFn: ({ signal }) => listParties(params, { signal }),
    placeholderData: keepPreviousData,
    enabled: canView,
  });
  const refreshList = () => queryClient.invalidateQueries({ queryKey: getListPartiesQueryKey() });
  const change = useMutation({
    mutationFn: ({ action, party }: Change) =>
      (action === "deactivate" ? deactivateParty : activateParty)(party.id, ifMatch(party.version)),
    onSuccess: (_saved, { action }) => {
      setConfirming(undefined);
      void refreshList();
      toast.success(
        t(action === "deactivate" ? "parties.done.deactivated" : "parties.done.activated"),
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
        <EmptyState icon={Lock} title={t("parties.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<PartyListItem> = helper.columns([
    helper.accessor("name", {
      header: t("parties.columns.name"),
      cell: ({ row }) => (
        <Link
          to={detailPath}
          params={{ partyId: row.original.id }}
          className="font-medium hover:underline"
        >
          {row.original.name}
        </Link>
      ),
    }),
    helper.display({
      id: "kind",
      header: t("parties.columns.kind"),
      cell: ({ row }) => t(`kinds.${row.original.kind}`),
    }),
    helper.display({
      id: "roles",
      header: t("parties.columns.roles"),
      cell: ({ row }) => (
        <span className="flex flex-wrap gap-1">
          {row.original.roles.map((role) => (
            <StatusBadge key={role} tone="info" label={t(`roles.${role}`)} />
          ))}
        </span>
      ),
    }),
    helper.display({
      id: "primaryPhone",
      header: t("parties.columns.phone"),
      cell: ({ row }) => row.original.primaryPhone ?? "—",
    }),
    helper.display({
      id: "primaryEmail",
      header: t("parties.columns.email"),
      cell: ({ row }) => (
        <span className="block max-w-xs truncate">{row.original.primaryEmail ?? "—"}</span>
      ),
    }),
    helper.display({
      id: "status",
      header: t("parties.columns.status"),
      cell: ({ row }) =>
        row.original.isActive ? (
          <StatusBadge tone="success" label={t("parties.status.active")} />
        ) : (
          <StatusBadge tone="muted" label={t("parties.status.inactive")} />
        ),
    }),
    helper.display({
      id: "actions",
      header: () => <span className="sr-only">{t("parties.columns.actions")}</span>,
      cell: ({ row }) =>
        canEdit || canDeactivate || canViewHistory ? (
          <DropdownMenu>
            <DropdownMenuTrigger
              render={
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("parties.actions.open", { name: row.original.name })}
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
                  {t("parties.actions.edit")}
                </DropdownMenuItem>
              )}
              {canDeactivate &&
                (row.original.isActive ? (
                  <DropdownMenuItem
                    onClick={() => {
                      setConfirming(row.original);
                    }}
                  >
                    {t("parties.actions.deactivate")}
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onClick={() => {
                      change.mutate({ action: "activate", party: row.original });
                    }}
                  >
                    {t("parties.actions.activate")}
                  </DropdownMenuItem>
                ))}
              {canViewHistory && (
                <DropdownMenuItem
                  onClick={() => {
                    setHistory({
                      rootType: "Party",
                      rootId: row.original.id,
                      name: row.original.name,
                    });
                  }}
                >
                  {t("parties.actions.history")}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        ) : null,
    }),
  ]);
  const roles = [
    { value: allValues, label: t("parties.filters.allRoles") },
    ...Object.values(PartyRole).map((role) => ({ value: role, label: t(`roles.${role}`) })),
  ];
  const kinds = [
    { value: allValues, label: t("parties.filters.allKinds") },
    ...Object.values(PartyKind).map((kind) => ({ value: kind, label: t(`kinds.${kind}`) })),
  ];
  const statuses = Object.values(PartyStatusFilter).map((status) => ({
    value: status,
    label: t(`parties.filters.status.${status}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={t(`${texts}.title`)}
        description={t(`${texts}.description`)}
        actions={
          canCreate ? (
            <Button
              onClick={() => {
                setIsCreating(true);
              }}
            >
              <Plus aria-hidden="true" />
              {t(`${texts}.add`)}
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
            aria-label={t(`${texts}.search`)}
            placeholder={t(`${texts}.search`)}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        {!isArtists && (
          <FilterSelect
            label={t("parties.filters.roleLabel")}
            options={roles}
            value={search.role ?? allValues}
            onChange={(value) => {
              onSearchChange({
                ...search,
                role: value === allValues ? undefined : (value as PartyRole),
                page: undefined,
              });
            }}
          />
        )}
        <FilterSelect
          label={t("parties.filters.kindLabel")}
          options={kinds}
          value={search.kind ?? allValues}
          onChange={(value) => {
            onSearchChange({
              ...search,
              kind: value === allValues ? undefined : (value as PartyKind),
              page: undefined,
            });
          }}
        />
        <FilterSelect
          label={t("parties.filters.statusLabel")}
          options={statuses}
          value={search.status ?? PartyStatusFilter.active}
          onChange={(value) => {
            onSearchChange({
              ...search,
              status: value === PartyStatusFilter.active ? undefined : (value as PartyStatusFilter),
              page: undefined,
            });
          }}
        />
      </div>
      <DataTable
        label={t(`${texts}.title`)}
        columns={columns}
        rows={parties.data?.items}
        getRowId={(row) => row.id}
        isLoading={parties.isPending}
        error={parties.error}
        onRetry={() => {
          void parties.refetch();
        }}
        empty={<EmptyState icon={BookUser} title={t(`${texts}.empty`)} />}
        sort={search.sort}
        sortable={["name"]}
        onSortChange={(sort) => {
          onSearchChange({ ...search, sort, page: undefined });
        }}
        page={page}
        pageSize={pageSize}
        totalCount={parties.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
        }}
      />
      <PartyFormDialog
        open={isCreating || editing !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setIsCreating(false);
            setEditing(undefined);
          }
        }}
        partyId={editing?.id}
        initialRoles={params.role === undefined ? [] : [params.role]}
        onSaved={(_saved, isNew) => {
          setIsCreating(false);
          setEditing(undefined);
          void refreshList();
          toast.success(t(isNew ? "parties.done.created" : "parties.done.saved"));
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
        title={t("parties.confirm.deactivateTitle", { name: confirming?.name })}
        description={t("parties.confirm.deactivateDescription")}
        confirmLabel={t("parties.actions.deactivate")}
        isPending={change.isPending}
        onConfirm={() => {
          if (confirming !== undefined) {
            change.mutate({ action: "deactivate", party: confirming });
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
