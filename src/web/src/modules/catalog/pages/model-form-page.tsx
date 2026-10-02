import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { Lock } from "lucide-react";
import { Controller, useForm } from "react-hook-form";
import { useTranslation } from "react-i18next";

import {
  createEquipmentModel,
  editEquipmentModel,
  getEquipmentModel,
  getGetEquipmentModelQueryKey,
  getListEquipmentCategoriesQueryKey,
  getListEquipmentModelsQueryKey,
  listEquipmentCategories,
} from "@/api/endpoints/catalog/catalog";
import { CategoryStatusFilter, type EquipmentModelDetails, TrackingType } from "@/api/model";
import { EmptyState } from "@/components/common/empty-state";
import { ErrorState } from "@/components/common/error-state";
import { FormAlert } from "@/components/common/form-alert";
import { PageHeader } from "@/components/common/page-header";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { Button } from "@/components/ui/button";
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
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { pathLabel } from "../category-tree";
import { type ModelFormValues, modelFormValues, modelRequest, modelSchema } from "../model-form";
import { catalogPermissions } from "../permissions";

const everyCategory = { status: CategoryStatusFilter.all };
const textFields = ["brand", "name"] as const;
const measureFields = ["weightKilograms", "powerWatts", "transportVolumeCubicMeters"] as const;

export type ModelFormPageProps = {
  /** The model being edited; a new model without it. */
  modelId?: string | undefined;
  onSaved: (model: EquipmentModelDetails) => void;
  onCancel: () => void;
};

// Creating or editing a model on a page of its own (11 §2.4, catalog CT-05): it grows with the power and
// loading values of later releases.
export function ModelFormPage({ modelId, onSaved, onCancel }: ModelFormPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const isEditing = modelId !== undefined;
  const canSave = hasPermission(
    user,
    isEditing ? catalogPermissions.editModels : catalogPermissions.createModels,
  );
  const model = useQuery({
    queryKey: getGetEquipmentModelQueryKey(modelId ?? ""),
    queryFn: ({ signal }) => getEquipmentModel(modelId ?? "", { signal }),
    enabled: canSave && isEditing,
  });
  const categories = useQuery({
    queryKey: getListEquipmentCategoriesQueryKey(everyCategory),
    queryFn: ({ signal }) => listEquipmentCategories(everyCategory, { signal }),
    enabled: canSave,
  });

  if (!canSave) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("models.forbidden")} />
      </div>
    );
  }

  const failed = model.isError ? model : categories.isError ? categories : undefined;
  if (failed !== undefined) {
    return (
      <div className="p-6">
        <ErrorState
          error={failed.error}
          onRetry={() => {
            void failed.refetch();
          }}
        />
      </div>
    );
  }

  if ((isEditing && model.data === undefined) || categories.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-96 w-full" />
      </div>
    );
  }

  // Active categories, and the model's own even if it was deactivated since (BR-SYS-001).
  const categoryOptions = categories.data
    .filter((category) => category.isActive || category.id === model.data?.categoryId)
    .map((category) => ({ value: category.id, label: pathLabel(category) }))
    .sort((left, right) => left.label.localeCompare(right.label, "tr"));

  return (
    <ModelForm
      key={model.data?.version ?? "new"}
      model={model.data}
      categoryOptions={categoryOptions}
      onSaved={onSaved}
      onCancel={onCancel}
    />
  );
}

type ModelFormProps = {
  model: EquipmentModelDetails | undefined;
  categoryOptions: readonly { value: string; label: string }[];
  onSaved: (model: EquipmentModelDetails) => void;
  onCancel: () => void;
};

function ModelForm({ model, categoryOptions, onSaved, onCancel }: ModelFormProps) {
  const { t } = useTranslation("catalog");
  const queryClient = useQueryClient();
  const { control, handleSubmit, setError } = useForm<ModelFormValues>({
    resolver: zodResolver(modelSchema),
    defaultValues: modelFormValues(model),
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const save = useMutation({
    mutationFn: (values: ModelFormValues) =>
      model === undefined
        ? createEquipmentModel(modelRequest(values))
        : editEquipmentModel(model.id, modelRequest(values), ifMatch(model.version)),
    onSuccess: (saved) => {
      queryClient.setQueryData(getGetEquipmentModelQueryKey(saved.id), saved);
      void queryClient.invalidateQueries({ queryKey: getListEquipmentModelsQueryKey() });
      void queryClient.invalidateQueries({ queryKey: getListEquipmentCategoriesQueryKey() });
      onSaved(saved);
    },
    onError: (error) => {
      if (!(error instanceof ApiError)) {
        return;
      }

      if (error.code === "BR-EQP-012") {
        setError("name", { message: errorMessage(error) }, { shouldFocus: true });
      }

      if (error.code === "BR-EQP-002") {
        setError("categoryId", { message: errorMessage(error) });
      }

      for (const field of fieldErrors(error)) {
        const name = [...textFields, ...measureFields, "categoryId" as const].find(
          (known) => known === field.path,
        );
        if (name !== undefined) {
          setError(name, { message: field.message }, { shouldFocus: true });
        }
      }
    },
  });
  const isShownOnFields =
    save.error instanceof ApiError &&
    (save.error.code === "BR-EQP-012" ||
      save.error.code === "BR-EQP-002" ||
      save.error.status === 400);
  const trackingTypes = Object.values(TrackingType).map((value) => ({
    value,
    label: t(`trackingTypes.${value}`),
  }));
  const title = model === undefined ? t("modelForm.createTitle") : t("modelForm.editTitle");

  return (
    <form
      noValidate
      className="flex flex-col gap-6 p-6"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          save.mutate(values);
        })(event)
      }
    >
      <PageHeader
        title={title}
        description={t("modelForm.required")}
        breadcrumb={
          <Link to="/catalog/models" className="hover:underline">
            {t("models.title")}
          </Link>
        }
        actions={
          <FormActions isNew={model === undefined} isPending={save.isPending} onCancel={onCancel} />
        }
      />
      <FieldSet className="max-w-2xl">
        <FieldLegend>{t("modelForm.general")}</FieldLegend>
        <FieldGroup>
          {textFields.map((name) => (
            <Controller
              key={name}
              control={control}
              name={name}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={`model-${name}`}>{t(`modelForm.${name}`)} *</FieldLabel>
                  <Input
                    {...field}
                    id={`model-${name}`}
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
            name="categoryId"
            render={({ field, fieldState }) => (
              <Field data-invalid={fieldState.invalid}>
                <FieldLabel htmlFor="model-category">{t("modelForm.category")} *</FieldLabel>
                <Select
                  items={categoryOptions}
                  value={field.value === "" ? null : field.value}
                  onValueChange={(value) => {
                    field.onChange(value ?? "");
                  }}
                >
                  <SelectTrigger
                    id="model-category"
                    className="w-full"
                    aria-invalid={fieldState.invalid}
                  >
                    <SelectValue placeholder={t("modelForm.chooseCategory")} />
                  </SelectTrigger>
                  <SelectContent>
                    {categoryOptions.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError errors={[fieldState.error]} />
              </Field>
            )}
          />
          <Controller
            control={control}
            name="trackingType"
            render={({ field }) => (
              <Field data-disabled={model?.hasStock === true}>
                <FieldLabel htmlFor="model-trackingType">
                  {t("modelForm.trackingType")} *
                </FieldLabel>
                <Select
                  items={trackingTypes}
                  value={field.value}
                  disabled={model?.hasStock === true}
                  onValueChange={(value) => {
                    field.onChange(value ?? TrackingType.serialized);
                  }}
                >
                  <SelectTrigger id="model-trackingType" className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {trackingTypes.map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldDescription>
                  {model?.hasStock === true
                    ? t("modelForm.trackingTypeFixed")
                    : t("modelForm.trackingTypeHelp")}
                </FieldDescription>
              </Field>
            )}
          />
        </FieldGroup>
      </FieldSet>
      <FieldSet className="max-w-2xl">
        <FieldLegend>{t("modelForm.measures")}</FieldLegend>
        <FieldGroup className="sm:grid sm:grid-cols-3">
          {measureFields.map((name) => (
            <Controller
              key={name}
              control={control}
              name={name}
              render={({ field, fieldState }) => (
                <Field data-invalid={fieldState.invalid}>
                  <FieldLabel htmlFor={`model-${name}`}>{t(`modelForm.${name}`)}</FieldLabel>
                  <Input
                    {...field}
                    id={`model-${name}`}
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
      </FieldSet>
      {save.isError && !isShownOnFields && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <div className="flex max-w-2xl justify-end">
        <FormActions isNew={model === undefined} isPending={save.isPending} onCancel={onCancel} />
      </div>
    </form>
  );
}

function FormActions({
  isNew,
  isPending,
  onCancel,
}: {
  isNew: boolean;
  isPending: boolean;
  onCancel: () => void;
}) {
  const { t } = useTranslation("catalog");
  return (
    <div className="flex gap-2">
      <Button type="button" variant="outline" onClick={onCancel}>
        {t("modelForm.cancel")}
      </Button>
      <Button type="submit" isLoading={isPending}>
        {isNew ? t("modelForm.create") : t("modelForm.save")}
      </Button>
    </div>
  );
}
