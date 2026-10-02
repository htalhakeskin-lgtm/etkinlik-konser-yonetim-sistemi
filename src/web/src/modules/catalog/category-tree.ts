import type { EquipmentCategoryItem } from "@/api/model";

/** A category in the tree's reading order, with its depth for indentation. */
export type CategoryRow = {
  category: EquipmentCategoryItem;
  depth: number;
};

/**
 * Turns the flat list into the tree's rows (catalog CT-02): each category follows its parent, siblings keep
 * the list's Turkish order. A category whose parent is not in the list (filtered out) starts a branch of
 * its own; the screen shows its path.
 */
export function categoryRows(categories: readonly EquipmentCategoryItem[]): CategoryRow[] {
  const ids = new Set(categories.map((category) => category.id));
  const children = new Map<string | null, EquipmentCategoryItem[]>();
  for (const category of categories) {
    const parent =
      category.parentId !== null && ids.has(category.parentId) ? category.parentId : null;
    children.set(parent, [...(children.get(parent) ?? []), category]);
  }

  const rows: CategoryRow[] = [];
  const visit = (parent: string | null, depth: number) => {
    for (const category of children.get(parent) ?? []) {
      rows.push({ category, depth });
      visit(category.id, depth + 1);
    }
  };
  visit(null, 0);
  return rows;
}

/** The categories a category may move under: not itself and nothing below it (BR-EQP-002). */
export function parentChoices(
  categories: readonly EquipmentCategoryItem[],
  movedId: string | undefined,
): EquipmentCategoryItem[] {
  if (movedId === undefined) {
    return [...categories];
  }

  const below = new Set<string>([movedId]);
  let isGrowing = true;
  while (isGrowing) {
    isGrowing = false;
    for (const category of categories) {
      if (category.parentId !== null && below.has(category.parentId) && !below.has(category.id)) {
        below.add(category.id);
        isGrowing = true;
      }
    }
  }

  return categories.filter((category) => !below.has(category.id));
}

/** The path as people read it: Ses › Mikrofon › Dinamik vokal. */
export function pathLabel(category: EquipmentCategoryItem): string {
  return category.path.join(" › ");
}
