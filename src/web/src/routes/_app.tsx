import { createFileRoute } from "@tanstack/react-router";

import { AppLayout } from "@/app/app-layout";
import { requireSession } from "@/app/session-guard";

export const Route = createFileRoute("/_app")({
  beforeLoad: async ({ context, location }) => ({
    user: await requireSession(context.queryClient, location.href),
  }),
  component: AppLayout,
});
