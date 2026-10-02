import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import {
  addVenueEquipmentUnavailability,
  editVenueEquipmentUnavailability,
} from "@/api/endpoints/venues/venues";
import type { UnavailabilityItem, VenueEquipmentItem, VenueEquipmentList } from "@/api/model";
import { FormAlert } from "@/components/common/form-alert";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ifMatch } from "@/lib/api-client";
import { errorMessage } from "@/lib/api-error-messages";
import { i18n } from "@/lib/i18n";

import { lastDayOf, nextDay } from "../venue-days";

/** The line a period belongs to, and the period being edited or a new one. */
export type PeriodEdit = { line: VenueEquipmentItem; period: UnavailabilityItem | "new" };

export type UnavailabilityDialogProps = {
  venueId: string;
  version: number;
  edit: PeriodEdit | undefined;
  onClose: () => void;
  onSaved: (equipment: VenueEquipmentList) => void;
};

// Days when part of a line cannot be used (US-VEN-002 criterion 4, BR-VEN-002): the days with the last one
// included, how many and why.
export function UnavailabilityDialog({
  venueId,
  version,
  edit,
  onClose,
  onSaved,
}: UnavailabilityDialogProps) {
  const { t } = useTranslation("venues");

  return (
    <Dialog
      open={edit !== undefined}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {edit?.period === "new" ? t("periodForm.createTitle") : t("periodForm.editTitle")}
          </DialogTitle>
          <DialogDescription>{edit?.line.name}</DialogDescription>
        </DialogHeader>
        {edit !== undefined && (
          <PeriodForm
            venueId={venueId}
            version={version}
            edit={edit}
            onCancel={onClose}
            onSaved={onSaved}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

function PeriodForm({
  venueId,
  version,
  edit,
  onCancel,
  onSaved,
}: {
  venueId: string;
  version: number;
  edit: PeriodEdit;
  onCancel: () => void;
  onSaved: (equipment: VenueEquipmentList) => void;
}) {
  const { t } = useTranslation("venues");
  const current = edit.period === "new" ? undefined : edit.period;
  const [firstDay, setFirstDay] = useState(current?.periodStart ?? "");
  const [lastDay, setLastDay] = useState(current === undefined ? "" : lastDayOf(current.periodEnd));
  const [quantity, setQuantity] = useState(current === undefined ? "1" : String(current.quantity));
  const [reason, setReason] = useState(current?.reason ?? "");
  const [problems, setProblems] = useState<Partial<Record<"days" | "quantity" | "reason", string>>>(
    {},
  );
  const save = useMutation({
    mutationFn: () => {
      const body = {
        periodStart: firstDay,
        periodEnd: nextDay(lastDay),
        quantity: Number(quantity),
        reason: reason.trim(),
      };
      return current === undefined
        ? addVenueEquipmentUnavailability(venueId, edit.line.id, body, ifMatch(version))
        : editVenueEquipmentUnavailability(
            venueId,
            edit.line.id,
            current.id,
            body,
            ifMatch(version),
          );
    },
    onSuccess: onSaved,
  });

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) => {
        event.preventDefault();
        const found: typeof problems = {};
        if (firstDay === "" || lastDay === "") {
          found.days = i18n.t("validation:required");
        } else if (lastDay < firstDay) {
          found.days = t("lineForm.daysOrder");
        }

        if (!/^\d{1,4}$/u.test(quantity.trim()) || Number(quantity) < 1) {
          found.quantity = i18n.t("validation:range", { min: 1, max: 9999 });
        }

        if (reason.trim() === "") {
          found.reason = i18n.t("validation:required");
        }

        setProblems(found);
        if (Object.keys(found).length === 0) {
          save.mutate();
        }
      }}
    >
      <FieldGroup>
        <div className="grid gap-4 sm:grid-cols-2">
          <Field data-invalid={problems.days !== undefined}>
            <FieldLabel htmlFor="period-first-day">{t("periodForm.firstDay")} *</FieldLabel>
            <Input
              id="period-first-day"
              type="date"
              value={firstDay}
              onChange={(event) => {
                setFirstDay(event.target.value);
              }}
            />
          </Field>
          <Field data-invalid={problems.days !== undefined}>
            <FieldLabel htmlFor="period-last-day">{t("periodForm.lastDay")} *</FieldLabel>
            <Input
              id="period-last-day"
              type="date"
              value={lastDay}
              onChange={(event) => {
                setLastDay(event.target.value);
              }}
            />
          </Field>
        </div>
        <FieldError errors={problems.days === undefined ? [] : [{ message: problems.days }]} />
        <Field data-invalid={problems.quantity !== undefined}>
          <FieldLabel htmlFor="period-quantity">{t("periodForm.quantity")} *</FieldLabel>
          <Input
            id="period-quantity"
            inputMode="numeric"
            value={quantity}
            onChange={(event) => {
              setQuantity(event.target.value);
            }}
          />
          <FieldError
            errors={problems.quantity === undefined ? [] : [{ message: problems.quantity }]}
          />
        </Field>
        <Field data-invalid={problems.reason !== undefined}>
          <FieldLabel htmlFor="period-reason">{t("periodForm.reason")} *</FieldLabel>
          <Input
            id="period-reason"
            value={reason}
            maxLength={500}
            autoComplete="off"
            onChange={(event) => {
              setReason(event.target.value);
            }}
          />
          <FieldError
            errors={problems.reason === undefined ? [] : [{ message: problems.reason }]}
          />
        </Field>
      </FieldGroup>
      {save.isError && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("lineForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {current === undefined ? t("lineForm.create") : t("lineForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
