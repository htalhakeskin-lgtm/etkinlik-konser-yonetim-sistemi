import { createFileRoute } from "@tanstack/react-router";

import { PartyDetailPage } from "@/modules/parties";
import { ArtistProductions } from "@/modules/riders";

export const Route = createFileRoute("/_app/artists/$partyId/")({
  component: function ArtistRoute() {
    const { partyId } = Route.useParams();
    return (
      <PartyDetailPage
        partyId={partyId}
        tab="general"
        variant="artists"
        artistSection={(artist) => (
          <ArtistProductions
            artistId={artist.id}
            artistName={artist.name}
            isArtistActive={artist.isActive}
          />
        )}
      />
    );
  },
});
