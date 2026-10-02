import { describe, expect, it } from "vitest";

import type { KitDetails } from "@/api/model";

import { invalidDraftKeys, kitLineDrafts, kitLineRequests, moveDraft } from "./kit-lines";

const kit = {
  lines: [
    {
      id: "l-1",
      modelId: "m-1",
      subKitId: null,
      name: "Robe Spiider",
      quantity: 4,
      isActive: true,
    },
    { id: "l-2", modelId: null, subKitId: "k-2", name: "Kablo seti", quantity: 3, isActive: true },
  ],
} as unknown as KitDetails;

describe("kit lines", () => {
  it("starts the editor from the kit and sends each line as a model or a kit", () => {
    const drafts = kitLineDrafts(kit);

    expect(kitLineRequests(drafts)).toEqual([
      { modelId: "m-1", quantity: 4 },
      { subKitId: "k-2", quantity: 3 },
    ]);
  });

  it("marks lines without a target or with a quantity outside 1–9999", () => {
    const drafts = [
      ...kitLineDrafts(kit),
      { key: "n-1", kind: "model" as const, target: null, quantity: "1" },
      { key: "n-2", kind: "model" as const, target: { id: "m-3", label: "X" }, quantity: "0" },
      { key: "n-3", kind: "kit" as const, target: { id: "k-4", label: "Y" }, quantity: "12345" },
    ];

    expect([...invalidDraftKeys(drafts)]).toEqual(["n-1", "n-2", "n-3"]);
  });

  it("moves a line within the list and keeps it at the ends", () => {
    const drafts = kitLineDrafts(kit);

    expect(moveDraft(drafts, 1, -1).map((draft) => draft.key)).toEqual(["l-2", "l-1"]);
    expect(moveDraft(drafts, 0, -1).map((draft) => draft.key)).toEqual(["l-1", "l-2"]);
  });
});
