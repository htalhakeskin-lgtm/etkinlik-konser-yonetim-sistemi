import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { Controller, useFieldArray, useForm, useWatch } from "react-hook-form";
import { useTranslation } from "react-i18next";

import {
  createParty,
  editParty,
  getGetPartyQueryKey,
  getParty,
} from "@/api/endpoints/parties/parties";
import { ContactPointKind, type PartyDetails, PartyKind, PartyRole } from "@/api/model";
import { FormAlert } from "@/components/common/form-alert";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
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
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";

import {
  type ContactPointValues,
  type PartyFormValues,
  partyFormValues,
  partyRequest,
  partySchema,
  withPrimary,
} from "../party-form";

// React Hook Form names an array item's field by its index.
function pointField<K extends keyof ContactPointValues>(index: number, key: K) {
  return `contactPoints.${index.toString()}.${key}` as `contactPoints.${number}.${K}`;
}

const nameFields = ["name", "firstName", "lastName", "legalName"] as const;

export type PartyFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** The party being edited; a new party without it. */
  partyId?: string | undefined;
  /** The roles a new party starts with, e.g. the artist role on the artists screen. */
  initialRoles?: readonly PartyRole[];
  onSaved: (party: PartyDetails, isNew: boolean) => void;
};

// Creating or editing a party with its contact points (US-PTY-001, US-PTY-002); the kind is chosen once
// (BR-PTY-001).
export function PartyFormDialog({
  open,
  onOpenChange,
  partyId,
  initialRoles = [],
  onSaved,
}: PartyFormDialogProps) {
  const { t } = useTranslation("parties");
  const isEditing = partyId !== undefined;
  const party = useQuery({
    queryKey: getGetPartyQueryKey(partyId ?? ""),
    queryFn: ({ signal }) => getParty(partyId ?? "", { signal }),
    enabled: open && isEditing,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("partyForm.editTitle") : t("partyForm.createTitle")}
          </DialogTitle>
          <DialogDescription>{t("partyForm.required")}</DialogDescription>
        </DialogHeader>
        {isEditing && party.data === undefined ? (
          party.isError ? (
            <FormAlert>{errorMessage(party.error)}</FormAlert>
          ) : (
            <SkeletonBlock className="h-60 w-full" />
          )
        ) : (
          <PartyForm
            key={party.data?.version ?? "new"}
            party={party.data}
            initialRoles={initialRoles}
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

type PartyFormProps = {
  party: PartyDetails | undefined;
  initialRoles: readonly PartyRole[];
  onCancel: () => void;
  onSaved: (party: PartyDetails, isNew: boolean) => void;
};

function PartyForm({ party, initialRoles, onCancel, onSaved }: PartyFormProps) {
  const { t } = useTranslation("parties");
  const defaults = partyFormValues(party);
  const { control, handleSubmit, setError, getValues, setValue } = useForm<PartyFormValues>({
    resolver: zodResolver(partySchema),
    defaultValues:
      party === undefined && initialRoles.length > 0
        ? { ...defaults, roles: [...initialRoles] }
        : defaults,
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const contactPoints = useFieldArray({ control, name: "contactPoints" });
  const kind = useWatch({ control, name: "kind" });
  const isPerson = kind === PartyKind.person;
  const save = useMutation({
    mutationFn: (values: PartyFormValues) =>
      party === undefined
        ? createParty(partyRequest(values))
        : editParty(party.id, partyRequest(values), ifMatch(party.version)),
    onSuccess: (saved) => {
      onSaved(saved, party === undefined);
    },
    onError: (error) => {
      if (!(error instanceof ApiError)) {
        return;
      }

      if (error.code === "BR-PTY-001") {
        setError("roles", { message: errorMessage(error) });
      }

      for (const field of fieldErrors(error)) {
        const name = nameFields.find((known) => known === field.path);
        if (name !== undefined) {
          setError(name, { message: field.message }, { shouldFocus: true });
        }
      }
    },
  });
  const isShownOnFields =
    save.error instanceof ApiError &&
    (save.error.code === "BR-PTY-001" || save.error.status === 400);
  const kinds = Object.values(PartyKind).map((value) => ({ value, label: t(`kinds.${value}`) }));
  const contactKinds = Object.values(ContactPointKind).map((value) => ({
    value,
    label: t(`contactKinds.${value}`),
  }));

  // A person's shown name starts as the first and last name; a stage name can replace it (parties PT-02).
  const suggestName = () => {
    const { name, firstName, lastName } = getValues();
    if (name.trim() === "" && firstName.trim() !== "" && lastName.trim() !== "") {
      setValue("name", `${firstName.trim()} ${lastName.trim()}`, { shouldValidate: true });
    }
  };

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
        {party === undefined && (
          <Controller
            control={control}
            name="kind"
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="party-kind">{t("partyForm.kind")} *</FieldLabel>
                <Select
                  items={kinds}
                  value={field.value}
                  onValueChange={(value) => {
                    field.onChange(value);
                  }}
                >
                  <SelectTrigger id="party-kind">
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
              </Field>
            )}
          />
        )}
        {isPerson && (
          <div className="grid gap-4 sm:grid-cols-2">
            {(["firstName", "lastName"] as const).map((name) => (
              <Controller
                key={name}
                control={control}
                name={name}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <FieldLabel htmlFor={`party-${name}`}>{t(`partyForm.${name}`)} *</FieldLabel>
                    <Input
                      {...field}
                      id={`party-${name}`}
                      autoComplete="off"
                      aria-invalid={fieldState.invalid}
                      onBlur={() => {
                        field.onBlur();
                        suggestName();
                      }}
                    />
                    <FieldError errors={[fieldState.error]} />
                  </Field>
                )}
              />
            ))}
          </div>
        )}
        <Controller
          control={control}
          name="name"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="party-name">
                {t(isPerson ? "partyForm.shownName" : "partyForm.name")} *
              </FieldLabel>
              <Input
                {...field}
                id="party-name"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        {!isPerson && (
          <Controller
            control={control}
            name="legalName"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor="party-legalName">{t("partyForm.legalName")}</FieldLabel>
                <Input
                  {...field}
                  id="party-legalName"
                  autoComplete="off"
                  aria-invalid={fieldState.invalid}
                />
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
        )}
        <Controller
          control={control}
          name="roles"
          render={({ field, fieldState }) => (
            <FieldSet data-invalid={fieldState.invalid}>
              <FieldLegend variant="label">{t("partyForm.roles")} *</FieldLegend>
              <div className="grid gap-2 sm:grid-cols-2">
                {Object.values(PartyRole).map((role) => (
                  <Field key={role} orientation="horizontal">
                    <Checkbox
                      id={`party-role-${role}`}
                      checked={field.value.includes(role)}
                      onCheckedChange={(checked) => {
                        field.onChange(
                          checked
                            ? [...field.value, role]
                            : field.value.filter((held) => held !== role),
                        );
                      }}
                      onBlur={field.onBlur}
                    />
                    <FieldLabel htmlFor={`party-role-${role}`} className="font-normal">
                      {t(`roles.${role}`)}
                    </FieldLabel>
                  </Field>
                ))}
              </div>
              <FieldError errors={[fieldState.error]} />
            </FieldSet>
          )}
        />
        <FieldSet>
          <FieldLegend variant="label">{t("partyForm.contactPoints")}</FieldLegend>
          {contactPoints.fields.length === 0 && (
            <p className="text-sm text-muted-foreground">{t("partyForm.noContactPoints")}</p>
          )}
          {contactPoints.fields.map((point, index) => (
            <div
              key={point.id}
              className="grid items-start gap-2 rounded-md border p-3 sm:grid-cols-[9rem_1fr_9rem_auto]"
            >
              <Controller
                control={control}
                name={pointField(index, "kind")}
                render={({ field }) => (
                  <Select
                    items={contactKinds}
                    value={field.value}
                    onValueChange={(value) => {
                      field.onChange(value);
                    }}
                  >
                    <SelectTrigger aria-label={t("partyForm.contactKind", { number: index + 1 })}>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {contactKinds.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                          {option.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
              <Controller
                control={control}
                name={pointField(index, "value")}
                render={({ field, fieldState }) => (
                  <Field data-invalid={fieldState.invalid}>
                    <Input
                      {...field}
                      aria-label={t("partyForm.contactValue", { number: index + 1 })}
                      autoComplete="off"
                      aria-invalid={fieldState.invalid}
                    />
                    <FieldError errors={[fieldState.error]} />
                  </Field>
                )}
              />
              <Controller
                control={control}
                name={pointField(index, "label")}
                render={({ field }) => (
                  <Input
                    {...field}
                    aria-label={t("partyForm.contactLabel", { number: index + 1 })}
                    placeholder={t("partyForm.contactLabelHint")}
                    autoComplete="off"
                  />
                )}
              />
              <div className="flex items-center gap-2">
                <Controller
                  control={control}
                  name={pointField(index, "isPrimary")}
                  render={({ field }) => (
                    <Field orientation="horizontal">
                      <Checkbox
                        id={`party-contact-primary-${point.id}`}
                        checked={field.value}
                        onCheckedChange={(checked) => {
                          if (checked) {
                            setValue(
                              "contactPoints",
                              withPrimary(getValues("contactPoints"), index),
                            );
                          } else {
                            field.onChange(false);
                          }
                        }}
                      />
                      <FieldLabel
                        htmlFor={`party-contact-primary-${point.id}`}
                        className="font-normal"
                      >
                        {t("partyForm.primary")}
                      </FieldLabel>
                    </Field>
                  )}
                />
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("partyForm.removeContact", { number: index + 1 })}
                  onClick={() => {
                    contactPoints.remove(index);
                  }}
                >
                  <Trash2 aria-hidden="true" />
                </Button>
              </div>
            </div>
          ))}
          {contactPoints.fields.length < 20 && (
            <Button
              type="button"
              variant="outline"
              className="self-start"
              onClick={() => {
                contactPoints.append({
                  kind: ContactPointKind.phone,
                  value: "",
                  label: "",
                  isPrimary: false,
                });
              }}
            >
              <Plus aria-hidden="true" />
              {t("partyForm.addContact")}
            </Button>
          )}
        </FieldSet>
      </FieldGroup>
      {save.isError && !isShownOnFields && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("partyForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {party === undefined ? t("partyForm.create") : t("partyForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
