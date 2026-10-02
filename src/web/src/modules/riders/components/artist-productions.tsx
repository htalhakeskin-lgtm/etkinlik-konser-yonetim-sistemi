import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "@tanstack/react-router";
import { Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  getGetProductionQueryKey,
  getListProductionsQueryKey,
  listProductions,
} from "@/api/endpoints/riders/riders";
import { ErrorState } from "@/components/common/error-state";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { ridersPermissions } from "../permissions";
import { type ProductionEdit, ProductionFormDialog } from "./production-form-dialog";

// An artist has few productions; one page shows them all.
const pageSize = 100;

export type ArtistProductionsProps = {
  artistId: string;
  artistName: string;
  /** A deactivated artist gets no new production (BR-PTY-004). */
  isArtistActive: boolean;
};

// The artist page's productions (US-ART-001, riders §8): name, newest rider version and status, active and
// inactive ones; adding opens the production's page.
export function ArtistProductions({
  artistId,
  artistName,
  isArtistActive,
}: ArtistProductionsProps) {
  const { t } = useTranslation("riders");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const canView = hasPermission(user, ridersPermissions.viewProductions);
  const canCreate = hasPermission(user, ridersPermissions.createProductions);
  const [edit, setEdit] = useState<ProductionEdit | undefined>();
  const params = { artistId, status: "all", pageSize } as const;
  const productions = useQuery({
    queryKey: getListProductionsQueryKey(params),
    queryFn: ({ signal }) => listProductions(params, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return null;
  }

  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4 lg:col-span-2">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-base font-semibold">{t("artistProductions.title")}</h2>
        {canCreate && isArtistActive && (
          <Button
            size="sm"
            variant="outline"
            onClick={() => {
              setEdit({ mode: "create", artistId, artistName });
            }}
          >
            <Plus aria-hidden="true" />
            {t("artistProductions.add")}
          </Button>
        )}
      </div>
      {productions.isError ? (
        <ErrorState
          error={productions.error}
          onRetry={() => {
            void productions.refetch();
          }}
        />
      ) : productions.data === undefined ? (
        <SkeletonBlock className="h-24 w-full" />
      ) : productions.data.items.length === 0 ? (
        <p className="text-sm text-muted-foreground">{t("artistProductions.empty")}</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("artistProductions.name")}</TableHead>
              <TableHead>{t("artistProductions.latestVersion")}</TableHead>
              <TableHead>{t("artistProductions.status")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {productions.data.items.map((production) => (
              <TableRow key={production.id}>
                <TableCell className="font-medium">
                  <Link
                    to="/productions/$productionId"
                    params={{ productionId: production.id }}
                    className="hover:underline"
                  >
                    {production.name}
                  </Link>
                </TableCell>
                <TableCell>
                  {production.latestVersionNumber === 0
                    ? t("rider.none")
                    : t("rider.versionNumber", { number: production.latestVersionNumber })}
                </TableCell>
                <TableCell>
                  <StatusBadge
                    tone={production.isActive ? "active" : "muted"}
                    label={t(production.isActive ? "status.active" : "status.inactive")}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      <ProductionFormDialog
        edit={edit}
        onClose={() => {
          setEdit(undefined);
        }}
        onSaved={(saved) => {
          setEdit(undefined);
          queryClient.setQueryData(getGetProductionQueryKey(saved.id), saved);
          void queryClient.invalidateQueries({ queryKey: getListProductionsQueryKey() });
          toast.success(t("done.created"));
          void navigate({ to: "/productions/$productionId", params: { productionId: saved.id } });
        }}
      />
    </section>
  );
}
