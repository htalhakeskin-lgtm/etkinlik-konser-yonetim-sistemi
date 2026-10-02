import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { KitStatusFilter } from "@/api/model";
import { KitsPage } from "@/modules/catalog";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  status: z.enum(KitStatusFilter).optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
  pageSize: z
    .union([z.literal(25), z.literal(50), z.literal(100)])
    .optional()
    .catch(undefined),
});

export const Route = createFileRoute("/_app/catalog/kits/")({
  validateSearch: searchSchema,
  component: function KitsRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <KitsPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
        onCreated={(kit) => {
          void navigate({ to: "/catalog/kits/$kitId", params: { kitId: kit.id } });
        }}
      />
    );
  },
});
