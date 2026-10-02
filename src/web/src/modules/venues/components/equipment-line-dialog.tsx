import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import {
  getListEquipmentCategoriesQueryKey,
  getListEquipmentModelsQueryKey,
  listEquipmentCategories,
  listEquipmentModels,
} from "@/api/endpoints/catalog/catalog";
import { addVenueEquipment, editVenueEquipment } from "@/api/endpoints/venues/venues";
import type { VenueEquipmentItem, VenueEquipmentList } from "@/api/model";
import { EntityPicker, type PickerOption } from "@/components/common/entity-picker";
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
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ifMatch } from "@/lib/api-client";
import { errorMessage } from "@/lib/api-error-messages";
import { i18n } from "@/lib/i18n";

import { endForApi, lastDayOf } from "../venue-days";

type TargetKind = "model" | "category" | "description";

const pickerPageSize = 10;

export type EquipmentLineDialogProps = {
  venueId: string;
  version: number;
  /** The line being edited; a new line without it. */
  line: VenueEquipmentItem | "new" | undefined;
  onClose: () => void;
  onSaved: (equipment: VenueEquipmentList) => void;
};

// Adding or changing a venue equipment line (US-VEN-002): a model or a category picked by search, or
// equipment the catalog lacks in words (never counted, BR-VEN-001), a quantity and optional days.
export function EquipmentLineDialog({
  venueId,
  version,
  line,
  onClose,
  onSaved,
}: EquipmentLineDialogProps) {
  const { t } = useTranslation("venues");

  return (
    <Dialog
      open={line !== undefined}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {line === "new" ? t("lineForm.createTitle") : t("lineForm.editTitle")}
          </DialogTitle>
          <DialogDescription>{t("lineForm.help")}</DialogDescription>
        </DialogHeader>
        {line !== undefined && (
          <LineForm
            venueId={venueId}
            version={version}
            line={line === "new" ? undefined : line}
            onCancel={onClose}
            onSaved={onSaved}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

type LineFormProps = {
  venueId: string;
  version: number;
  line: VenueEquipmentItem | undefined;
  onCancel: () => void;
  onSaved: (equipment: VenueEquipmentList) => void;
};

function LineForm({ venueId, version, line, onCancel, onSaved }: LineFormProps) {
  const { t } = useTranslation("venues");
  const [kind, setKind] = useState<TargetKind>(
    (line?.categoryId ?? null) !== null
      ? "category"
      : (line?.description ?? null) !== null
        ? "description"
        : "model",
  );
  const [target, setTarget] = useState<PickerOption | null>(
    line?.isCounted === true
      ? { id: line.modelId ?? line.categoryId ?? "", label: line.name }
      : null,
  );
  const [description, setDescription] = useState(line?.description ?? "");
  const [quantity, setQuantity] = useState(line === undefined ? "1" : String(line.quantity));
  const [firstDay, setFirstDay] = useState(line?.validityStart ?? "");
  const [lastDay, setLastDay] = useState(lastDayOf(line?.validityEnd ?? null));
  const [problems, setProblems] = useState<Partial<Record<"target" | "quantity" | "days", string>>>(
    {},
  );
  const save = useMutation({
    mutationFn: () => {
      const body = {
        modelId: kind === "model" ? (target?.id ?? null) : null,
        categoryId: kind === "category" ? (target?.id ?? null) : null,
        description: kind === "description" ? description.trim() : null,
        quantity: Number(quantity),
        validityStart: firstDay === "" ? null : firstDay,
        validityEnd: endForApi(lastDay),
      };
      return line === undefined
        ? addVenueEquipment(venueId, body, ifMatch(version))
        : editVenueEquipment(venueId, line.id, body, ifMatch(version));
    },
    onSuccess: onSaved,
  });
  const kinds = (["model", "category", "description"] as const).map((value) => ({
    value,
    label: t(`lineForm.kinds.${value}`),
  }));

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) => {
        event.preventDefault();
        const found: typeof problems = {};
        if (kind === "description" ? description.trim() === "" : target === null) {
          found.target = i18n.t("validation:required");
        }

        if (!/^\d{1,4}$/u.test(quantity.trim()) || Number(quantity) < 1) {
          found.quantity = i18n.t("validation:range", { min: 1, max: 9999 });
        }

        if (firstDay !== "" && lastDay !== "" && lastDay < firstDay) {
          found.days = t("lineForm.daysOrder");
        }

        setProblems(found);
        if (Object.keys(found).length === 0) {
          save.mutate();
        }
      }}
    >
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="line-kind">{t("lineForm.kind")} *</FieldLabel>
          <Select
            items={kinds}
            value={kind}
            onValueChange={(value) => {
              setKind(value ?? "model");
              setTarget(null);
            }}
          >
            <SelectTrigger id="line-kind" className="w-full">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {kinds.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          {kind === "description" && (
            <FieldDescription>{t("lineForm.notCounted")}</FieldDescription>
          )}
        </Field>
        <Field data-invalid={problems.target !== undefined}>
          <FieldLabel htmlFor="line-target">{t(`lineForm.targets.${kind}`)} *</FieldLabel>
          {kind === "description" ? (
            <Input
              id="line-target"
              value={description}
              maxLength={200}
              autoComplete="off"
              onChange={(event) => {
                setDescription(event.target.value);
              }}
            />
          ) : kind === "model" ? (
            <EntityPicker
              id="line-target"
              value={target}
              onChange={setTarget}
              placeholder={t("lineForm.searchModel")}
              queryKey={(q) =>
                getListEquipmentModelsQueryKey({ q, status: "active", pageSize: pickerPageSize })
              }
              search={async (q, signal) =>
                (
                  await listEquipmentModels(
                    { q: q === "" ? undefined : q, status: "active", pageSize: pickerPageSize },
                    { signal },
                  )
                ).items.map((model) => ({
                  id: model.id,
                  label: `${model.brand} ${model.name}`,
                  description: model.categoryPath.join(" › "),
                }))
              }
            />
          ) : (
            <EntityPicker
              id="line-target"
              value={target}
              onChange={setTarget}
              placeholder={t("lineForm.searchCategory")}
              queryKey={(q) => [...getListEquipmentCategoriesQueryKey({ status: "active" }), q]}
              search={async (q, signal) => {
                const key = q.toLocaleLowerCase("tr");
                return (await listEquipmentCategories({ status: "active" }, { signal }))
                  .map((category) => ({ id: category.id, label: category.path.join(" › ") }))
                  .filter((option) => option.label.toLocaleLowerCase("tr").includes(key))
                  .slice(0, pickerPageSize);
              }}
            />
          )}
          <FieldError
            errors={problems.target === undefined ? [] : [{ message: problems.target }]}
          />
        </Field>
        <Field data-invalid={problems.quantity !== undefined}>
          <FieldLabel htmlFor="line-quantity">{t("lineForm.quantity")} *</FieldLabel>
          <Input
            id="line-quantity"
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
        <div className="grid gap-4 sm:grid-cols-2">
          <Field>
            <FieldLabel htmlFor="line-first-day">{t("lineForm.firstDay")}</FieldLabel>
            <Input
              id="line-first-day"
              type="date"
              value={firstDay}
              onChange={(event) => {
                setFirstDay(event.target.value);
              }}
            />
          </Field>
          <Field data-invalid={problems.days !== undefined}>
            <FieldLabel htmlFor="line-last-day">{t("lineForm.lastDay")}</FieldLabel>
            <Input
              id="line-last-day"
              type="date"
              value={lastDay}
              onChange={(event) => {
                setLastDay(event.target.value);
              }}
            />
            <FieldError errors={problems.days === undefined ? [] : [{ message: problems.days }]} />
          </Field>
        </div>
        <FieldDescription>{t("lineForm.daysHelp")}</FieldDescription>
      </FieldGroup>
      {save.isError && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("lineForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {line === undefined ? t("lineForm.create") : t("lineForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
