// A request answered 401 after the user had signed in: the session ended (P-03, P-16). The query
// client reports it here and the shell opens the re-login dialog (ui §7.7); nothing else listens.

type Listener = () => void;

const listeners = new Set<Listener>();

/** Tells the listeners that the server no longer knows the session. */
export function reportSessionExpired(): void {
  for (const listener of listeners) {
    listener();
  }
}

/** Listens for an ended session; returns the function that stops listening. */
export function onSessionExpired(listener: Listener): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}
