import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { useTranslation } from "react-i18next";

import { LoginForm } from "../components/login-form";
import { meQuery, safeRedirect } from "../session";

export type LoginPageProps = {
  redirectTo?: string | undefined;
};

// The sign-in screen (11 §2.8). With a temporary password the next screen sets a new one (BR-SYS-006).
export function LoginPage({ redirectTo }: LoginPageProps) {
  const { t } = useTranslation("identity");
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  return (
    <main className="flex min-h-screen items-center justify-center bg-background p-4 text-foreground">
      <div className="flex w-full max-w-sm flex-col gap-6 rounded-xl border bg-card p-6 shadow-sm">
        <h1 className="text-center text-2xl font-semibold text-primary">{t("signIn.title")}</h1>
        <LoginForm
          onSignedIn={(user) => {
            queryClient.setQueryData(meQuery.queryKey, user);
            void navigate({
              to: user.mustChangePassword ? "/set-password" : safeRedirect(redirectTo),
              replace: true,
            });
          }}
        />
      </div>
    </main>
  );
}
