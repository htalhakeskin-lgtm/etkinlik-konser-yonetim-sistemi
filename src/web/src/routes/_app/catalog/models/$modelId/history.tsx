import { createFileRoute } from "@tanstack/react-router";

import { ModelDetailPage } from "@/modules/catalog";

export const Route = createFileRoute("/_app/catalog/models/$modelId/history")({
  component: function ModelRoute() {
    const { modelId } = Route.useParams();
    return <ModelDetailPage modelId={modelId} tab="history" />;
  },
});
