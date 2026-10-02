import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { AuditPage } from "@/modules/audit";

const day = z.string().regex(/^\d{4}-\d{2}-\d{2}$/u);

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  actorId: z.uuid().optional().catch(undefined),
  from: day.optional().catch(undefined),
  to: day.optional().catch(undefined),
  entityType: z.string().max(200).optional().catch(undefined),
});

export const Route = createFileRoute("/_app/audit")({
  validateSearch: searchSchema,
  component: function AuditRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <AuditPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
      />
    );
  },
});
