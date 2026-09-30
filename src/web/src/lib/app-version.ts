// A newer release is detected from the X-App-Version header of API responses (api §12). The page is
// never reloaded for the user; the version banner asks them to reload when their work is done.
const buildVersion = import.meta.env.VITE_APP_VERSION;

let isNewVersionAvailable = false;
const listeners = new Set<() => void>();

/** Compares the server's version with this build's; development builds have none and never differ. */
export function reportServerVersion(serverVersion: string | null): void {
  if (
    isNewVersionAvailable ||
    buildVersion === undefined ||
    serverVersion === null ||
    serverVersion === buildVersion
  ) {
    return;
  }

  isNewVersionAvailable = true;
  for (const listener of listeners) {
    listener();
  }
}

/** Whether the server runs a newer release than this page. */
export function hasNewVersion(): boolean {
  return isNewVersionAvailable;
}

/** Calls the listener once a newer release is detected; returns the unsubscribe function. */
export function subscribeToNewVersion(listener: () => void): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}
