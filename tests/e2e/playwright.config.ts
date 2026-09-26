import { defineConfig, devices } from "@playwright/test";

// End-to-end tests (docs/standards/testing.md §8). CI points E2E_BASE_URL at the production-like
// stack; without it the built front end is served locally (there is no API yet).
const externalBaseUrl = process.env.E2E_BASE_URL;
const baseURL = externalBaseUrl ?? "http://localhost:4173";
const isCi = process.env.CI !== undefined;

export default defineConfig({
  testDir: "./specs",
  fullyParallel: true,
  forbidOnly: isCi,
  retries: isCi ? 1 : 0,
  reporter: isCi ? [["list"], ["html", { open: "never" }]] : [["list"]],
  use: {
    baseURL,
    // The interface must show Europe/Istanbul times whatever the browser's time zone, so the
    // browser runs in UTC on purpose (testing.md §8).
    locale: "tr-TR",
    timezoneId: "UTC",
    trace: "on-first-retry",
    screenshot: "only-on-failure",
  },
  // Pull requests run chromium and mobile-chromium; the nightly run adds Firefox and WebKit.
  projects: [
    { name: "chromium", use: { ...devices["Desktop Chrome"] } },
    // Warehouse screens are phone screens (ui.md §14); tests tagged @mobile run here.
    { name: "mobile-chromium", use: { ...devices["Pixel 7"] }, grep: /@mobile/ },
    { name: "firefox", use: { ...devices["Desktop Firefox"] } },
    { name: "webkit", use: { ...devices["Desktop Safari"] } },
  ],
  ...(externalBaseUrl === undefined && {
    webServer: {
      command:
        "pnpm --filter @festos/web run build && pnpm --filter @festos/web run preview --port 4173 --strictPort",
      url: baseURL,
      reuseExistingServer: !isCi,
      timeout: 120_000,
    },
  }),
});
