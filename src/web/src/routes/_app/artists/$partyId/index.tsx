import { createFileRoute } from "@tanstack/react-router";

import { PartyDetailPage } from "@/modules/parties";

export const Route = createFileRoute("/_app/artists/$partyId/")({
  component: function ArtistRoute() {
    const { partyId } = Route.useParams();
    return <PartyDetailPage partyId={partyId} tab="general" variant="artists" />;
  },
});
