import { describe, expect, it } from "vitest";

import type { EquipmentCategoryItem } from "@/api/model";

import { categoryRows, parentChoices, pathLabel } from "./category-tree";

function aCategory(
  id: string,
  name: string,
  parentId: string | null,
  path: string[],
): EquipmentCategoryItem {
  return { id, name, parentId, path, activeModelCount: 0, isActive: true, version: 1 };
}

const sound = aCategory("s", "Ses", null, ["Ses"]);
const light = aCategory("l", "Işık", null, ["Işık"]);
const microphone = aCategory("m", "Mikrofon", "s", ["Ses", "Mikrofon"]);
const vocal = aCategory("v", "Dinamik vokal", "m", ["Ses", "Mikrofon", "Dinamik vokal"]);

describe("categoryRows", () => {
  it("puts each category after its parent, keeping the list's order among siblings", () => {
    expect(
      categoryRows([vocal, light, microphone, sound]).map((row) => [row.category.id, row.depth]),
    ).toEqual([
      ["l", 0],
      ["s", 0],
      ["m", 1],
      ["v", 2],
    ]);
  });

  it("starts a branch at a category whose parent is filtered out", () => {
    expect(categoryRows([vocal]).map((row) => [row.category.id, row.depth])).toEqual([["v", 0]]);
  });
});

describe("parentChoices", () => {
  it("leaves out the moved category and everything below it", () => {
    expect(
      parentChoices([sound, light, microphone, vocal], "s").map((category) => category.id),
    ).toEqual(["l"]);
    expect(parentChoices([sound, light], undefined)).toHaveLength(2);
  });
});

describe("pathLabel", () => {
  it("joins the path as people read it", () => {
    expect(pathLabel(vocal)).toBe("Ses › Mikrofon › Dinamik vokal");
  });
});
