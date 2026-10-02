import { z } from "zod";

import { type EquipmentModelDetails, type EquipmentModelRequest, TrackingType } from "@/api/model";
import { decimalField, fromApiDecimal, optionalDecimal, wholeField } from "@/lib/decimal-input";

export const modelSchema = z.object({
  brand: z.string().trim().min(1).max(100),
  name: z.string().trim().min(1).max(200),
  categoryId: z.string().min(1),
  trackingType: z.enum(TrackingType),
  weightKilograms: decimalField(10_000_000, 3),
  powerWatts: wholeField(1_000_000),
  transportVolumeCubicMeters: decimalField(100_000, 3),
});

export type ModelFormValues = z.infer<typeof modelSchema>;

/** The form's values for a new model, or for the one being edited; decimals with a Turkish comma. */
export function modelFormValues(model: EquipmentModelDetails | undefined): ModelFormValues {
  return {
    brand: model?.brand ?? "",
    name: model?.name ?? "",
    categoryId: model?.categoryId ?? "",
    trackingType: model?.trackingType ?? TrackingType.serialized,
    weightKilograms: fromApiDecimal(model?.weightKilograms),
    powerWatts:
      model?.powerWatts === null || model?.powerWatts === undefined ? "" : String(model.powerWatts),
    transportVolumeCubicMeters: fromApiDecimal(model?.transportVolumeCubicMeters),
  };
}

/** The request body: decimals as the API's text (api A-11), empty values left out. */
export function modelRequest(values: ModelFormValues): EquipmentModelRequest {
  return {
    brand: values.brand,
    name: values.name,
    categoryId: values.categoryId,
    trackingType: values.trackingType,
    weightKilograms: optionalDecimal(values.weightKilograms),
    powerWatts: values.powerWatts === "" ? null : Number(values.powerWatts),
    transportVolumeCubicMeters: optionalDecimal(values.transportVolumeCubicMeters),
  };
}
