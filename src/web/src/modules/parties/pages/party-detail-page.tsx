import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Lock, Plus, SearchX } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateParty,
  addContactPerson,
  addRepresentation,
  deactivateParty,
  editContactPerson,
  editRepresentation,
  getGetPartyQueryKey,
  getListPartiesQueryKey,
  getParty,
  removeContactPerson,
  removeRepresentation,
} from "@/api/endpoints/parties/parties";
import { type PartyDetails, PartyKind, PartyRole } from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { PageHeader } from "@/components/common/page-header";
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
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";
import { auditPermissions, HistoryTab } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { PartyFormDialog } from "../components/party-form-dialog";
import { type PartyLink, PartyLinkDialog } from "../components/party-link-dialog";
import { partiesPermissions } from "../permissions";

export type PartyDetailTab = "general" | "history";

export type PartyDetailPageProps = {
  partyId: string;
  tab: PartyDetailTab;
};

type Removal =
  | { kind: "contactPerson"; id: string; name: string }
  | { kind: "representation"; id: string; name: string };

// A party's page (11 §3): its names, roles and contact points; an organization's contact persons, an
// artist's agencies, an agency's artists and a person's employers; and its change history.
export function PartyDetailPage({ partyId, tab }: PartyDetailPageProps) {
  const { t } = useTranslation("parties");
  const user = useSignedInUser();
  const canView = hasPermission(user, partiesPermissions.viewParties);
  const party = useQuery({
    queryKey: getGetPartyQueryKey(partyId),
    queryFn: ({ signal }) => getParty(partyId, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("parties.forbidden")} />
      </div>
    );
  }

  if (party.isError) {
    return (
      <div className="p-6">
        {party.error instanceof ApiError && party.error.status === 404 ? (
          <EmptyState icon={SearchX} title={t("detail.notFound")} />
        ) : (
          <ErrorState
            error={party.error}
            onRetry={() => {
              void party.refetch();
            }}
          />
        )}
      </div>
    );
  }

  if (party.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-64 w-full" />
      </div>
    );
  }

  return <PartyDetail party={party.data} tab={tab} />;
}

function PartyDetail({ party, tab }: { party: PartyDetails; tab: PartyDetailTab }) {
  const { t } = useTranslation("parties");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, partiesPermissions.editParties);
  const canDeactivate = hasPermission(user, partiesPermissions.deactivateParties);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [isEditing, setIsEditing] = useState(false);
  const [isConfirming, setIsConfirming] = useState(false);
  const [contactLink, setContactLink] = useState<(PartyLink & { id?: string }) | undefined>();
  const [agencyLink, setAgencyLink] = useState<(PartyLink & { id?: string }) | undefined>();
  const [removing, setRemoving] = useState<Removal | undefined>(undefined);
  const isActive = party.deactivatedAt === null;

  const show = (saved: PartyDetails) => {
    queryClient.setQueryData(getGetPartyQueryKey(saved.id), saved);
    void queryClient.invalidateQueries({ queryKey: getListPartiesQueryKey() });
  };
  const status = useMutation({
    mutationFn: () =>
      (isActive ? deactivateParty : activateParty)(party.id, ifMatch(party.version)),
    onSuccess: (saved) => {
      setIsConfirming(false);
      show(saved);
      toast.success(t(isActive ? "parties.done.deactivated" : "parties.done.activated"));
    },
    onError: (error) => {
      setIsConfirming(false);
      toast.error(errorMessage(error));
    },
  });
  const removal = useMutation({
    mutationFn: (target: Removal) =>
      target.kind === "contactPerson"
        ? removeContactPerson(party.id, target.id, ifMatch(party.version))
        : removeRepresentation(party.id, target.id, ifMatch(party.version)),
    onSuccess: (saved) => {
      setRemoving(undefined);
      show(saved);
      toast.success(t("link.removed"));
    },
    onError: (error) => {
      setRemoving(undefined);
      toast.error(errorMessage(error));
    },
  });

  const isArtistSide = party.roles.includes(PartyRole.artist) || party.representations.length > 0;
  const isAgencySide =
    party.roles.includes(PartyRole.agency) || party.representedArtists.length > 0;

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={party.name}
        description={[
          t(`kinds.${party.kind}`),
          ...party.roles.map((role) => t(`roles.${role}`)),
        ].join(" · ")}
        breadcrumb={
          <Link to="/parties" className="hover:underline">
            {t("parties.title")}
          </Link>
        }
        actions={
          <>
            {!isActive && <StatusBadge tone="muted" label={t("parties.status.inactive")} />}
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
                {isActive ? t("parties.actions.deactivate") : t("parties.actions.activate")}
              </Button>
            )}
            {canEdit && (
              <Button
                onClick={() => {
                  setIsEditing(true);
                }}
              >
                {t("parties.actions.edit")}
              </Button>
            )}
          </>
        }
      />
      <nav aria-label={t("detail.tabs")} className="flex gap-1 border-b">
        <TabLink to="/parties/$partyId" partyId={party.id} isCurrent={tab === "general"}>
          {t("detail.general")}
        </TabLink>
        {canViewHistory && (
          <TabLink to="/parties/$partyId/history" partyId={party.id} isCurrent={tab === "history"}>
            {t("detail.history")}
          </TabLink>
        )}
      </nav>
      {tab === "history" ? (
        <HistoryTab rootType="Party" rootId={party.id} />
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          <Section title={t("detail.contactPoints")}>
            {party.contactPoints.length === 0 ? (
              <p className="text-sm text-muted-foreground">{t("partyForm.noContactPoints")}</p>
            ) : (
              <ul className="flex flex-col gap-2 text-sm">
                {party.contactPoints.map((point) => (
                  <li key={point.id} className="flex flex-wrap items-center gap-2">
                    <span className="w-20 text-muted-foreground">
                      {t(`contactKinds.${point.kind}`)}
                    </span>
                    <span className="font-medium">{point.value}</span>
                    {point.label !== null && (
                      <span className="text-muted-foreground">({point.label})</span>
                    )}
                    {point.isPrimary && <StatusBadge tone="info" label={t("partyForm.primary")} />}
                  </li>
                ))}
              </ul>
            )}
          </Section>
          {party.kind === PartyKind.organization && (
            <Section
              title={t("detail.contactPersons")}
              action={
                canEdit && isActive ? (
                  <AddButton
                    label={t("link.addContactPerson")}
                    onClick={() => {
                      setContactLink({ mode: "add" });
                    }}
                  />
                ) : undefined
              }
            >
              <LinkTable
                empty={t("detail.noContactPersons")}
                columns={[
                  t("detail.name"),
                  t("detail.title"),
                  t("detail.phone"),
                  t("detail.email"),
                ]}
                rows={party.contactPersons.map((contact) => ({
                  id: contact.id,
                  partyId: contact.personId,
                  name: contact.name,
                  isActive: contact.isActive,
                  cells: [contact.title, contact.primaryPhone, contact.primaryEmail],
                  onEdit: canEdit
                    ? () => {
                        setContactLink({
                          mode: "edit",
                          id: contact.id,
                          otherName: contact.name,
                          text: contact.title,
                        });
                      }
                    : undefined,
                  onRemove: canEdit
                    ? () => {
                        setRemoving({ kind: "contactPerson", id: contact.id, name: contact.name });
                      }
                    : undefined,
                }))}
              />
            </Section>
          )}
          {isArtistSide && (
            <Section
              title={t("detail.agencies")}
              action={
                canEdit && isActive ? (
                  <AddButton
                    label={t("link.addAgency")}
                    onClick={() => {
                      setAgencyLink({ mode: "add" });
                    }}
                  />
                ) : undefined
              }
            >
              <LinkTable
                empty={t("detail.noAgencies")}
                columns={[t("detail.name"), t("detail.description")]}
                rows={party.representations.map((representation) => ({
                  id: representation.id,
                  partyId: representation.agencyId,
                  name: representation.agencyName,
                  isActive: representation.isActive,
                  cells: [representation.description],
                  onEdit: canEdit
                    ? () => {
                        setAgencyLink({
                          mode: "edit",
                          id: representation.id,
                          otherName: representation.agencyName,
                          text: representation.description,
                        });
                      }
                    : undefined,
                  onRemove: canEdit
                    ? () => {
                        setRemoving({
                          kind: "representation",
                          id: representation.id,
                          name: representation.agencyName,
                        });
                      }
                    : undefined,
                }))}
              />
            </Section>
          )}
          {isAgencySide && (
            <Section title={t("detail.representedArtists")}>
              <LinkTable
                empty={t("detail.noRepresentedArtists")}
                columns={[t("detail.name"), t("detail.description")]}
                rows={party.representedArtists.map((artist) => ({
                  id: artist.artistId,
                  partyId: artist.artistId,
                  name: artist.name,
                  isActive: artist.isActive,
                  cells: [artist.description],
                }))}
              />
            </Section>
          )}
          {party.employers.length > 0 && (
            <Section title={t("detail.employers")}>
              <LinkTable
                empty=""
                columns={[t("detail.name"), t("detail.title")]}
                rows={party.employers.map((employer) => ({
                  id: employer.organizationId,
                  partyId: employer.organizationId,
                  name: employer.name,
                  isActive: employer.isActive,
                  cells: [employer.title],
                }))}
              />
            </Section>
          )}
        </div>
      )}
      <PartyFormDialog
        open={isEditing}
        onOpenChange={setIsEditing}
        partyId={party.id}
        onSaved={(saved) => {
          setIsEditing(false);
          show(saved);
          toast.success(t("parties.done.saved"));
        }}
      />
      <PartyLinkDialog
        link={contactLink}
        title={
          contactLink?.mode === "edit" ? t("link.editContactPerson") : t("link.addContactPerson")
        }
        pickerLabel={t("link.person")}
        textLabel={t("detail.title")}
        textMaxLength={100}
        candidates={{ kind: PartyKind.person }}
        onClose={() => {
          setContactLink(undefined);
        }}
        onSubmit={(otherId, title) =>
          contactLink?.mode === "edit" && contactLink.id !== undefined
            ? editContactPerson(party.id, contactLink.id, { title }, ifMatch(party.version))
            : addContactPerson(party.id, { personId: otherId ?? "", title }, ifMatch(party.version))
        }
        onSaved={(saved) => {
          setContactLink(undefined);
          show(saved);
          toast.success(t("link.saved"));
        }}
      />
      <PartyLinkDialog
        link={agencyLink}
        title={agencyLink?.mode === "edit" ? t("link.editAgency") : t("link.addAgency")}
        pickerLabel={t("link.agency")}
        textLabel={t("detail.description")}
        textMaxLength={200}
        candidates={{ role: PartyRole.agency }}
        onClose={() => {
          setAgencyLink(undefined);
        }}
        onSubmit={(otherId, description) =>
          agencyLink?.mode === "edit" && agencyLink.id !== undefined
            ? editRepresentation(party.id, agencyLink.id, { description }, ifMatch(party.version))
            : addRepresentation(
                party.id,
                { agencyId: otherId ?? "", description },
                ifMatch(party.version),
              )
        }
        onSaved={(saved) => {
          setAgencyLink(undefined);
          show(saved);
          toast.success(t("link.saved"));
        }}
      />
      <ConfirmDialog
        open={isConfirming}
        onOpenChange={setIsConfirming}
        title={t("parties.confirm.deactivateTitle", { name: party.name })}
        description={t("parties.confirm.deactivateDescription")}
        confirmLabel={t("parties.actions.deactivate")}
        isPending={status.isPending}
        onConfirm={() => {
          status.mutate();
        }}
      />
      <ConfirmDialog
        open={removing !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setRemoving(undefined);
          }
        }}
        title={t("link.removeTitle", { name: removing?.name })}
        description={t("link.removeDescription")}
        confirmLabel={t("link.remove")}
        isPending={removal.isPending}
        onConfirm={() => {
          if (removing !== undefined) {
            removal.mutate(removing);
          }
        }}
      />
    </div>
  );
}

type TabLinkProps = {
  to: "/parties/$partyId" | "/parties/$partyId/history";
  partyId: string;
  isCurrent: boolean;
  children: ReactNode;
};

function TabLink({ to, partyId, isCurrent, children }: TabLinkProps) {
  return (
    <Link
      to={to}
      params={{ partyId }}
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

function Section({
  title,
  action,
  children,
}: {
  title: string;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4">
      <div className="flex items-center justify-between gap-2">
        <h2 className="text-base font-semibold">{title}</h2>
        {action}
      </div>
      {children}
    </section>
  );
}

function AddButton({ label, onClick }: { label: string; onClick: () => void }) {
  return (
    <Button size="sm" variant="outline" onClick={onClick}>
      <Plus aria-hidden="true" />
      {label}
    </Button>
  );
}

type LinkRow = {
  id: string;
  partyId: string;
  name: string;
  isActive: boolean;
  cells: readonly (string | null)[];
  onEdit?: (() => void) | undefined;
  onRemove?: (() => void) | undefined;
};

function LinkTable({
  empty,
  columns,
  rows,
}: {
  empty: string;
  columns: readonly string[];
  rows: readonly LinkRow[];
}) {
  const { t } = useTranslation("parties");
  if (rows.length === 0) {
    return <p className="text-sm text-muted-foreground">{empty}</p>;
  }

  const hasActions = rows.some((row) => row.onEdit !== undefined || row.onRemove !== undefined);
  return (
    <Table>
      <TableHeader>
        <TableRow>
          {columns.map((column) => (
            <TableHead key={column}>{column}</TableHead>
          ))}
          {hasActions && (
            <TableHead>
              <span className="sr-only">{t("parties.columns.actions")}</span>
            </TableHead>
          )}
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((row) => (
          <TableRow key={row.id}>
            <TableCell>
              <Link
                to="/parties/$partyId"
                params={{ partyId: row.partyId }}
                className="font-medium hover:underline"
              >
                {row.name}
              </Link>
              {!row.isActive && (
                <span className="ml-2">
                  <StatusBadge tone="muted" label={t("parties.status.inactive")} />
                </span>
              )}
            </TableCell>
            {row.cells.map((cell, index) => (
              <TableCell key={columns[index + 1]} className="whitespace-normal">
                {cell ?? "—"}
              </TableCell>
            ))}
            {hasActions && (
              <TableCell className="text-right whitespace-nowrap">
                {row.onEdit !== undefined && (
                  <Button size="sm" variant="ghost" onClick={row.onEdit}>
                    {t("parties.actions.edit")}
                  </Button>
                )}
                {row.onRemove !== undefined && (
                  <Button size="sm" variant="ghost" onClick={row.onRemove}>
                    {t("link.remove")}
                  </Button>
                )}
              </TableCell>
            )}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
