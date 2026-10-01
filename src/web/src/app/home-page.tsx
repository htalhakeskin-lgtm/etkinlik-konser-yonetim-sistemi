import { useMutation, useQueryClient, useSuspenseQuery } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

import { logout } from "@/api/endpoints/identity/identity";
import { Button } from "@/components/ui/button";
import { meQuery } from "@/modules/identity";

// A placeholder start screen until the shell and the role start screens arrive (11 §4).
export function HomePage() {
  const { t } = useTranslation();
  const { t: tIdentity } = useTranslation("identity");
  const { data: user } = useSuspenseQuery(meQuery);
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const signOut = useMutation({
    mutationFn: () => logout(),
    meta: { expectsUnauthorized: true },
    onSettled: () => {
      queryClient.clear();
      void navigate({ to: "/login", replace: true });
    },
  });

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-2 bg-background p-4 text-foreground">
      <h1 className="text-3xl font-semibold text-primary">{t("app.name")}</h1>
      <p>{t("home.welcome", { name: user.fullName })}</p>
      <p className="text-muted-foreground">{t("home.placeholder")}</p>
      <Button
        variant="outline"
        isLoading={signOut.isPending}
        onClick={() => {
          signOut.mutate();
        }}
      >
        {tIdentity("signOut")}
      </Button>
    </main>
  );
}
