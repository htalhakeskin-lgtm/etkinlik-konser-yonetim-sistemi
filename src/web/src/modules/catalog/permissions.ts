/** The permissions Catalog's screens ask for (catalog §6). */
export const catalogPermissions = {
  viewCategories: "Catalog.Categories.View",
  createCategories: "Catalog.Categories.Create",
  editCategories: "Catalog.Categories.Edit",
  deactivateCategories: "Catalog.Categories.Deactivate",
  viewModels: "Catalog.Models.View",
  createModels: "Catalog.Models.Create",
  editModels: "Catalog.Models.Edit",
  deactivateModels: "Catalog.Models.Deactivate",
  viewKits: "Catalog.Kits.View",
  createKits: "Catalog.Kits.Create",
  editKits: "Catalog.Kits.Edit",
  deactivateKits: "Catalog.Kits.Deactivate",
} as const;
