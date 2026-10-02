import { createFileRoute } from "@tanstack/react-router";

import { PartyDetailPage } from "@/modules/parties";

export const Route = createFileRoute("/_app/parties/$partyId/history")({
  component: function PartyRoute() {
    const { partyId } = Route.useParams();
    return <PartyDetailPage partyId={partyId} tab="history" />;
  },
});
