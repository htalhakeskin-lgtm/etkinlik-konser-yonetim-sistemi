import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { onSessionExpired } from "@/lib/session-expiry";

import { meQuery } from "../session";
import { LoginForm } from "./login-form";

// The session ended while the user worked (ui §7.7): they sign in again over the screen, which keeps
// what they entered, and send it again. Another user starts over with a fresh page.
export function ReauthDialog() {
  const { t } = useTranslation("identity");
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [isOpen, setIsOpen] = useState(false);
  const user = queryClient.getQueryData(meQuery.queryKey);

  useEffect(
    () =>
      onSessionExpired(() => {
        // Without a signed-in user the route guard sends the visitor to the sign-in screen instead.
        if (queryClient.getQueryData(meQuery.queryKey) !== undefined) {
          setIsOpen(true);
        }
      }),
    [queryClient],
  );

  return (
    <Dialog open={isOpen} disablePointerDismissal>
      <DialogContent showCloseButton={false}>
        <DialogHeader>
          <DialogTitle>{t("reauth.title")}</DialogTitle>
          <DialogDescription>{t("reauth.description")}</DialogDescription>
        </DialogHeader>
        <LoginForm
          defaultEmail={user?.email}
          onSignedIn={(signedIn) => {
            if (signedIn.id !== user?.id) {
              window.location.assign("/");
              return;
            }

            queryClient.setQueryData(meQuery.queryKey, signedIn);
            setIsOpen(false);
            if (signedIn.mustChangePassword) {
              void navigate({ to: "/set-password" });
              return;
            }

            void queryClient.invalidateQueries({
              predicate: (query) => query.queryKey !== meQuery.queryKey,
            });
          }}
        />
      </DialogContent>
    </Dialog>
  );
}
