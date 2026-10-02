import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { VenueStatusFilter } from "@/api/model";
import { VenuesPage } from "@/modules/venues";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  city: z.string().optional().catch(undefined),
  status: z.enum(VenueStatusFilter).optional().catch(undefined),
  sort: z.string().optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
  pageSize: z
    .union([z.literal(25), z.literal(50), z.literal(100)])
    .optional()
    .catch(undefined),
});

export const Route = createFileRoute("/_app/venues/")({
  validateSearch: searchSchema,
  component: function VenuesRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <VenuesPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
      />
    );
  },
});
