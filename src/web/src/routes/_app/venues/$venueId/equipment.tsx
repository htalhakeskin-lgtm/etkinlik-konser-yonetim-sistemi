import { createFileRoute } from "@tanstack/react-router";

import { VenueDetailPage } from "@/modules/venues";

export const Route = createFileRoute("/_app/venues/$venueId/equipment")({
  component: function VenueRoute() {
    const { venueId } = Route.useParams();
    return <VenueDetailPage venueId={venueId} tab="equipment" />;
  },
});
