import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { Role, UserStatusFilter } from "@/api/model";
import { UsersPage } from "@/modules/identity";

// A value the screen cannot use is dropped rather than failing the page (ui §5.1).
const searchSchema = z.object({
  q: z.string().optional().catch(undefined),
  role: z.enum(Role).optional().catch(undefined),
  status: z.enum(UserStatusFilter).optional().catch(undefined),
  sort: z.string().optional().catch(undefined),
  page: z.number().int().min(1).optional().catch(undefined),
  pageSize: z
    .union([z.literal(25), z.literal(50), z.literal(100)])
    .optional()
    .catch(undefined),
});

export const Route = createFileRoute("/_app/admin/users")({
  validateSearch: searchSchema,
  component: function UsersRoute() {
    const search = Route.useSearch();
    const navigate = Route.useNavigate();
    return (
      <UsersPage
        search={search}
        onSearchChange={(next) => {
          void navigate({ search: next, replace: true });
        }}
      />
    );
  },
});
