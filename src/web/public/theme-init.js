// Applies the saved theme before the first paint, so the wrong theme never flashes
// (docs/standards/ui.md §3.8). A separate file because the Content Security Policy does not allow
// inline scripts. Keep in sync with src/lib/theme.ts.
(function () {
  let preference = "system";
  try {
    const stored = window.localStorage.getItem("festos.theme");
    if (stored === "light" || stored === "dark") {
      preference = stored;
    }
  } catch {
    // Storage can be unavailable (private browsing); the device setting is used.
  }
  const isDark =
    preference === "dark" ||
    (preference === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches);
  document.documentElement.classList.toggle("dark", isDark);
})();
