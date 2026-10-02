import { z } from "zod";

import {
  ContactPointKind,
  type ContactPointRequest,
  type PartyDetails,
  PartyKind,
  type PartyRequest,
  PartyRole,
} from "@/api/model";
import { i18n } from "@/lib/i18n";

// The same bounds the server checks (parties §8); the server stays the judge.
const phonePattern = /^\+?[0-9 ()-]+$/u;
const maxContactPoints = 20;

const required = () => i18n.t("validation:required");

const contactPoint = z
  .object({
    id: z.string().optional(),
    kind: z.enum(ContactPointKind),
    value: z.string().trim().min(1).max(500),
    label: z.string().trim().max(100),
    isPrimary: z.boolean(),
  })
  .superRefine((point, context) => {
    const digits = point.value.replace(/\D/gu, "").length;
    if (
      point.kind === ContactPointKind.phone &&
      (point.value.length > 32 || !phonePattern.test(point.value) || digits < 7 || digits > 15)
    ) {
      context.addIssue({
        code: "custom",
        path: ["value"],
        message: i18n.t("parties:partyForm.invalidPhone"),
      });
    }

    if (
      point.kind === ContactPointKind.email &&
      !z.email().max(254).safeParse(point.value).success
    ) {
      context.addIssue({
        code: "custom",
        path: ["value"],
        message: i18n.t("parties:partyForm.invalidEmail"),
      });
    }
  });

export const partySchema = z
  .object({
    kind: z.enum(PartyKind),
    name: z.string().trim().min(1).max(200),
    firstName: z.string().trim().max(100),
    lastName: z.string().trim().max(100),
    legalName: z.string().trim().max(200),
    roles: z.array(z.enum(PartyRole)).min(1),
    contactPoints: z.array(contactPoint).max(maxContactPoints),
  })
  .superRefine((values, context) => {
    if (values.kind === PartyKind.person) {
      for (const name of ["firstName", "lastName"] as const) {
        if (values[name] === "") {
          context.addIssue({ code: "custom", path: [name], message: required() });
        }
      }
    }
  });

export type PartyFormValues = z.infer<typeof partySchema>;

export type ContactPointValues = PartyFormValues["contactPoints"][number];

/** The form's values for a new party, or for the one being edited. */
export function partyFormValues(party: PartyDetails | undefined): PartyFormValues {
  return {
    kind: party?.kind ?? PartyKind.organization,
    name: party?.name ?? "",
    firstName: party?.firstName ?? "",
    lastName: party?.lastName ?? "",
    legalName: party?.legalName ?? "",
    roles: party?.roles ?? [],
    contactPoints:
      party?.contactPoints.map((point) => ({
        id: point.id,
        kind: point.kind,
        value: point.value,
        label: point.label ?? "",
        isPrimary: point.isPrimary,
      })) ?? [],
  };
}

/** The request body; names the kind does not use are left out (parties §8). */
export function partyRequest(values: PartyFormValues): PartyRequest {
  const isPerson = values.kind === PartyKind.person;
  return {
    kind: values.kind,
    name: values.name,
    firstName: isPerson ? values.firstName : null,
    lastName: isPerson ? values.lastName : null,
    legalName: isPerson || values.legalName === "" ? null : values.legalName,
    roles: values.roles,
    contactPoints: values.contactPoints.map((point): ContactPointRequest => ({
      id: point.id ?? null,
      kind: point.kind,
      value: point.value,
      label: point.label === "" ? null : point.label,
      isPrimary: point.isPrimary,
    })),
  };
}

/**
 * Marks one contact point as the primary of its kind and leaves the others of that kind unmarked, as the
 * server keeps one primary per kind (BR-PTY-002).
 */
export function withPrimary(
  points: readonly ContactPointValues[],
  index: number,
): ContactPointValues[] {
  const kind = points[index]?.kind;
  return points.map((point, position) =>
    point.kind === kind ? { ...point, isPrimary: position === index } : point,
  );
}
