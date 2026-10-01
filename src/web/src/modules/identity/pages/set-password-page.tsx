import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

import { logout } from "@/api/endpoints/identity/identity";
import { Button } from "@/components/ui/button";

import { SetPasswordForm } from "../components/set-password-form";
import { meQuery } from "../session";

// The screen between signing in with a temporary password and everything else (BR-SYS-006).
export function SetPasswordPage() {
  const { t } = useTranslation("identity");
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
    <main className="flex min-h-screen items-center justify-center bg-background p-4 text-foreground">
      <div className="flex w-full max-w-sm flex-col gap-6 rounded-xl border bg-card p-6 shadow-sm">
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-semibold">{t("setPassword.title")}</h1>
          <p className="text-sm text-muted-foreground">{t("setPassword.description")}</p>
        </div>
        <SetPasswordForm
          onChanged={(user) => {
            queryClient.setQueryData(meQuery.queryKey, user);
            void navigate({ to: "/", replace: true });
          }}
        />
        <Button
          variant="ghost"
          isLoading={signOut.isPending}
          onClick={() => {
            signOut.mutate();
          }}
        >
          {t("signOut")}
        </Button>
      </div>
    </main>
  );
}
