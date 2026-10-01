import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";
import type { z } from "zod";

import {
  createWarehouse,
  editWarehouse,
  getGetWarehouseQueryKey,
  getWarehouse,
} from "@/api/endpoints/inventory/inventory";
import type { WarehouseDetails } from "@/api/model";
import { CreateWarehouseBody } from "@/api/zod/inventory/inventory.zod";
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
import { Field, FieldError, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";

const schema = CreateWarehouseBody.extend({
  name: CreateWarehouseBody.shape.name.trim().min(1).max(100),
  city: CreateWarehouseBody.shape.city.trim().min(1).max(100),
  address: CreateWarehouseBody.shape.address.trim().min(1).max(500),
});

type WarehouseFormValues = z.infer<typeof schema>;

const formFields = ["name", "city", "address"] as const;

export type WarehouseFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** The warehouse being edited; a new warehouse without it. */
  warehouseId?: string | undefined;
  onSaved: (warehouse: WarehouseDetails, isNew: boolean) => void;
};

// Creating or editing a warehouse (US-SYS-005): a small record, so a dialog (ui §7.2).
export function WarehouseFormDialog({
  open,
  onOpenChange,
  warehouseId,
  onSaved,
}: WarehouseFormDialogProps) {
  const { t } = useTranslation("inventory");
  const isEditing = warehouseId !== undefined;
  const warehouse = useQuery({
    queryKey: getGetWarehouseQueryKey(warehouseId ?? ""),
    queryFn: ({ signal }) => getWarehouse(warehouseId ?? "", { signal }),
    enabled: open && isEditing,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("warehouseForm.editTitle") : t("warehouseForm.createTitle")}
          </DialogTitle>
          <DialogDescription>{t("warehouseForm.required")}</DialogDescription>
        </DialogHeader>
        {isEditing && warehouse.data === undefined ? (
          warehouse.isError ? (
            <FormAlert>{errorMessage(warehouse.error)}</FormAlert>
          ) : (
            <SkeletonBlock className="h-40 w-full" />
          )
        ) : (
          <WarehouseForm
            key={warehouse.data?.version ?? "new"}
            warehouse={warehouse.data}
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

type WarehouseFormProps = {
  warehouse: WarehouseDetails | undefined;
  onCancel: () => void;
  onSaved: (warehouse: WarehouseDetails, isNew: boolean) => void;
};

function WarehouseForm({ warehouse, onCancel, onSaved }: WarehouseFormProps) {
  const { t } = useTranslation("inventory");
  const { control, handleSubmit, setError } = useForm<WarehouseFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: warehouse?.name ?? "",
      city: warehouse?.city ?? "",
      address: warehouse?.address ?? "",
    },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const save = useMutation({
    mutationFn: (values: WarehouseFormValues) =>
      warehouse === undefined
        ? createWarehouse(values)
        : editWarehouse(warehouse.id, values, ifMatch(warehouse.version)),
    onSuccess: (saved) => {
      onSaved(saved, warehouse === undefined);
    },
    onError: (error) => {
      if (!(error instanceof ApiError)) {
        return;
      }

      if (error.code === "BR-SYS-016") {
        setError("name", { message: errorMessage(error) }, { shouldFocus: true });
      }

      for (const field of fieldErrors(error)) {
        const name = formFields.find((known) => known === field.path);
        if (name !== undefined) {
          setError(name, { message: field.message }, { shouldFocus: true });
        }
      }
    },
  });
  const isFieldError =
    save.error instanceof ApiError &&
    (save.error.code === "BR-SYS-016" || save.error.status === 400);

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
        {formFields.map((name) => (
          <Controller
            key={name}
            control={control}
            name={name}
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor={`warehouse-${name}`}>
                  {t(`warehouseForm.${name}`)} *
                </FieldLabel>
                <Input
                  {...field}
                  id={`warehouse-${name}`}
                  autoComplete="off"
                  aria-invalid={fieldState.invalid}
                />
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
        ))}
      </FieldGroup>
      {save.isError && !isFieldError && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("warehouseForm.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {warehouse === undefined ? t("warehouseForm.create") : t("warehouseForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
