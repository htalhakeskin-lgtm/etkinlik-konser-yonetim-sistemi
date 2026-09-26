// Light / dark / device theme (docs/standards/ui.md §3.8). The choice is stored per device.
// public/theme-init.js applies it before the first paint; keep the two in sync.

export type ThemePreference = "light" | "dark" | "system";

const STORAGE_KEY = "festos.theme";
const DARK_QUERY = "(prefers-color-scheme: dark)";

export function readThemePreference(): ThemePreference {
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    return stored === "light" || stored === "dark" ? stored : "system";
  } catch {
    return "system";
  }
}

export function applyThemePreference(preference: ThemePreference): void {
  const isDark =
    preference === "dark" || (preference === "system" && window.matchMedia(DARK_QUERY).matches);
  document.documentElement.classList.toggle("dark", isDark);
}

export function saveThemePreference(preference: ThemePreference): void {
  try {
    if (preference === "system") {
      window.localStorage.removeItem(STORAGE_KEY);
    } else {
      window.localStorage.setItem(STORAGE_KEY, preference);
    }
  } catch {
    // Storage can be unavailable (private browsing); the choice then lasts for this page only.
  }
  applyThemePreference(preference);
}

// Follows the device setting while the preference is "system". Returns a function that stops it.
export function followSystemTheme(): () => void {
  const query = window.matchMedia(DARK_QUERY);
  const handleChange = () => {
    if (readThemePreference() === "system") {
      applyThemePreference("system");
    }
  };
  query.addEventListener("change", handleChange);
  return () => {
    query.removeEventListener("change", handleChange);
  };
}
