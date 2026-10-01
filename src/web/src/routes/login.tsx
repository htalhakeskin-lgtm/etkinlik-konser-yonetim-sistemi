import { createFileRoute } from "@tanstack/react-router";
import { z } from "zod";

import { LoginPage } from "@/modules/identity";

export const Route = createFileRoute("/login")({
  validateSearch: z.object({ redirect: z.string().optional() }),
  component: function LoginRoute() {
    const { redirect } = Route.useSearch();
    return <LoginPage redirectTo={redirect} />;
  },
});
