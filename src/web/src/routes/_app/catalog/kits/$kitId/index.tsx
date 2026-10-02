import { createFileRoute } from "@tanstack/react-router";

import { KitDetailPage } from "@/modules/catalog";

export const Route = createFileRoute("/_app/catalog/kits/$kitId/")({
  component: function KitRoute() {
    const { kitId } = Route.useParams();
    return <KitDetailPage kitId={kitId} tab="general" />;
  },
});
