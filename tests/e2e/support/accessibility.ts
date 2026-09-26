import AxeBuilder from "@axe-core/playwright";
import { expect, type Page } from "@playwright/test";

// Automated WCAG 2.2 AA scan of the current page (docs/standards/testing.md §9).
export async function expectNoAccessibilityViolations(page: Page): Promise<void> {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22aa"])
    .analyze();

  expect(results.violations).toEqual([]);
}
