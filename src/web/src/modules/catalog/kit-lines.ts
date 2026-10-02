import type { KitDetails, KitLineRequest } from "@/api/model";
import type { PickerOption } from "@/components/common/entity-picker";

/** A kit line in the editor: what it holds, a model or another kit, and how many as typed. */
export type KitLineDraft = {
  key: string;
  kind: "model" | "kit";
  target: PickerOption | null;
  quantity: string;
};

/** The kit's lines as the editor starts them. */
export function kitLineDrafts(kit: KitDetails): KitLineDraft[] {
  return kit.lines.map((line) => ({
    key: line.id,
    kind: line.subKitId === null ? "model" : "kit",
    target: { id: line.modelId ?? line.subKitId ?? "", label: line.name },
    quantity: String(line.quantity),
  }));
}

/** The keys of the drafts that cannot be sent: no target, or not a whole number from 1 to 9999. */
export function invalidDraftKeys(drafts: readonly KitLineDraft[]): Set<string> {
  return new Set(
    drafts
      .filter(
        (draft) =>
          draft.target === null ||
          !/^\d{1,4}$/u.test(draft.quantity.trim()) ||
          Number(draft.quantity) < 1,
      )
      .map((draft) => draft.key),
  );
}

/** The lines as the API takes them (catalog CT-04: the whole kit at once). */
export function kitLineRequests(drafts: readonly KitLineDraft[]): KitLineRequest[] {
  return drafts.map((draft) =>
    draft.kind === "model"
      ? { modelId: draft.target?.id ?? null, quantity: Number(draft.quantity) }
      : { subKitId: draft.target?.id ?? null, quantity: Number(draft.quantity) },
  );
}

/** The drafts with one moved a place up or down. */
export function moveDraft(
  drafts: readonly KitLineDraft[],
  index: number,
  by: -1 | 1,
): KitLineDraft[] {
  const target = index + by;
  if (target < 0 || target >= drafts.length) {
    return [...drafts];
  }

  const moved = [...drafts];
  const [draft] = moved.splice(index, 1);
  if (draft !== undefined) {
    moved.splice(target, 0, draft);
  }

  return moved;
}
