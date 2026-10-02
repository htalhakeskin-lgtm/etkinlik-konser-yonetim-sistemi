import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";

import { getListPartiesQueryKey, listParties } from "@/api/endpoints/parties/parties";
import {
  createVenue,
  editVenue,
  getGetVenueQueryKey,
  getVenue,
} from "@/api/endpoints/venues/venues";
import type { VenueDetails } from "@/api/model";
import { EntityPicker } from "@/components/common/entity-picker";
import { FormAlert } from "@/components/common/form-alert";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";

import { type VenueFormValues, venueFormValues, venueRequest, venueSchema } from "../venue-form";

const textFields = ["name", "city", "address"] as const;
const measureFields = ["stageWidthMeters", "stageDepthMeters", "stageHeightMeters"] as const;
const serverFields = [
  ...textFields,
  ...measureFields,
  "capacity",
  "loadingDock",
  "powerCapacityAmperes",
  "timeZone",
] as const;
const pickerPageSize = 10;

export type VenueFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** The venue being edited; a new venue without it. */
  venueId?: string | undefined;
  onSaved: (venue: VenueDetails, isNew: boolean) => void;
};

// Creating or editing a venue with its technical details (US-VEN-001); the operator is picked among active
// venue operators (BR-PTY-004, parties MD-03).
export function VenueFormDialog({ open, onOpenChange, venueId, onSaved }: VenueFormDialogProps) {
  const { t } = useTranslation("venues");
  const isEditing = venueId !== undefined;
  const venue = useQuery({
    queryKey: getGetVenueQueryKey(venueId ?? ""),
    queryFn: ({ signal }) => getVenue(venueId ?? "", { signal }),
    enabled: open && isEditing,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("venueForm.editTitle") : t("venueForm.createTitle")}
          </DialogTitle>
          <DialogDescription>{t("venueForm.required")}</DialogDescription>
        </DialogHeader>
        {isEditing && venue.data === undefined ? (
          venue.isError ? (
            <FormAlert>{errorMessage(venue.error)}</FormAlert>
          ) : (
            <SkeletonBlock className="h-80 w-full" />
          )
        ) : (
          <VenueForm
            key={venue.data?.version ?? "new"}
            venue={venue.data}
            onCancel={() => {
              onOpenChange(false);
            }}
            onSaved={onSaved}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

type VenueFormProps = {
  venue: VenueDetails | undefined;
  onCancel: () => void;
  onSaved: (venue: VenueDetails, isNew: boolean) => void;
};

function VenueForm({ venue, onCancel, onSaved }: VenueFormProps) {
  const { t } = useTranslation("venues");
  const { control, handleSubmit, setError } = useForm<VenueFormValues>({
    resolver: zodResolver(venueSchema),
    defaultValues: venueFormValues(venue),
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const save = useMutation({
    mutationFn: (values: VenueFormValues) =>
      venue === undefined
        ? createVenue(venueRequest(values))
        : editVenue(venue.id, venueRequest(values), ifMatch(venue.version)),
    onSuccess: (saved) => {
      onSaved(saved, venue === undefined);
    },
    onError: (error) => {
      if (!(error instanceof ApiError)) {
        return;
      }

      if (error.code === "BR-VEN-003") {
        setError("name", { message: errorMessage(error) }, { shouldFocus: true });
      }

      if (error.code === "BR-PTY-004") {
        setError("operator", { message: errorMessage(error) });
      }

      for (const field of fieldErrors(error)) {
        const name = serverFields.find((known) => known === field.path);
        if (name !== undefined) {
          setError(name, { message: field.message }, { shouldFocus: true });
        }
      }
    },
  });
  const isShownOnFields =
    save.error instanceof ApiError &&
    (save.error.code === "BR-VEN-003" ||
      save.error.code === "BR-PTY-004" ||
      save.error.status === 400);

  return (
    <form
      noValidate
      className="flex max-h-[70vh] flex-col gap-6 overflow-y-auto pr-1"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          save.mutate(values);
        })(event)
      }
    >
      <FieldGroup>
        {textFields.map((name) => (
          <Controller
            key={name}
            control={control}
            name={name}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`venue-${name}`}>{t(`venueForm.${name}`)} *</FieldLabel>
                <Input
                  {...field}
                  id={`venue-${name}`}
                  autoComplete="off"
                  aria-invalid={fieldState.invalid}
                />
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
        ))}
        <Controller
          control={control}
          name="capacity"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="venue-capacity">{t("venueForm.capacity")} *</FieldLabel>
              <Input
                {...field}
                id="venue-capacity"
                inputMode="numeric"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="operator"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="venue-operator">{t("venueForm.operator")}</FieldLabel>
              <EntityPicker
                id="venue-operator"
                value={field.value}
                invalid={fieldState.invalid}
                onBlur={field.onBlur}
                onChange={field.onChange}
                placeholder={t("venueForm.operatorHint")}
                queryKey={(q) =>
                  getListPartiesQueryKey({
                    q,
                    role: "venueOperator",
                    status: "active",
                    pageSize: pickerPageSize,
                  })
                }
                search={async (q, signal) =>
                  (
                    await listParties(
                      {
                        q: q === "" ? undefined : q,
                        role: "venueOperator",
                        status: "active",
                        pageSize: pickerPageSize,
                      },
                      { signal },
                    )
                  ).items.map((party) => ({ id: party.id, label: party.name }))
                }
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
      </FieldGroup>
      <FieldSet>
        <FieldLegend variant="label">{t("venueForm.technical")}</FieldLegend>
        <FieldDescription>{t("venueForm.technicalHelp")}</FieldDescription>
        <FieldGroup className="sm:grid sm:grid-cols-3">
          {measureFields.map((name) => (
            <Controller
              key={name}
              control={control}
              name={name}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={`venue-${name}`}>{t(`venueForm.${name}`)}</FieldLabel>
                  <Input
                    {...field}
                    id={`venue-${name}`}
                    inputMode="decimal"
                    autoComplete="off"
                    aria-invalid={fieldState.invalid}
                  />
                  <FieldError errors={[fieldState.error]} />
                </Field>
              )}
            />
          ))}
        </FieldGroup>
        <FieldGroup className="sm:grid sm:grid-cols-3">
          <Controller
            control={control}
            name="powerCapacityAmperes"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor="venue-power">{t("venueForm.powerCapacityAmperes")}</FieldLabel>
                <Input
                  {...field}
                  id="venue-power"
                  inputMode="decimal"
                  autoComplete="off"
                  aria-invalid={fieldState.invalid}
                />
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
          <Controller
            control={control}
            name="curfew"
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="venue-curfew">{t("venueForm.curfew")}</FieldLabel>
                <Input {...field} id="venue-curfew" type="time" />
              </Field>
            )}
          />
          <Controller
            control={control}
            name="timeZone"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor="venue-timeZone">{t("venueForm.timeZone")} *</FieldLabel>
                <Input
                  {...field}
                  id="venue-timeZone"
                  autoComplete="off"
                  aria-invalid={fieldState.invalid}
                />
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
        </FieldGroup>
        <Controller
          control={control}
          name="loadingDock"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="venue-loadingDock">{t("venueForm.loadingDock")}</FieldLabel>
              <Input
                {...field}
                id="venue-loadingDock"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
      </FieldSet>
      {save.isError && !isShownOnFields && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("venueForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {venue === undefined ? t("venueForm.create") : t("venueForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
