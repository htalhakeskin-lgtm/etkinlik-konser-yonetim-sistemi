import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { createColumnHelper } from "@tanstack/react-table";
import { Lock, Search, UserRound } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

import { getListUsersQueryKey, listUsers } from "@/api/endpoints/identity/identity";
import { type ListUsersParams, Role, type UserListItem, UserStatusFilter } from "@/api/model";
import { DataTable } from "@/components/common/data-table";
import {
  type DataTableColumns,
  type dataTableFeatures,
  type PageSize,
} from "@/components/common/data-table-model";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";
import { StatusBadge } from "@/components/common/status-badge";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { formatTime } from "@/lib/format";

import { hasPermission, identityPermissions, useSignedInUser } from "../permissions";

/** The users list's state in the address (ui §5.1). */
export type UsersSearch = Omit<ListUsersParams, "pageSize"> & { pageSize?: PageSize };

export type UsersPageProps = {
  search: UsersSearch;
  onSearchChange: (search: UsersSearch) => void;
};

const searchDelayMs = 300;
const allRoles = "all";
const helper = createColumnHelper<typeof dataTableFeatures, UserListItem>();

// The system administrator's users list (US-SYS-001, 11 §3): search, role and status filters, one
// page at a time.
export function UsersPage({ search, onSearchChange }: UsersPageProps) {
  const { t } = useTranslation("identity");
  const user = useSignedInUser();
  const canView = hasPermission(user, identityPermissions.viewUsers);
  const [text, setText] = useState(search.q ?? "");
  const page = search.page ?? 1;
  const pageSize = search.pageSize ?? 25;
  const params: ListUsersParams = { ...search, page, pageSize };
  const users = useQuery({
    queryKey: getListUsersQueryKey(params),
    queryFn: ({ signal }) => listUsers(params, { signal }),
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
        <EmptyState icon={Lock} title={t("users.forbidden")} />
      </div>
    );
  }

  const columns: DataTableColumns<UserListItem> = helper.columns([
    helper.accessor("fullName", {
      header: t("users.columns.fullName"),
      cell: (info) => <span className="font-medium">{info.getValue()}</span>,
    }),
    helper.accessor("email", { header: t("users.columns.email") }),
    helper.accessor("roles", {
      header: t("users.columns.roles"),
      cell: ({ row }) => row.original.roles.map((role) => t(`roles.${role}`)).join(", "),
    }),
    helper.display({
      id: "status",
      header: t("users.columns.status"),
      cell: ({ row }) => {
        const listed = row.original;
        if (!listed.isActive) {
          return <StatusBadge tone="muted" label={t("users.status.inactive")} />;
        }

        return listed.lockedUntil === null ? (
          <StatusBadge tone="success" label={t("users.status.active")} />
        ) : (
          <StatusBadge
            tone="warning"
            label={t("users.status.locked", { time: formatTime(listed.lockedUntil) })}
          />
        );
      },
    }),
  ]);
  const roles = [
    { value: allRoles, label: t("users.filters.allRoles") },
    ...Object.values(Role).map((role) => ({ value: role, label: t(`roles.${role}`) })),
  ];
  const statuses = Object.values(UserStatusFilter).map((status) => ({
    value: status,
    label: t(`users.filters.status.${status}`),
  }));

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader title={t("users.title")} description={t("users.description")} />
      <div className="flex flex-wrap items-center gap-2">
        <div className="relative w-full max-w-xs">
          <Search
            aria-hidden="true"
            className="pointer-events-none absolute top-1/2 left-2.5 size-4 -translate-y-1/2 text-muted-foreground"
          />
          <Input
            type="search"
            className="pl-8"
            aria-label={t("users.search")}
            placeholder={t("users.search")}
            value={text}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
        </div>
        <Select
          items={roles}
          value={search.role ?? allRoles}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              role: value === allRoles ? undefined : (value as Role),
              page: undefined,
            });
          }}
        >
          <SelectTrigger aria-label={t("users.filters.role")}>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {roles.map((role) => (
              <SelectItem key={role.value} value={role.value}>
                {role.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select
          items={statuses}
          value={search.status ?? UserStatusFilter.active}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              status: value === UserStatusFilter.active ? undefined : (value as UserStatusFilter),
              page: undefined,
            });
          }}
        >
          <SelectTrigger aria-label={t("users.filters.statusLabel")}>
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
        label={t("users.title")}
        columns={columns}
        rows={users.data?.items}
        getRowId={(row) => row.id}
        isLoading={users.isPending}
        error={users.error}
        onRetry={() => {
          void users.refetch();
        }}
        empty={<EmptyState icon={UserRound} title={t("users.empty")} />}
        sort={search.sort}
        sortable={["fullName", "email"]}
        onSortChange={(sort) => {
          onSearchChange({ ...search, sort, page: undefined });
        }}
        page={page}
        pageSize={pageSize}
        totalCount={users.data?.totalCount ?? 0}
        onPageChange={(next) => {
          onSearchChange({ ...search, page: next === 1 ? undefined : next });
        }}
        onPageSizeChange={(size) => {
          onSearchChange({ ...search, pageSize: size === 25 ? undefined : size, page: undefined });
        }}
      />
    </div>
  );
}
