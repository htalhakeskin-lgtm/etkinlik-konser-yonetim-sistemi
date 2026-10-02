import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import { createEquipmentCategory, editEquipmentCategory } from "@/api/endpoints/catalog/catalog";
import type { EquipmentCategoryItem } from "@/api/model";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";

import { parentChoices, pathLabel } from "../category-tree";

const schema = z.object({
  name: z.string().trim().min(1).max(200),
  parentId: z.string(),
});

type CategoryFormValues = z.infer<typeof schema>;

const topLevel = "top";

/** A category being created (under a parent, or at the top) or edited. */
export type CategoryEdit =
  { mode: "create"; parentId: string | null } | { mode: "edit"; category: EquipmentCategoryItem };

export type CategoryFormDialogProps = {
  edit: CategoryEdit | undefined;
  /** Every category, active or not, for the parent choice. */
  categories: readonly EquipmentCategoryItem[];
  onClose: () => void;
  onSaved: (category: EquipmentCategoryItem, isNew: boolean) => void;
};

// Creating, renaming or moving a category (US-EQP-001): a small record, so a dialog (ui §7.2).
export function CategoryFormDialog({
  edit,
  categories,
  onClose,
  onSaved,
}: CategoryFormDialogProps) {
  const { t } = useTranslation("catalog");

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
            {edit?.mode === "edit" ? t("categoryForm.editTitle") : t("categoryForm.createTitle")}
          </DialogTitle>
          <DialogDescription>{t("categoryForm.required")}</DialogDescription>
        </DialogHeader>
        {edit !== undefined && (
          <CategoryForm edit={edit} categories={categories} onCancel={onClose} onSaved={onSaved} />
        )}
      </DialogContent>
    </Dialog>
  );
}

type CategoryFormProps = {
  edit: CategoryEdit;
  categories: readonly EquipmentCategoryItem[];
  onCancel: () => void;
  onSaved: (category: EquipmentCategoryItem, isNew: boolean) => void;
};

function CategoryForm({ edit, categories, onCancel, onSaved }: CategoryFormProps) {
  const { t } = useTranslation("catalog");
  const current = edit.mode === "edit" ? edit.category : undefined;
  const parentId =
    current === undefined ? (edit.mode === "create" ? edit.parentId : null) : current.parentId;
  const { control, handleSubmit, setError } = useForm<CategoryFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: current?.name ?? "", parentId: parentId ?? topLevel },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const save = useMutation({
    mutationFn: (values: CategoryFormValues) => {
      const body = {
        name: values.name,
        parentId: values.parentId === topLevel ? null : values.parentId,
      };
      return current === undefined
        ? createEquipmentCategory(body)
        : editEquipmentCategory(current.id, body, ifMatch(current.version));
    },
    onSuccess: (saved) => {
      onSaved(saved, current === undefined);
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === "BR-EQP-011") {
        setError("name", { message: errorMessage(error) }, { shouldFocus: true });
      }
    },
  });
  const isOnName = save.error instanceof ApiError && save.error.code === "BR-EQP-011";
  const parents = [
    { value: topLevel, label: t("categoryForm.topLevel") },
    ...parentChoices(categories, current?.id)
      .filter((category) => category.isActive)
      .map((category) => ({ value: category.id, label: pathLabel(category) }))
      .sort((left, right) => left.label.localeCompare(right.label, "tr")),
  ];

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          save.mutate(values);
        })(event)
      }
    >
      <FieldGroup>
        <Controller
          control={control}
          name="name"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="category-name">{t("categoryForm.name")} *</FieldLabel>
              <Input
                {...field}
                id="category-name"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="parentId"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="category-parent">{t("categoryForm.parent")}</FieldLabel>
              <Select
                items={parents}
                value={field.value}
                onValueChange={(value) => {
                  field.onChange(value ?? topLevel);
                }}
              >
                <SelectTrigger id="category-parent" className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {parents.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
          )}
        />
      </FieldGroup>
      {save.isError && !isOnName && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("categoryForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {current === undefined ? t("categoryForm.create") : t("categoryForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
