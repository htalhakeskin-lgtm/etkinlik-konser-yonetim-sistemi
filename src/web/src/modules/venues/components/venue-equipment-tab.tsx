import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, Trash2 } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  getGetVenueQueryKey,
  getListUsableVenueEquipmentQueryKey,
  getListVenueEquipmentQueryKey,
  listUsableVenueEquipment,
  listVenueEquipment,
  removeVenueEquipment,
  removeVenueEquipmentUnavailability,
} from "@/api/endpoints/venues/venues";
import {
  EquipmentStatusFilter,
  type UnavailabilityItem,
  type VenueEquipmentItem,
} from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { ErrorState } from "@/components/common/error-state";
import { FormAlert } from "@/components/common/form-alert";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
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
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { venuesPermissions } from "../permissions";
import { formatDays, nextDay } from "../venue-days";
import { EquipmentLineDialog } from "./equipment-line-dialog";
import { type PeriodEdit, UnavailabilityDialog } from "./unavailability-dialog";

type Removal =
  | { kind: "line"; line: VenueEquipmentItem }
  | { kind: "period"; line: VenueEquipmentItem; period: UnavailabilityItem };

const statuses = Object.values(EquipmentStatusFilter);

// The venue's own equipment (US-VEN-002): the lines with their days and the days when part of them cannot
// be used, edited one change at a time on the venue's version, and how many can be used on given days
// (BR-VEN-001).
export function VenueEquipmentTab({ venueId }: { venueId: string }) {
  const { t } = useTranslation("venues");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, venuesPermissions.editEquipment);
  const [status, setStatus] = useState<EquipmentStatusFilter>(EquipmentStatusFilter.current);
  const [editedLine, setEditedLine] = useState<VenueEquipmentItem | "new" | undefined>();
  const [editedPeriod, setEditedPeriod] = useState<PeriodEdit | undefined>();
  const [removal, setRemoval] = useState<Removal | undefined>();
  const equipment = useQuery({
    queryKey: getListVenueEquipmentQueryKey(venueId, { status }),
    queryFn: ({ signal }) => listVenueEquipment(venueId, { status }, { signal }),
  });
  const version = equipment.data?.version ?? 0;

  // A change moves the venue's version, so the venue itself is read again with the equipment.
  const refresh = () => {
    void queryClient.invalidateQueries({ queryKey: getListVenueEquipmentQueryKey(venueId) });
    void queryClient.invalidateQueries({ queryKey: getListUsableVenueEquipmentQueryKey(venueId) });
    void queryClient.invalidateQueries({ queryKey: getGetVenueQueryKey(venueId) });
  };
  const remove = useMutation({
    mutationFn: (target: Removal) =>
      target.kind === "line"
        ? removeVenueEquipment(venueId, target.line.id, ifMatch(version))
        : removeVenueEquipmentUnavailability(
            venueId,
            target.line.id,
            target.period.id,
            ifMatch(version),
          ),
    onSuccess: (_, target) => {
      setRemoval(undefined);
      refresh();
      toast.success(
        t(target.kind === "line" ? "equipment.done.lineRemoved" : "equipment.done.periodRemoved"),
      );
    },
    onError: (error) => {
      setRemoval(undefined);
      toast.error(errorMessage(error));
    },
  });
  const statusOptions = statuses.map((value) => ({ value, label: t(`equipment.status.${value}`) }));

  return (
    <div className="grid gap-4 xl:grid-cols-[2fr_1fr]">
      <Section
        title={t("equipment.title")}
        actions={
          <>
            <Select
              items={statusOptions}
              value={status}
              onValueChange={(value) => {
                setStatus(value ?? EquipmentStatusFilter.current);
              }}
            >
              <SelectTrigger aria-label={t("equipment.statusLabel")} className="w-40">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {statusOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {canEdit && (
              <Button
                disabled={equipment.data === undefined}
                onClick={() => {
                  setEditedLine("new");
                }}
              >
                <Plus aria-hidden="true" />
                {t("equipment.addLine")}
              </Button>
            )}
          </>
        }
      >
        {equipment.isError ? (
          <ErrorState
            error={equipment.error}
            onRetry={() => {
              void equipment.refetch();
            }}
          />
        ) : equipment.data === undefined ? (
          <SkeletonBlock className="h-40 w-full" />
        ) : equipment.data.lines.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t("equipment.empty")}</p>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("equipment.columns.name")}</TableHead>
                <TableHead className="w-20">{t("equipment.columns.quantity")}</TableHead>
                <TableHead>{t("equipment.columns.validity")}</TableHead>
                <TableHead>{t("equipment.columns.unavailable")}</TableHead>
                {canEdit && (
                  <TableHead>
                    <span className="sr-only">{t("equipment.columns.actions")}</span>
                  </TableHead>
                )}
              </TableRow>
            </TableHeader>
            <TableBody>
              {equipment.data.lines.map((line) => (
                <TableRow key={line.id}>
                  <TableCell className="min-w-56 whitespace-normal">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium">{line.name}</span>
                      {!line.isCounted && (
                        <StatusBadge tone="neutral" label={t("equipment.notCounted")} />
                      )}
                      {line.isCounted && !line.isTargetActive && (
                        <StatusBadge tone="muted" label={t("equipment.targetInactive")} />
                      )}
                    </div>
                    {line.categoryPath.length > 0 && (
                      <div className="text-xs text-muted-foreground">
                        {line.categoryPath.join(" › ")}
                      </div>
                    )}
                  </TableCell>
                  <TableCell className="tabular-nums">{line.quantity}</TableCell>
                  <TableCell className="whitespace-nowrap">
                    {line.validityStart === null && line.validityEnd === null
                      ? t("equipment.always")
                      : formatDays(line.validityStart, line.validityEnd)}
                  </TableCell>
                  <TableCell className="whitespace-normal">
                    <ul className="flex flex-col gap-1">
                      {line.unavailabilities.map((period) => (
                        <li key={period.id} className="flex items-center gap-1 text-sm">
                          <span>
                            {t("equipment.period", {
                              days: formatDays(period.periodStart, period.periodEnd),
                              count: period.quantity,
                              reason: period.reason,
                            })}
                          </span>
                          {canEdit && (
                            <>
                              <Button
                                variant="ghost"
                                size="icon-sm"
                                aria-label={t("equipment.editPeriod", { name: line.name })}
                                onClick={() => {
                                  setEditedPeriod({ line, period });
                                }}
                              >
                                <Pencil aria-hidden="true" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon-sm"
                                aria-label={t("equipment.removePeriod", { name: line.name })}
                                onClick={() => {
                                  setRemoval({ kind: "period", line, period });
                                }}
                              >
                                <Trash2 aria-hidden="true" />
                              </Button>
                            </>
                          )}
                        </li>
                      ))}
                    </ul>
                    {line.unavailabilities.length === 0 && !canEdit && "—"}
                    {canEdit && (
                      <Button
                        variant="link"
                        size="sm"
                        className="px-0"
                        onClick={() => {
                          setEditedPeriod({ line, period: "new" });
                        }}
                      >
                        {t("equipment.addPeriod")}
                      </Button>
                    )}
                  </TableCell>
                  {canEdit && (
                    <TableCell className="text-right whitespace-nowrap">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        aria-label={t("equipment.editLine", { name: line.name })}
                        onClick={() => {
                          setEditedLine(line);
                        }}
                      >
                        <Pencil aria-hidden="true" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        aria-label={t("equipment.removeLine", { name: line.name })}
                        onClick={() => {
                          setRemoval({ kind: "line", line });
                        }}
                      >
                        <Trash2 aria-hidden="true" />
                      </Button>
                    </TableCell>
                  )}
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Section>
      <UsableEquipment venueId={venueId} />
      <EquipmentLineDialog
        venueId={venueId}
        version={version}
        line={editedLine}
        onClose={() => {
          setEditedLine(undefined);
        }}
        onSaved={() => {
          setEditedLine(undefined);
          refresh();
          toast.success(t("equipment.done.lineSaved"));
        }}
      />
      <UnavailabilityDialog
        venueId={venueId}
        version={version}
        edit={editedPeriod}
        onClose={() => {
          setEditedPeriod(undefined);
        }}
        onSaved={() => {
          setEditedPeriod(undefined);
          refresh();
          toast.success(t("equipment.done.periodSaved"));
        }}
      />
      <ConfirmDialog
        open={removal !== undefined}
        onOpenChange={(isOpen) => {
          if (!isOpen) {
            setRemoval(undefined);
          }
        }}
        title={
          removal?.kind === "period"
            ? t("equipment.confirm.periodTitle", { name: removal.line.name })
            : t("equipment.confirm.lineTitle", { name: removal?.line.name ?? "" })
        }
        description={
          removal?.kind === "period"
            ? t("equipment.confirm.periodDescription")
            : t("equipment.confirm.lineDescription")
        }
        confirmLabel={t("equipment.confirm.remove")}
        isPending={remove.isPending}
        onConfirm={() => {
          if (removal !== undefined) {
            remove.mutate(removal);
          }
        }}
      />
    </div>
  );
}

// How many of each line can be used on the given days (BR-VEN-001), the last day included.
function UsableEquipment({ venueId }: { venueId: string }) {
  const { t } = useTranslation("venues");
  const [firstDay, setFirstDay] = useState("");
  const [lastDay, setLastDay] = useState("");
  const [days, setDays] = useState<{ from: string; to: string } | undefined>();
  const [isInvalid, setIsInvalid] = useState(false);
  const usable = useQuery({
    queryKey: getListUsableVenueEquipmentQueryKey(venueId, days),
    queryFn: ({ signal }) =>
      listUsableVenueEquipment(venueId, days ?? { from: "", to: "" }, { signal }),
    enabled: days !== undefined,
  });

  return (
    <Section title={t("equipment.usable.title")}>
      <p className="text-sm text-muted-foreground">{t("equipment.usable.help")}</p>
      <form
        noValidate
        className="flex flex-col gap-3"
        onSubmit={(event) => {
          event.preventDefault();
          const isValid = firstDay !== "" && lastDay !== "" && lastDay >= firstDay;
          setIsInvalid(!isValid);
          if (isValid) {
            setDays({ from: firstDay, to: nextDay(lastDay) });
          }
        }}
      >
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-1 2xl:grid-cols-2">
          <Field>
            <FieldLabel htmlFor="usable-first-day">{t("equipment.usable.firstDay")}</FieldLabel>
            <Input
              id="usable-first-day"
              type="date"
              value={firstDay}
              onChange={(event) => {
                setFirstDay(event.target.value);
              }}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="usable-last-day">{t("equipment.usable.lastDay")}</FieldLabel>
            <Input
              id="usable-last-day"
              type="date"
              value={lastDay}
              onChange={(event) => {
                setLastDay(event.target.value);
              }}
            />
          </Field>
        </div>
        {isInvalid && <FormAlert>{t("equipment.usable.invalid")}</FormAlert>}
        <Button type="submit" variant="outline" isLoading={usable.isFetching}>
          {t("equipment.usable.show")}
        </Button>
      </form>
      {usable.isError && <FormAlert>{errorMessage(usable.error)}</FormAlert>}
      {usable.data !== undefined &&
        (usable.data.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t("equipment.usable.empty")}</p>
        ) : (
          <ul className="flex flex-col gap-1 text-sm">
            {usable.data.map((item) => (
              <li key={item.id} className="flex justify-between gap-2">
                <span>{item.name}</span>
                <span className="tabular-nums">
                  {item.isCounted
                    ? t("equipment.usable.quantity", {
                        usable: item.usableQuantity,
                        total: item.quantity,
                      })
                    : t("equipment.notCounted")}
                </span>
              </li>
            ))}
          </ul>
        ))}
    </Section>
  );
}

function Section({
  title,
  actions,
  children,
}: {
  title: string;
  actions?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-base font-semibold">{title}</h2>
        {actions !== undefined && <div className="flex flex-wrap gap-2">{actions}</div>}
      </div>
      {children}
    </section>
  );
}
