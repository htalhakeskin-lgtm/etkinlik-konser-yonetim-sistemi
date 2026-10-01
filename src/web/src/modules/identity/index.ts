// The identity module's entry point for the shell and the other modules (08 §11).
export { ReauthDialog } from "./components/reauth-dialog";
export { UserMenu } from "./components/user-menu";
export { LoginPage } from "./pages/login-page";
export { RolesPage } from "./pages/roles-page";
export { SetPasswordPage } from "./pages/set-password-page";
export { UsersPage, type UsersSearch } from "./pages/users-page";
export { hasPermission, identityPermissions, useSignedInUser } from "./permissions";
export { meQuery, safeRedirect } from "./session";
