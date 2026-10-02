import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { PartyKind, PartyStatusFilter } from "@/api/model";
import { PartiesPage } from "@/modules/parties";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1). The role is the
// artist role, so it is not in the address.
const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  kind: z.enum(PartyKind).optional().catch(undefined),
  status: z.enum(PartyStatusFilter).optional().catch(undefined),
  sort: z.string().optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
  pageSize: z
    .union([z.literal(25), z.literal(50), z.literal(100)])
    .optional()
    .catch(undefined),
});

export const Route = createFileRoute("/_app/artists/")({
  validateSearch: searchSchema,
  component: function ArtistsRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <PartiesPage
        variant="artists"
        search={search}
        onSearchChange={(next) => {
          const { q, kind, status, sort, page, pageSize } = next;
          void navigate({ search: { q, kind, status, sort, page, pageSize }, replace: true });
        }}
      />
    );
  },
});
