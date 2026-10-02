import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { ModelStatusFilter, TrackingType } from "@/api/model";
import { ModelsPage } from "@/modules/catalog";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  categoryId: z.uuid().optional().catch(undefined),
  trackingType: z.enum(TrackingType).optional().catch(undefined),
  status: z.enum(ModelStatusFilter).optional().catch(undefined),
  sort: z.string().optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
  pageSize: z
    .union([z.literal(25), z.literal(50), z.literal(100)])
    .optional()
    .catch(undefined),
});

export const Route = createFileRoute("/_app/catalog/models/")({
  validateSearch: searchSchema,
  component: function ModelsRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <ModelsPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
      />
    );
  },
});
