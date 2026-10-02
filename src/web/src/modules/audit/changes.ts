import { z } from "zod";

import type { AuditEntryItem } from "@/api/model";
import { formatDateTime } from "@/lib/format";
import { i18n } from "@/lib/i18n";

/** One changed field, ready to show: its name, and its old and new value as text. */
export type FieldChange = {
  field: string;
  label: string;
  old: string | undefined;
  new: string | undefined;
};

const changesSchema = z.record(
  z.string(),
  z.object({ old: z.unknown().optional(), new: z.unknown().optional() }),
);
const isoMoment = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/u;

// The record's module owns the names of its fields and values ({module}:{type}.fields.{field}, audit §4).
// A name it has not written yet shows as the raw field or value.
function named(module: string, path: string, fallback: string): string {
  const text: unknown = i18n.getResource(i18n.language, module, path);
  return typeof text === "string" ? text : fallback;
}

function show(entry: AuditEntryItem, field: string, value: unknown): string {
  if (value === null || value === undefined) {
    return i18n.t("audit:changes.none");
  }

  if (typeof value === "boolean") {
    return i18n.t(value ? "audit:changes.yes" : "audit:changes.no");
  }

  if (Array.isArray(value)) {
    return value.length === 0
      ? i18n.t("audit:changes.none")
      : value.map((item) => show(entry, field, item)).join(", ");
  }

  if (typeof value === "string") {
    return isoMoment.test(value)
      ? formatDateTime(value)
      : named(entry.module, `${entry.entityType}.values.${field}.${value}`, value);
  }

  return typeof value === "number" ? String(value) : JSON.stringify(value);
}

/** The entry's changed fields with their names and values in Turkish. */
export function describeChanges(entry: AuditEntryItem): FieldChange[] {
  const parsed = changesSchema.safeParse(entry.changes);
  if (!parsed.success) {
    return [];
  }

  return Object.entries(parsed.data).map(([field, change]) => ({
    field,
    label: named(entry.module, `${entry.entityType}.fields.${field}`, field),
    old: "old" in change ? show(entry, field, change.old) : undefined,
    new: "new" in change ? show(entry, field, change.new) : undefined,
  }));
}

/** The record type's name, e.g. Kullanıcı. */
export function entityTypeName(entityType: string): string {
  return named("audit", `entityTypes.${entityType}`, entityType);
}
