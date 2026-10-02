import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { createProduction, editProduction } from "@/api/endpoints/riders/riders";
import type { ProductionDetails } from "@/api/model";
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
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";
import { i18n } from "@/lib/i18n";

const nameMaxLength = 200;
const descriptionMaxLength = 2000;

/** A new production of an artist, or the production being changed. */
export type ProductionEdit =
  | { mode: "create"; artistId: string; artistName: string }
  | { mode: "edit"; production: ProductionDetails };

export type ProductionFormDialogProps = {
  edit: ProductionEdit | undefined;
  onClose: () => void;
  onSaved: (production: ProductionDetails) => void;
};

// Creating an artist's production, which opens its empty rider, or changing its name and description; the
// artist never changes (US-ART-001, riders §5).
export function ProductionFormDialog({ edit, onClose, onSaved }: ProductionFormDialogProps) {
  const { t } = useTranslation("riders");

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
            {edit?.mode === "edit"
              ? t("productionForm.editTitle")
              : t("productionForm.createTitle")}
          </DialogTitle>
          <DialogDescription>
            {edit === undefined
              ? ""
              : t("productionForm.artist", {
                  name:
                    edit.mode === "create" ? edit.artistName : (edit.production.artistName ?? ""),
                })}
          </DialogDescription>
        </DialogHeader>
        {edit !== undefined && <ProductionForm edit={edit} onCancel={onClose} onSaved={onSaved} />}
      </DialogContent>
    </Dialog>
  );
}

function ProductionForm({
  edit,
  onCancel,
  onSaved,
}: {
  edit: ProductionEdit;
  onCancel: () => void;
  onSaved: (production: ProductionDetails) => void;
}) {
  const { t } = useTranslation("riders");
  const current = edit.mode === "edit" ? edit.production : undefined;
  const [name, setName] = useState(current?.name ?? "");
  const [description, setDescription] = useState(current?.description ?? "");
  const [problems, setProblems] = useState<Partial<Record<"name" | "description", string>>>({});
  const save = useMutation({
    mutationFn: () => {
      const body = {
        name: name.trim(),
        description: description.trim() === "" ? null : description.trim(),
      };
      return edit.mode === "create"
        ? createProduction({ artistPartyId: edit.artistId, ...body })
        : editProduction(edit.production.id, body, ifMatch(edit.production.version));
    },
    onSuccess: onSaved,
  });
  // A taken name belongs under the name field (BR-RDR-009).
  const isOnName = save.error instanceof ApiError && save.error.code === "BR-RDR-009";
  const nameProblem = problems.name ?? (isOnName ? errorMessage(save.error) : undefined);

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) => {
        event.preventDefault();
        const found: typeof problems = {};
        if (name.trim() === "") {
          found.name = i18n.t("validation:required");
        } else if (name.trim().length > nameMaxLength) {
          found.name = i18n.t("validation:maxLength", { max: nameMaxLength });
        }

        if (description.trim().length > descriptionMaxLength) {
          found.description = i18n.t("validation:maxLength", { max: descriptionMaxLength });
        }

        setProblems(found);
        if (Object.keys(found).length === 0) {
          save.mutate();
        }
      }}
    >
      <FieldGroup>
        <Field data-invalid={nameProblem !== undefined}>
          <FieldLabel htmlFor="production-name">{t("productionForm.name")} *</FieldLabel>
          <Input
            id="production-name"
            value={name}
            autoComplete="off"
            aria-invalid={nameProblem !== undefined}
            onChange={(event) => {
              setName(event.target.value);
            }}
          />
          <FieldError errors={nameProblem === undefined ? [] : [{ message: nameProblem }]} />
        </Field>
        <Field data-invalid={problems.description !== undefined}>
          <FieldLabel htmlFor="production-description">
            {t("productionForm.description")}
          </FieldLabel>
          <textarea
            id="production-description"
            rows={4}
            value={description}
            aria-invalid={problems.description !== undefined}
            onChange={(event) => {
              setDescription(event.target.value);
            }}
            className="rounded-md border border-input bg-background px-3 py-2 text-sm focus-visible:ring-3 focus-visible:ring-ring focus-visible:outline-none"
          />
          <FieldError
            errors={problems.description === undefined ? [] : [{ message: problems.description }]}
          />
        </Field>
      </FieldGroup>
      {save.isError && !isOnName && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("productionForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {edit.mode === "create" ? t("productionForm.create") : t("productionForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
