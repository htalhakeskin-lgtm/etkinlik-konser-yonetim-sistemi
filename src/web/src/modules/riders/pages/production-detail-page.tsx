import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { ClipboardList, Lock, SearchX } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateProduction,
  deactivateProduction,
  getGetProductionQueryKey,
  getListProductionsQueryKey,
  getProduction,
} from "@/api/endpoints/riders/riders";
import type { ProductionDetails } from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { PageHeader } from "@/components/common/page-header";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
import { Button } from "@/components/ui/button";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";
import { auditPermissions, HistoryTab } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { type ProductionEdit, ProductionFormDialog } from "../components/production-form-dialog";
import { ridersPermissions } from "../permissions";

export type ProductionDetailTab = "rider" | "history";

export type ProductionDetailPageProps = {
  productionId: string;
  tab: ProductionDetailTab;
};

// A production's page (riders §8): its artist and status in the header, its rider and its change history in
// tabs kept in the address.
export function ProductionDetailPage({ productionId, tab }: ProductionDetailPageProps) {
  const { t } = useTranslation("riders");
  const user = useSignedInUser();
  const canView = hasPermission(user, ridersPermissions.viewProductions);
  const production = useQuery({
    queryKey: getGetProductionQueryKey(productionId),
    queryFn: ({ signal }) => getProduction(productionId, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("forbidden")} />
      </div>
    );
  }

  if (production.isError) {
    return (
      <div className="p-6">
        {production.error instanceof ApiError && production.error.status === 404 ? (
          <EmptyState icon={SearchX} title={t("productionDetail.notFound")} />
        ) : (
          <ErrorState
            error={production.error}
            onRetry={() => {
              void production.refetch();
            }}
          />
        )}
      </div>
    );
  }

  if (production.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-64 w-full" />
      </div>
    );
  }

  return <ProductionDetail production={production.data} tab={tab} />;
}

function ProductionDetail({
  production,
  tab,
}: {
  production: ProductionDetails;
  tab: ProductionDetailTab;
}) {
  const { t } = useTranslation("riders");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, ridersPermissions.editProductions);
  const canDeactivate = hasPermission(user, ridersPermissions.deactivateProductions);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [edit, setEdit] = useState<ProductionEdit | undefined>();
  const [isConfirming, setIsConfirming] = useState(false);
  const isActive = production.deactivatedAt === null;

  const show = (saved: ProductionDetails) => {
    queryClient.setQueryData(getGetProductionQueryKey(saved.id), saved);
    void queryClient.invalidateQueries({ queryKey: getListProductionsQueryKey() });
  };
  const status = useMutation({
    mutationFn: () =>
      (isActive ? deactivateProduction : activateProduction)(
        production.id,
        ifMatch(production.version),
      ),
    onSuccess: (saved) => {
      setIsConfirming(false);
      show(saved);
      toast.success(t(isActive ? "done.deactivated" : "done.activated"));
    },
    onError: (error) => {
      setIsConfirming(false);
      toast.error(errorMessage(error));
    },
  });

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={production.name}
        description={production.description ?? undefined}
        breadcrumb={
          <span className="inline-flex items-center gap-2">
            <Link to="/artists" className="hover:underline">
              {t("productionDetail.artists")}
            </Link>
            <span aria-hidden="true">›</span>
            <Link
              to="/artists/$partyId"
              params={{ partyId: production.artistPartyId }}
              className="hover:underline"
            >
              {production.artistName ?? t("productionDetail.unknownArtist")}
            </Link>
            {!production.isArtistActive && (
              <StatusBadge tone="muted" label={t("status.inactive")} />
            )}
          </span>
        }
        actions={
          <>
            {!isActive && <StatusBadge tone="muted" label={t("status.inactive")} />}
            {canDeactivate && (
              <Button
                variant="outline"
                isLoading={status.isPending && !isConfirming}
                onClick={() => {
                  if (isActive) {
                    setIsConfirming(true);
                  } else {
                    status.mutate();
                  }
                }}
              >
                {isActive ? t("actions.deactivate") : t("actions.activate")}
              </Button>
            )}
            {canEdit && (
              <Button
                onClick={() => {
                  setEdit({ mode: "edit", production });
                }}
              >
                {t("actions.edit")}
              </Button>
            )}
          </>
        }
      />
      <nav aria-label={t("productionDetail.tabs")} className="flex gap-1 border-b">
        <TabLink
          to="/productions/$productionId"
          productionId={production.id}
          isCurrent={tab === "rider"}
        >
          {t("productionDetail.rider")}
        </TabLink>
        {canViewHistory && (
          <TabLink
            to="/productions/$productionId/history"
            productionId={production.id}
            isCurrent={tab === "history"}
          >
            {t("productionDetail.history")}
          </TabLink>
        )}
      </nav>
      {tab === "history" ? (
        <HistoryTab rootType="Production" rootId={production.id} />
      ) : production.latestVersionNumber === 0 ? (
        <EmptyState
          icon={ClipboardList}
          title={t("rider.none")}
          description={t("rider.noneDescription")}
        />
      ) : (
        <p className="text-sm text-muted-foreground">
          {t("rider.versionNumber", { number: production.latestVersionNumber })}
        </p>
      )}
      <ProductionFormDialog
        edit={edit}
        onClose={() => {
          setEdit(undefined);
        }}
        onSaved={(saved) => {
          setEdit(undefined);
          show(saved);
          toast.success(t("done.saved"));
        }}
      />
      <ConfirmDialog
        open={isConfirming}
        onOpenChange={setIsConfirming}
        title={t("confirm.deactivateTitle", { name: production.name })}
        description={t("confirm.deactivateDescription")}
        confirmLabel={t("actions.deactivate")}
        isPending={status.isPending}
        onConfirm={() => {
          status.mutate();
        }}
      />
    </div>
  );
}

type TabLinkProps = {
  to: "/productions/$productionId" | "/productions/$productionId/history";
  productionId: string;
  isCurrent: boolean;
  children: ReactNode;
};

function TabLink({ to, productionId, isCurrent, children }: TabLinkProps) {
  return (
    <Link
      to={to}
      params={{ productionId }}
      aria-current={isCurrent ? "page" : undefined}
      className={
        isCurrent
          ? "-mb-px border-b-2 border-primary px-3 py-2 text-sm font-medium"
          : "-mb-px border-b-2 border-transparent px-3 py-2 text-sm text-muted-foreground hover:text-foreground"
      }
    >
      {children}
    </Link>
  );
}
