import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { ChevronDown } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

import { logout } from "@/api/endpoints/identity/identity";
import type { SignedInUserDetails } from "@/api/model";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { ChangePasswordDialog } from "./change-password-dialog";

export type UserMenuProps = {
  user: SignedInUserDetails;
};

function initials(fullName: string): string {
  return fullName
    .split(/\s+/u)
    .filter((part) => part.length > 0)
    .map((part) => part[0]?.toLocaleUpperCase("tr-TR"))
    .slice(0, 2)
    .join("");
}

// The top bar's user menu (11 §2.1): name and roles, changing the password, signing out. A general
// manager reads that the access is read-only (BR-SYS-004).
export function UserMenu({ user }: UserMenuProps) {
  const { t } = useTranslation("identity");
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const isReadOnly = user.roles.length > 0 && user.roles.every((role) => role === "generalManager");
  const signOut = useMutation({
    mutationFn: () => logout(),
    meta: { expectsUnauthorized: true },
    onSettled: () => {
      queryClient.clear();
      void navigate({ to: "/login", replace: true });
    },
  });

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button variant="ghost" aria-label={t("userMenu.open", { name: user.fullName })} />
          }
        >
          <span
            aria-hidden="true"
            className="flex size-7 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground"
          >
            {initials(user.fullName)}
          </span>
          <ChevronDown aria-hidden="true" />
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="min-w-56">
          <DropdownMenuGroup>
            <DropdownMenuLabel className="flex flex-col gap-0.5">
              <span className="font-medium text-foreground">{user.fullName}</span>
              <span className="text-xs font-normal">
                {user.roles.map((role) => t(`roles.${role}`)).join(", ")}
              </span>
              {isReadOnly && <span className="text-xs font-normal">{t("userMenu.readOnly")}</span>}
            </DropdownMenuLabel>
          </DropdownMenuGroup>
          <DropdownMenuSeparator />
          <DropdownMenuItem
            onClick={() => {
              setIsChangingPassword(true);
            }}
          >
            {t("userMenu.changePassword")}
          </DropdownMenuItem>
          <DropdownMenuItem
            onClick={() => {
              signOut.mutate();
            }}
          >
            {t("signOut")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <ChangePasswordDialog open={isChangingPassword} onOpenChange={setIsChangingPassword} />
    </>
  );
}
