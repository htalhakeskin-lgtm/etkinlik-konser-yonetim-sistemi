import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { getListPartiesQueryKey, listParties } from "@/api/endpoints/parties/parties";
import type { ListPartiesParams, PartyDetails } from "@/api/model";
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
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { errorMessage } from "@/lib/api-error-messages";
import { i18n } from "@/lib/i18n";

/** A tie being added (the other party is picked) or edited (only the text changes). */
export type PartyLink = { mode: "add" } | { mode: "edit"; otherName: string; text: string | null };

export type PartyLinkDialogProps = {
  link: PartyLink | undefined;
  title: string;
  pickerLabel: string;
  textLabel: string;
  textMaxLength: number;
  /** Which parties the picker offers, e.g. active people or active agencies. */
  candidates: Omit<ListPartiesParams, "q" | "status" | "page" | "pageSize">;
  onClose: () => void;
  onSubmit: (otherId: string | null, text: string | null) => Promise<PartyDetails>;
  onSaved: (party: PartyDetails) => void;
};

const pickerPageSize = 10;

// Ties another party to this one (parties §10): a contact person to an organization, an agency to an
// artist. Only active parties with the needed role or kind are offered (BR-SYS-001, BR-PTY-003, 004).
export function PartyLinkDialog({
  link,
  title,
  pickerLabel,
  textLabel,
  textMaxLength,
  candidates,
  onClose,
  onSubmit,
  onSaved,
}: PartyLinkDialogProps) {
  return (
    <Dialog
      open={link !== undefined}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{i18n.t("parties:partyForm.required")}</DialogDescription>
        </DialogHeader>
        {link !== undefined && (
          <PartyLinkForm
            link={link}
            pickerLabel={pickerLabel}
            textLabel={textLabel}
            textMaxLength={textMaxLength}
            candidates={candidates}
            onCancel={onClose}
            onSubmit={onSubmit}
            onSaved={onSaved}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

type PartyLinkFormProps = Omit<PartyLinkDialogProps, "link" | "title" | "onClose"> & {
  link: PartyLink;
  onCancel: () => void;
};

function PartyLinkForm({
  link,
  pickerLabel,
  textLabel,
  textMaxLength,
  candidates,
  onCancel,
  onSubmit,
  onSaved,
}: PartyLinkFormProps) {
  const { t } = useTranslation("parties");
  const [other, setOther] = useState<PickerOption | null>(null);
  const [text, setText] = useState(link.mode === "edit" ? (link.text ?? "") : "");
  const [isOtherMissing, setIsOtherMissing] = useState(false);
  const save = useMutation({
    mutationFn: () => onSubmit(other?.id ?? null, text.trim() === "" ? null : text.trim()),
    onSuccess: onSaved,
  });
  const isTooLong = text.trim().length > textMaxLength;

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) => {
        event.preventDefault();
        if (link.mode === "add" && other === null) {
          setIsOtherMissing(true);
          return;
        }

        if (!isTooLong) {
          save.mutate();
        }
      }}
    >
      <FieldGroup>
        {link.mode === "add" ? (
          <Field data-invalid={isOtherMissing}>
            <FieldLabel htmlFor="party-link-other">{pickerLabel} *</FieldLabel>
            <EntityPicker
              id="party-link-other"
              value={other}
              invalid={isOtherMissing}
              onChange={(next) => {
                setOther(next);
                setIsOtherMissing(false);
              }}
              queryKey={(q) => getListPartiesQueryKey(pickerParams(candidates, q))}
              search={async (q, signal) =>
                (await listParties(pickerParams(candidates, q), { signal })).items.map((party) => ({
                  id: party.id,
                  label: party.name,
                  description: party.primaryPhone ?? party.primaryEmail ?? undefined,
                }))
              }
              placeholder={t("link.searchHint")}
            />
            <FieldError
              errors={isOtherMissing ? [{ message: i18n.t("validation:required") }] : []}
            />
          </Field>
        ) : (
          <Field>
            <FieldLabel>{pickerLabel}</FieldLabel>
            <p className="text-sm font-medium">{link.otherName}</p>
          </Field>
        )}
        <Field data-invalid={isTooLong}>
          <FieldLabel htmlFor="party-link-text">{textLabel}</FieldLabel>
          <Input
            id="party-link-text"
            value={text}
            autoComplete="off"
            aria-invalid={isTooLong}
            onChange={(event) => {
              setText(event.target.value);
            }}
          />
          <FieldError
            errors={
              isTooLong ? [{ message: i18n.t("validation:maxLength", { max: textMaxLength }) }] : []
            }
          />
        </Field>
      </FieldGroup>
      {save.isError && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("partyForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {link.mode === "add" ? t("link.add") : t("partyForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}

function pickerParams(
  candidates: PartyLinkDialogProps["candidates"],
  q: string,
): ListPartiesParams {
  return { ...candidates, q: q === "" ? undefined : q, status: "active", pageSize: pickerPageSize };
}
