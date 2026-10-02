import { describe, expect, it } from "vitest";

import type { EquipmentModelDetails } from "@/api/model";

import { modelFormValues, modelRequest, modelSchema } from "./model-form";

const values = {
  brand: "Shure",
  name: "SM58",
  categoryId: "c-1",
  trackingType: "serialized" as const,
  weightKilograms: "0,298",
  powerWatts: "",
  transportVolumeCubicMeters: "",
};

describe("modelSchema", () => {
  it("takes a comma or a dot and refuses a value that is not a positive decimal", () => {
    expect(modelSchema.safeParse(values).success).toBe(true);
    expect(modelSchema.safeParse({ ...values, weightKilograms: "0.298" }).success).toBe(true);
    const result = modelSchema.safeParse({
      ...values,
      weightKilograms: "1,2345",
      powerWatts: "0",
      transportVolumeCubicMeters: "abc",
    });
    expect(result.error?.issues.map((issue) => issue.path.join("."))).toEqual([
      "weightKilograms",
      "powerWatts",
      "transportVolumeCubicMeters",
    ]);
  });
});

describe("modelRequest", () => {
  it("sends decimals as the API's text and leaves empty values out", () => {
    expect(modelRequest(values)).toEqual({
      brand: "Shure",
      name: "SM58",
      categoryId: "c-1",
      trackingType: "serialized",
      weightKilograms: "0.298",
      powerWatts: null,
      transportVolumeCubicMeters: null,
    });
  });
});

describe("modelFormValues", () => {
  it("shows the API's decimals with a comma and without the scale's trailing zeros", () => {
    const form = modelFormValues({
      weightKilograms: "12.500",
      transportVolumeCubicMeters: "0.020",
      powerWatts: 300,
    } as EquipmentModelDetails);

    expect(form.weightKilograms).toBe("12,5");
    expect(form.transportVolumeCubicMeters).toBe("0,02");
    expect(form.powerWatts).toBe("300");
  });
});
