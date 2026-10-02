import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { createKit, editKit } from "@/api/endpoints/catalog/catalog";
import type { KitDetails } from "@/api/model";
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

import { kitLineDrafts, kitLineRequests } from "../kit-lines";

export type KitNameDialogProps = {
  open: boolean;
  /** The kit being renamed; a new, empty kit without it. */
  kit?: KitDetails | undefined;
  onClose: () => void;
  onSaved: (kit: KitDetails, isNew: boolean) => void;
};

// Creating a kit by its name, its lines then added on its page; or renaming one (catalog §8).
export function KitNameDialog({ open, kit, onClose, onSaved }: KitNameDialogProps) {
  const { t } = useTranslation("catalog");

  return (
    <Dialog
      open={open}
      onOpenChange={(isOpen) => {
        if (!isOpen) {
          onClose();
        }
      }}
    >
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {kit === undefined ? t("kitForm.createTitle") : t("kitForm.renameTitle")}
          </DialogTitle>
          <DialogDescription>{t("kitForm.required")}</DialogDescription>
        </DialogHeader>
        {open && <KitNameForm kit={kit} onCancel={onClose} onSaved={onSaved} />}
      </DialogContent>
    </Dialog>
  );
}

function KitNameForm({
  kit,
  onCancel,
  onSaved,
}: {
  kit: KitDetails | undefined;
  onCancel: () => void;
  onSaved: (kit: KitDetails, isNew: boolean) => void;
}) {
  const { t } = useTranslation("catalog");
  const [name, setName] = useState(kit?.name ?? "");
  const [problem, setProblem] = useState<string | undefined>(undefined);
  const save = useMutation({
    mutationFn: (trimmed: string) =>
      kit === undefined
        ? createKit({ name: trimmed, lines: [] })
        : editKit(
            kit.id,
            { name: trimmed, lines: kitLineRequests(kitLineDrafts(kit)) },
            ifMatch(kit.version),
          ),
    onSuccess: (saved) => {
      onSaved(saved, kit === undefined);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === "BR-EQP-013") {
        setProblem(errorMessage(error));
      }
    },
  });
  const isOnName = save.error instanceof ApiError && save.error.code === "BR-EQP-013";

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) => {
        event.preventDefault();
        const trimmed = name.trim();
        if (trimmed === "" || trimmed.length > 200) {
          setProblem(
            trimmed === ""
              ? i18n.t("validation:required")
              : i18n.t("validation:maxLength", { max: 200 }),
          );
          return;
        }

        setProblem(undefined);
        save.mutate(trimmed);
      }}
    >
      <FieldGroup>
        <Field data-invalid={problem !== undefined}>
          <FieldLabel htmlFor="kit-name">{t("kitForm.name")} *</FieldLabel>
          <Input
            id="kit-name"
            value={name}
            autoComplete="off"
            aria-invalid={problem !== undefined}
            onChange={(event) => {
              setName(event.target.value);
            }}
          />
          <FieldError errors={problem === undefined ? [] : [{ message: problem }]} />
        </Field>
      </FieldGroup>
      {save.isError && !isOnName && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("kitForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {kit === undefined ? t("kitForm.create") : t("kitForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
