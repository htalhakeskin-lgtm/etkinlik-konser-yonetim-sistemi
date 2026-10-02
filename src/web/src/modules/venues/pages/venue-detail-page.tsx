import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Lock, SearchX } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateVenue,
  deactivateVenue,
  getGetVenueQueryKey,
  getListVenuesQueryKey,
  getVenue,
} from "@/api/endpoints/venues/venues";
import type { VenueDetails } from "@/api/model";
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

import { VenueFormDialog } from "../components/venue-form-dialog";
import { venuesPermissions } from "../permissions";

export type VenueDetailTab = "general" | "history";

export type VenueDetailPageProps = {
  venueId: string;
  tab: VenueDetailTab;
};

// A venue's page (11 §3): its details with tabs in the address; the equipment tab joins with the venue
// equipment.
export function VenueDetailPage({ venueId, tab }: VenueDetailPageProps) {
  const { t } = useTranslation("venues");
  const user = useSignedInUser();
  const canView = hasPermission(user, venuesPermissions.viewVenues);
  const venue = useQuery({
    queryKey: getGetVenueQueryKey(venueId),
    queryFn: ({ signal }) => getVenue(venueId, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("venues.forbidden")} />
      </div>
    );
  }

  if (venue.isError) {
    return (
      <div className="p-6">
        {venue.error instanceof ApiError && venue.error.status === 404 ? (
          <EmptyState icon={SearchX} title={t("venueDetail.notFound")} />
        ) : (
          <ErrorState
            error={venue.error}
            onRetry={() => {
              void venue.refetch();
            }}
          />
        )}
      </div>
    );
  }

  if (venue.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-64 w-full" />
      </div>
    );
  }

  return <VenueDetail venue={venue.data} tab={tab} />;
}

function VenueDetail({ venue, tab }: { venue: VenueDetails; tab: VenueDetailTab }) {
  const { t } = useTranslation("venues");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, venuesPermissions.editVenues);
  const canDeactivate = hasPermission(user, venuesPermissions.deactivateVenues);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [isEditing, setIsEditing] = useState(false);
  const [isConfirming, setIsConfirming] = useState(false);
  const isActive = venue.deactivatedAt === null;

  const show = (saved: VenueDetails) => {
    queryClient.setQueryData(getGetVenueQueryKey(saved.id), saved);
    void queryClient.invalidateQueries({ queryKey: getListVenuesQueryKey() });
  };
  const status = useMutation({
    mutationFn: () =>
      (isActive ? deactivateVenue : activateVenue)(venue.id, ifMatch(venue.version)),
    onSuccess: (saved) => {
      setIsConfirming(false);
      show(saved);
      toast.success(t(isActive ? "venues.done.deactivated" : "venues.done.activated"));
    },
    onError: (error) => {
      setIsConfirming(false);
      toast.error(errorMessage(error));
    },
  });
  const meters = (value: string | null) => (value === null ? "—" : `${formatDecimal(value)} m`);

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={venue.name}
        description={`${venue.city} · ${t("venueDetail.capacity", { count: venue.capacity })}`}
        breadcrumb={
          <Link to="/venues" className="hover:underline">
            {t("venues.title")}
          </Link>
        }
        actions={
          <>
            {!isActive && <StatusBadge tone="muted" label={t("venues.status.inactive")} />}
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
                {isActive ? t("venues.actions.deactivate") : t("venues.actions.activate")}
              </Button>
            )}
            {canEdit && (
              <Button
                onClick={() => {
                  setIsEditing(true);
                }}
              >
                {t("venues.actions.edit")}
              </Button>
            )}
          </>
        }
      />
      <nav aria-label={t("venueDetail.tabs")} className="flex gap-1 border-b">
        <TabLink to="/venues/$venueId" venueId={venue.id} isCurrent={tab === "general"}>
          {t("venueDetail.general")}
        </TabLink>
        {canViewHistory && (
          <TabLink to="/venues/$venueId/history" venueId={venue.id} isCurrent={tab === "history"}>
            {t("venueDetail.history")}
          </TabLink>
        )}
      </nav>
      {tab === "history" ? (
        <HistoryTab rootType="Venue" rootId={venue.id} />
      ) : (
        <section className="max-w-3xl rounded-lg border p-4">
          <dl className="grid gap-x-6 gap-y-3 text-sm sm:grid-cols-[14rem_1fr]">
            <Row label={t("venueForm.address")}>{venue.address}</Row>
            <Row label={t("venueForm.capacity")}>{venue.capacity.toLocaleString("tr-TR")}</Row>
            <Row label={t("venueForm.operator")}>
              {venue.operatorPartyId === null ? (
                "—"
              ) : (
                <span className="inline-flex items-center gap-2">
                  <Link
                    to="/parties/$partyId"
                    params={{ partyId: venue.operatorPartyId }}
                    className="hover:underline"
                  >
                    {venue.operatorName}
                  </Link>
                  {!venue.isOperatorActive && (
                    <StatusBadge tone="muted" label={t("venues.status.inactive")} />
                  )}
                </span>
              )}
            </Row>
            <Row label={t("venueDetail.stage")}>
              {[venue.stageWidthMeters, venue.stageDepthMeters, venue.stageHeightMeters]
                .map(meters)
                .join(" × ")}
            </Row>
            <Row label={t("venueForm.loadingDock")}>{venue.loadingDock ?? "—"}</Row>
            <Row label={t("venueForm.powerCapacityAmperes")}>
              {venue.powerCapacityAmperes === null
                ? "—"
                : `${formatDecimal(venue.powerCapacityAmperes)} A`}
            </Row>
            <Row label={t("venueForm.curfew")}>{venue.curfew?.slice(0, 5) ?? "—"}</Row>
            <Row label={t("venueForm.timeZone")}>{venue.timeZone}</Row>
          </dl>
        </section>
      )}
      <VenueFormDialog
        open={isEditing}
        onOpenChange={setIsEditing}
        venueId={venue.id}
        onSaved={(saved) => {
          setIsEditing(false);
          show(saved);
          toast.success(t("venues.done.saved"));
        }}
      />
      <ConfirmDialog
        open={isConfirming}
        onOpenChange={setIsConfirming}
        title={t("venues.confirm.deactivateTitle", { name: venue.name })}
        description={t("venues.confirm.deactivateDescription")}
        confirmLabel={t("venues.actions.deactivate")}
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
  to: "/venues/$venueId" | "/venues/$venueId/history";
  venueId: string;
  isCurrent: boolean;
  children: ReactNode;
};

function TabLink({ to, venueId, isCurrent, children }: TabLinkProps) {
  return (
    <Link
      to={to}
      params={{ venueId }}
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
