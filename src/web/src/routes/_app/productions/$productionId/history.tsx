import { createFileRoute } from "@tanstack/react-router";

import { ProductionDetailPage } from "@/modules/riders";

export const Route = createFileRoute("/_app/productions/$productionId/history")({
  component: function ProductionRoute() {
    const { productionId } = Route.useParams();
    return <ProductionDetailPage productionId={productionId} tab="history" />;
  },
});
