import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Lock, SearchX } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateEquipmentModel,
  deactivateEquipmentModel,
  getEquipmentModel,
  getGetEquipmentModelQueryKey,
  getListEquipmentCategoriesQueryKey,
  getListEquipmentModelsQueryKey,
} from "@/api/endpoints/catalog/catalog";
import type { EquipmentModelDetails } from "@/api/model";
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
import { formatDecimal } from "@/lib/format";
import { auditPermissions, HistoryTab } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { catalogPermissions } from "../permissions";

export type ModelDetailTab = "general" | "history";

export type ModelDetailPageProps = {
  modelId: string;
  tab: ModelDetailTab;
};

// A model's page (11 §3): its category, tracking type and technical values, the kits that hold it, and
// its change history.
export function ModelDetailPage({ modelId, tab }: ModelDetailPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const canView = hasPermission(user, catalogPermissions.viewModels);
  const model = useQuery({
    queryKey: getGetEquipmentModelQueryKey(modelId),
    queryFn: ({ signal }) => getEquipmentModel(modelId, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("models.forbidden")} />
      </div>
    );
  }

  if (model.isError) {
    return (
      <div className="p-6">
        {model.error instanceof ApiError && model.error.status === 404 ? (
          <EmptyState icon={SearchX} title={t("modelDetail.notFound")} />
        ) : (
          <ErrorState
            error={model.error}
            onRetry={() => {
              void model.refetch();
            }}
          />
        )}
      </div>
    );
  }

  if (model.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-64 w-full" />
      </div>
    );
  }

  return <ModelDetail model={model.data} tab={tab} />;
}

function ModelDetail({ model, tab }: { model: EquipmentModelDetails; tab: ModelDetailTab }) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, catalogPermissions.editModels);
  const canDeactivate = hasPermission(user, catalogPermissions.deactivateModels);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [isConfirming, setIsConfirming] = useState(false);
  const isActive = model.deactivatedAt === null;
  const name = `${model.brand} ${model.name}`;
  const status = useMutation({
    mutationFn: () =>
      (isActive ? deactivateEquipmentModel : activateEquipmentModel)(
        model.id,
        ifMatch(model.version),
      ),
    onSuccess: (saved) => {
      setIsConfirming(false);
      queryClient.setQueryData(getGetEquipmentModelQueryKey(saved.id), saved);
      void queryClient.invalidateQueries({ queryKey: getListEquipmentModelsQueryKey() });
      void queryClient.invalidateQueries({ queryKey: getListEquipmentCategoriesQueryKey() });
      toast.success(t(isActive ? "models.done.deactivated" : "models.done.activated"));
    },
    onError: (error) => {
      setIsConfirming(false);
      toast.error(errorMessage(error));
    },
  });

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={name}
        description={model.categoryPath.join(" › ")}
        breadcrumb={
          <Link to="/catalog/models" className="hover:underline">
            {t("models.title")}
          </Link>
        }
        actions={
          <>
            {!isActive && <StatusBadge tone="muted" label={t("models.status.inactive")} />}
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
                {isActive ? t("models.actions.deactivate") : t("models.actions.activate")}
              </Button>
            )}
            {canEdit && (
              <Button
                render={<Link to="/catalog/models/$modelId/edit" params={{ modelId: model.id }} />}
              >
                {t("models.actions.edit")}
              </Button>
            )}
          </>
        }
      />
      <nav aria-label={t("modelDetail.tabs")} className="flex gap-1 border-b">
        <TabLink to="/catalog/models/$modelId" modelId={model.id} isCurrent={tab === "general"}>
          {t("modelDetail.general")}
        </TabLink>
        {canViewHistory && (
          <TabLink
            to="/catalog/models/$modelId/history"
            modelId={model.id}
            isCurrent={tab === "history"}
          >
            {t("modelDetail.history")}
          </TabLink>
        )}
      </nav>
      {tab === "history" ? (
        <HistoryTab rootType="EquipmentModel" rootId={model.id} />
      ) : (
        <section className="max-w-2xl rounded-lg border p-4">
          <dl className="grid gap-x-6 gap-y-3 text-sm sm:grid-cols-[12rem_1fr]">
            <Row label={t("modelForm.brand")}>{model.brand}</Row>
            <Row label={t("modelForm.name")}>{model.name}</Row>
            <Row label={t("modelForm.category")}>{model.categoryPath.join(" › ")}</Row>
            <Row label={t("modelForm.trackingType")}>
              {t(`trackingTypes.${model.trackingType}`)}
              {model.hasStock && (
                <span className="ml-2 text-muted-foreground">({t("modelDetail.hasStock")})</span>
              )}
            </Row>
            <Row label={t("modelForm.weightKilograms")}>{formatDecimal(model.weightKilograms)}</Row>
            <Row label={t("modelForm.powerWatts")}>
              {model.powerWatts === null ? "—" : model.powerWatts.toLocaleString("tr-TR")}
            </Row>
            <Row label={t("modelForm.transportVolumeCubicMeters")}>
              {formatDecimal(model.transportVolumeCubicMeters)}
            </Row>
          </dl>
          <h2 className="mt-6 mb-2 text-base font-semibold">{t("modelDetail.kits")}</h2>
          {model.kits.length === 0 ? (
            <p className="text-sm text-muted-foreground">{t("modelDetail.noKits")}</p>
          ) : (
            <ul className="flex flex-col gap-1 text-sm">
              {model.kits.map((kit) => (
                <li key={kit.id} className="flex items-center gap-2">
                  <Link
                    to="/catalog/kits/$kitId"
                    params={{ kitId: kit.id }}
                    className="font-medium hover:underline"
                  >
                    {kit.name}
                  </Link>
                  {!kit.isActive && (
                    <StatusBadge tone="muted" label={t("models.status.inactive")} />
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      )}
      <ConfirmDialog
        open={isConfirming}
        onOpenChange={setIsConfirming}
        title={t("models.confirm.deactivateTitle", { name })}
        description={t("models.confirm.deactivateDescription")}
        confirmLabel={t("models.actions.deactivate")}
        isPending={status.isPending}
        onConfirm={() => {
          status.mutate();
        }}
      />
    </div>
  );
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <>
      <dt className="text-muted-foreground">{label}</dt>
      <dd className="font-medium">{children}</dd>
    </>
  );
}

type TabLinkProps = {
  to: "/catalog/models/$modelId" | "/catalog/models/$modelId/history";
  modelId: string;
  isCurrent: boolean;
  children: ReactNode;
};

function TabLink({ to, modelId, isCurrent, children }: TabLinkProps) {
  return (
    <Link
      to={to}
      params={{ modelId }}
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
