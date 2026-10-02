import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { CategoryStatusFilter } from "@/api/model";
import { CategoriesPage } from "@/modules/catalog";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  status: z.enum(CategoryStatusFilter).optional().catch(undefined),
});

export const Route = createFileRoute("/_app/catalog/categories")({
  validateSearch: searchSchema,
  component: function CategoriesRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <CategoriesPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
      />
    );
  },
});
