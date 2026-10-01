import { createFileRoute, redirect } from "@tanstack/react-router";

import { HomePage } from "@/app/home-page";
import { startPage } from "@/app/start-page";

export const Route = createFileRoute("/_app/")({
  // `/` sends each role to its own start screen (11 §4).
  beforeLoad: ({ context }) => {
    const target = startPage(context.user.roles);
    if (target !== undefined) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error -- TanStack Router redirects by throwing
      throw redirect({ to: target, replace: true });
    }
  },
  component: HomePage,
});
