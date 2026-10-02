import { useQuery } from "@tanstack/react-query";
import { Lock } from "lucide-react";
import { useTranslation } from "react-i18next";

import { getListUsersQueryKey, listUsers } from "@/api/endpoints/identity/identity";
import type { ListUsersParams } from "@/api/model";
import { EmptyState } from "@/components/common/empty-state";
import { PageHeader } from "@/components/common/page-header";
import { Button } from "@/components/ui/button";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { hasPermission, identityPermissions, useSignedInUser } from "@/modules/identity";

import { entityTypeName } from "../changes";
import { HistoryList } from "../components/history-list";
import { auditPermissions } from "../permissions";

/** The history screen's state in the address (ui §5.1); the days are both included. */
export type AuditSearch = {
  actorId?: string | undefined;
  from?: string | undefined;
  to?: string | undefined;
  /** The record type; a type's parts (a party's contact points) come with it. */
  rootType?: string | undefined;
};

export type AuditPageProps = {
  search: AuditSearch;
  onSearchChange: (search: AuditSearch) => void;
};

const allValues = "all";
const recordTypes = ["User", "Warehouse", "Party", "EquipmentCategory"];
const everyUser: ListUsersParams = { status: "all", pageSize: 100 };

// The screen shows the last day as included; the API's range stops before its `to` day (api §6.3).
function dayAfter(day: string): string {
  const date = new Date(`${day}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + 1);
  return date.toISOString().slice(0, 10);
}

// The general change history (US-SYS-004 criteria 2–3): filtered by user, days and record type.
export function AuditPage({ search, onSearchChange }: AuditPageProps) {
  const { t } = useTranslation("audit");
  const user = useSignedInUser();
  const canView = hasPermission(user, auditPermissions.viewEntries);
  const canListUsers = hasPermission(user, identityPermissions.viewUsers);
  const users = useQuery({
    queryKey: getListUsersQueryKey(everyUser),
    queryFn: ({ signal }) => listUsers(everyUser, { signal }),
    enabled: canView && canListUsers,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("forbidden")} />
      </div>
    );
  }

  const actors = [
    { value: allValues, label: t("filters.allActors") },
    ...(users.data?.items ?? []).map((listed) => ({ value: listed.id, label: listed.fullName })),
  ];
  const types = [
    { value: allValues, label: t("filters.allEntityTypes") },
    ...recordTypes.map((type) => ({ value: type, label: entityTypeName(type) })),
  ];
  const isFiltered = [search.actorId, search.from, search.to, search.rootType].some(
    (value) => value !== undefined,
  );

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader title={t("title")} description={t("description")} />
      <div className="flex flex-wrap items-end gap-3">
        {canListUsers && (
          <Select
            items={actors}
            value={search.actorId ?? allValues}
            onValueChange={(value) => {
              onSearchChange({
                ...search,
                actorId: value === null || value === allValues ? undefined : value,
              });
            }}
          >
            <SelectTrigger aria-label={t("filters.actor")}>
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {actors.map((actor) => (
                <SelectItem key={actor.value} value={actor.value}>
                  {actor.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        )}
        <Field className="w-auto">
          <FieldLabel htmlFor="audit-from">{t("filters.from")}</FieldLabel>
          <Input
            id="audit-from"
            type="date"
            value={search.from ?? ""}
            onChange={(event) => {
              onSearchChange({
                ...search,
                from: event.target.value === "" ? undefined : event.target.value,
              });
            }}
          />
        </Field>
        <Field className="w-auto">
          <FieldLabel htmlFor="audit-to">{t("filters.to")}</FieldLabel>
          <Input
            id="audit-to"
            type="date"
            value={search.to ?? ""}
            onChange={(event) => {
              onSearchChange({
                ...search,
                to: event.target.value === "" ? undefined : event.target.value,
              });
            }}
          />
        </Field>
        <Select
          items={types}
          value={search.rootType ?? allValues}
          onValueChange={(value) => {
            onSearchChange({
              ...search,
              rootType: value === null || value === allValues ? undefined : value,
            });
          }}
        >
          <SelectTrigger aria-label={t("filters.entityType")}>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {types.map((type) => (
              <SelectItem key={type.value} value={type.value}>
                {type.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {isFiltered && (
          <Button
            variant="ghost"
            onClick={() => {
              onSearchChange({});
            }}
          >
            {t("filters.clear")}
          </Button>
        )}
      </div>
      <HistoryList
        filter={{
          actorId: search.actorId,
          from: search.from,
          to: search.to === undefined ? undefined : dayAfter(search.to),
          rootType: search.rootType,
        }}
      />
    </div>
  );
}
