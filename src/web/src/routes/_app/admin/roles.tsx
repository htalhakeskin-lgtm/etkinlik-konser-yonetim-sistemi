import { createFileRoute } from "@tanstack/react-router";

import { RolesPage } from "@/modules/identity";

export const Route = createFileRoute("/_app/admin/roles")({
  component: RolesPage,
});
