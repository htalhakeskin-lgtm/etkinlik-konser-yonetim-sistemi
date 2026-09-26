import { expect, test } from "@playwright/test";

import { expectNoAccessibilityViolations } from "../support/accessibility";

// Keeps the pipeline honest until the demo scenario tests arrive: the built app loads, speaks
// Turkish, applies the saved theme before the first paint and passes the accessibility scan.
test.describe("application", () => {
  test("opens in Turkish with the product name", { tag: ["@smoke"] }, async ({ page }) => {
    await page.goto("/");

    await expect(page.locator("html")).toHaveAttribute("lang", "tr");
    await expect(page.getByRole("heading", { level: 1, name: "FestOS" })).toBeVisible();
    await expectNoAccessibilityViolations(page);
  });

  test(
    "applies the saved dark theme before the page renders",
    { tag: ["@smoke"] },
    async ({ page }) => {
      await page.addInitScript(() => {
        window.localStorage.setItem("festos.theme", "dark");
        // Records the theme once the HTML is parsed, before the application code has run.
        document.addEventListener("readystatechange", () => {
          if (document.readyState === "interactive") {
            document.documentElement.dataset.themeAtParse = String(
              document.documentElement.classList.contains("dark"),
            );
          }
        });
      });

      await page.goto("/");

      await expect(page.locator("html")).toHaveAttribute("data-theme-at-parse", "true");
      await expect(page.locator("html")).toHaveClass(/dark/);
      await expectNoAccessibilityViolations(page);
    },
  );
});
