import { useQuery } from "@tanstack/react-query";
import type { ParseKeys, TFunction } from "i18next";
import { Check, Lock, Minus } from "lucide-react";
import { Fragment } from "react";
import { useTranslation } from "react-i18next";

import { getListRolesQueryKey, listRoles } from "@/api/endpoints/identity/identity";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { PageHeader } from "@/components/common/page-header";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { hasPermission, identityPermissions, useSignedInUser } from "../permissions";

// Modules and permissions come from the server as codes; their names are keyed by the code, and a code
// without a name yet is shown as it is.
function nameOf(t: TFunction<"identity">, group: "modules" | "permissions", code: string): string {
  const key = `${group}.${code}` as ParseKeys<"identity">;
  const name = t(key);
  return name === key ? code : name;
}

// The read-only role and permission matrix (US-SYS-003): roles as columns, permissions as rows,
// grouped by module. The server reads it from the same definition it checks (identity §5.2).
export function RolesPage() {
  const { t } = useTranslation("identity");
  const user = useSignedInUser();
  const canView = hasPermission(user, identityPermissions.viewRoles);
  const matrix = useQuery({
    queryKey: getListRolesQueryKey(),
    queryFn: ({ signal }) => listRoles({ signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("users.forbidden")} />
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader title={t("rolesPage.title")} description={t("rolesPage.description")} />
      {matrix.isError ? (
        <ErrorState
          error={matrix.error}
          onRetry={() => {
            void matrix.refetch();
          }}
        />
      ) : matrix.data === undefined ? (
        <SkeletonBlock className="h-64 w-full" />
      ) : (
        <div className="overflow-auto rounded-md border">
          <Table aria-label={t("rolesPage.title")}>
            <TableHeader className="sticky top-0 z-10 bg-card">
              <TableRow>
                <TableHead>{t("rolesPage.permission")}</TableHead>
                {matrix.data.roles.map((role) => (
                  <TableHead key={role} className="text-center">
                    {t(`roles.${role}`)}
                  </TableHead>
                ))}
              </TableRow>
            </TableHeader>
            <TableBody>
              {matrix.data.modules.map((module) => (
                <Fragment key={module.module}>
                  <TableRow>
                    <TableHead
                      scope="rowgroup"
                      colSpan={matrix.data.roles.length + 1}
                      className="bg-muted/50 text-foreground"
                    >
                      {nameOf(t, "modules", module.module)}
                    </TableHead>
                  </TableRow>
                  {module.permissions.map((grant) => (
                    <TableRow key={grant.code}>
                      <TableHead scope="row" className="font-normal">
                        {nameOf(t, "permissions", grant.code)}
                      </TableHead>
                      {matrix.data.roles.map((role) => (
                        <TableCell key={role} className="text-center">
                          {grant.roles.includes(role) ? (
                            <>
                              <Check
                                aria-hidden="true"
                                className="mx-auto size-4 text-status-success"
                              />
                              <span className="sr-only">{t("rolesPage.granted")}</span>
                            </>
                          ) : (
                            <>
                              <Minus
                                aria-hidden="true"
                                className="mx-auto size-4 text-muted-foreground"
                              />
                              <span className="sr-only">{t("rolesPage.notGranted")}</span>
                            </>
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </Fragment>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}
