import { z } from "zod";

import type { VenueDetails, VenueRequest } from "@/api/model";
import { decimalField, fromApiDecimal, optionalDecimal, wholeField } from "@/lib/decimal-input";
import { i18n } from "@/lib/i18n";

/** The time zone a venue gets unless told otherwise (database §7.2). */
export const defaultTimeZone = "Europe/Istanbul";

export const venueSchema = z.object({
  name: z.string().trim().min(1).max(200),
  city: z.string().trim().min(1).max(100),
  address: z.string().trim().min(1).max(500),
  capacity: wholeField(1_000_000).refine((value) => value !== "", {
    message: i18n.t("validation:required"),
  }),
  operator: z.object({ id: z.string(), label: z.string() }).nullable(),
  stageWidthMeters: decimalField(1000, 2),
  stageDepthMeters: decimalField(1000, 2),
  stageHeightMeters: decimalField(1000, 2),
  loadingDock: z.string().trim().max(2000),
  powerCapacityAmperes: decimalField(100_000, 2),
  curfew: z.string(),
  timeZone: z.string().trim().min(1).max(64),
});

export type VenueFormValues = z.infer<typeof venueSchema>;

/** The form's values for a new venue, or for the one being edited. */
export function venueFormValues(venue: VenueDetails | undefined): VenueFormValues {
  return {
    name: venue?.name ?? "",
    city: venue?.city ?? "",
    address: venue?.address ?? "",
    capacity: venue === undefined ? "" : String(venue.capacity),
    operator:
      venue?.operatorPartyId === null || venue?.operatorPartyId === undefined
        ? null
        : { id: venue.operatorPartyId, label: venue.operatorName ?? "" },
    stageWidthMeters: fromApiDecimal(venue?.stageWidthMeters),
    stageDepthMeters: fromApiDecimal(venue?.stageDepthMeters),
    stageHeightMeters: fromApiDecimal(venue?.stageHeightMeters),
    loadingDock: venue?.loadingDock ?? "",
    powerCapacityAmperes: fromApiDecimal(venue?.powerCapacityAmperes),
    curfew: venue?.curfew?.slice(0, 5) ?? "",
    timeZone: venue?.timeZone ?? defaultTimeZone,
  };
}

/** The request body: decimals as the API's text (api A-11), the curfew as a time, empty values left out. */
export function venueRequest(values: VenueFormValues): VenueRequest {
  return {
    name: values.name,
    city: values.city,
    address: values.address,
    capacity: Number(values.capacity),
    operatorPartyId: values.operator?.id ?? null,
    stageWidthMeters: optionalDecimal(values.stageWidthMeters),
    stageDepthMeters: optionalDecimal(values.stageDepthMeters),
    stageHeightMeters: optionalDecimal(values.stageHeightMeters),
    loadingDock: values.loadingDock === "" ? null : values.loadingDock,
    powerCapacityAmperes: optionalDecimal(values.powerCapacityAmperes),
    curfew: values.curfew === "" ? null : `${values.curfew}:00`,
    timeZone: values.timeZone,
  };
}
