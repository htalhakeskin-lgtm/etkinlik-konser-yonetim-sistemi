import { z } from "zod";

import { type EquipmentModelDetails, type EquipmentModelRequest, TrackingType } from "@/api/model";
import { i18n } from "@/lib/i18n";

// A decimal as people type it: digits with a comma or a dot and at most three decimals.
const decimalPattern = /^\d{1,7}([.,]\d{1,3})?$/u;

const decimal = (maximum: number) =>
  z
    .string()
    .trim()
    .refine((value) => value === "" || decimalPattern.test(value), {
      message: i18n.t("catalog:modelForm.invalidDecimal"),
    })
    .refine(
      (value) =>
        value === "" ||
        !decimalPattern.test(value) ||
        (Number(toApi(value)) > 0 && Number(toApi(value)) < maximum),
      {
        message: i18n.t("catalog:modelForm.positive"),
      },
    );

export const modelSchema = z.object({
  brand: z.string().trim().min(1).max(100),
  name: z.string().trim().min(1).max(200),
  categoryId: z.string().min(1),
  trackingType: z.enum(TrackingType),
  weightKilograms: decimal(10_000_000),
  powerWatts: z
    .string()
    .trim()
    .refine((value) => value === "" || (/^\d{1,7}$/u.test(value) && Number(value) > 0), {
      message: i18n.t("catalog:modelForm.positiveWhole"),
    }),
  transportVolumeCubicMeters: decimal(100_000),
});

export type ModelFormValues = z.infer<typeof modelSchema>;

/** The form's values for a new model, or for the one being edited; decimals with a Turkish comma. */
export function modelFormValues(model: EquipmentModelDetails | undefined): ModelFormValues {
  return {
    brand: model?.brand ?? "",
    name: model?.name ?? "",
    categoryId: model?.categoryId ?? "",
    trackingType: model?.trackingType ?? TrackingType.serialized,
    weightKilograms: fromApi(model?.weightKilograms),
    powerWatts:
      model?.powerWatts === null || model?.powerWatts === undefined ? "" : String(model.powerWatts),
    transportVolumeCubicMeters: fromApi(model?.transportVolumeCubicMeters),
  };
}

/** The request body: decimals as the API's text (api A-11), empty values left out. */
export function modelRequest(values: ModelFormValues): EquipmentModelRequest {
  return {
    brand: values.brand,
    name: values.name,
    categoryId: values.categoryId,
    trackingType: values.trackingType,
    weightKilograms: values.weightKilograms === "" ? null : toApi(values.weightKilograms),
    powerWatts: values.powerWatts === "" ? null : Number(values.powerWatts),
    transportVolumeCubicMeters:
      values.transportVolumeCubicMeters === "" ? null : toApi(values.transportVolumeCubicMeters),
  };
}

function toApi(value: string): string {
  return value.replace(",", ".");
}

// "12.500" from the API shows as "12,5" in the field; trailing zeros of the scale are dropped.
function fromApi(value: string | null | undefined): string {
  if (value === null || value === undefined) {
    return "";
  }

  const trimmed = value.includes(".") ? value.replace(/0+$/u, "").replace(/\.$/u, "") : value;
  return trimmed.replace(".", ",");
}
