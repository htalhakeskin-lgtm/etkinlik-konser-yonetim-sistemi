import { Ellipsis } from "lucide-react";
import { useTranslation } from "react-i18next";

import type { UserListItem } from "@/api/model";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { hasPermission, identityPermissions, useSignedInUser } from "../permissions";

export type UserAction = "edit" | "resetPassword" | "deactivate" | "activate";

export type UserRowActionsProps = {
  user: UserListItem;
  onAction: (action: UserAction, user: UserListItem) => void;
};

// The "⋯" menu at the end of a row (ui §8): only the actions the signed-in user may take (ui §12).
export function UserRowActions({ user, onAction }: UserRowActionsProps) {
  const { t } = useTranslation("identity");
  const me = useSignedInUser();
  const canEdit = hasPermission(me, identityPermissions.editUsers);
  const canReset = hasPermission(me, identityPermissions.resetUserPasswords);
  const canDeactivate = hasPermission(me, identityPermissions.deactivateUsers);
  const isSelf = user.id === me.id;

  if (!canEdit && !canReset && !canDeactivate) {
    return null;
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button
            variant="ghost"
            size="icon-sm"
            aria-label={t("users.actions.open", { name: user.fullName })}
          />
        }
      >
        <Ellipsis aria-hidden="true" />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        {canEdit && (
          <DropdownMenuItem
            onClick={() => {
              onAction("edit", user);
            }}
          >
            {t("users.actions.edit")}
          </DropdownMenuItem>
        )}
        {canReset && user.isActive && (
          <DropdownMenuItem
            onClick={() => {
              onAction("resetPassword", user);
            }}
          >
            {t("users.actions.resetPassword")}
          </DropdownMenuItem>
        )}
        {canDeactivate &&
          (user.isActive ? (
            <DropdownMenuItem
              disabled={isSelf}
              onClick={() => {
                onAction("deactivate", user);
              }}
            >
              {isSelf ? t("users.actions.cannotDeactivateSelf") : t("users.actions.deactivate")}
            </DropdownMenuItem>
          ) : (
            <DropdownMenuItem
              onClick={() => {
                onAction("activate", user);
              }}
            >
              {t("users.actions.activate")}
            </DropdownMenuItem>
          ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
