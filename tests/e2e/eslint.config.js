// ESLint for the end-to-end tests (docs/standards/code-style.md §5.3, testing.md §8).
import js from "@eslint/js";
import playwright from "eslint-plugin-playwright";
import { defineConfig, globalIgnores } from "eslint/config";
import globals from "globals";
import tseslint from "typescript-eslint";

export default defineConfig([
  globalIgnores(["playwright-report", "test-results", "blob-report"]),

  {
    linterOptions: { reportUnusedDisableDirectives: "error" },
  },

  {
    files: ["**/*.ts"],
    extends: [
      js.configs.recommended,
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
    ],
    languageOptions: {
      globals: globals.node,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      "@typescript-eslint/consistent-type-definitions": ["error", "type"],
      "@typescript-eslint/consistent-type-imports": "error",
      eqeqeq: ["error", "always"],
    },
  },

  {
    // Playwright rules, including the ban on fixed waits (testing.md §8).
    files: ["specs/**/*.ts", "support/**/*.ts"],
    extends: [playwright.configs["flat/recommended"]],
  },

  {
    files: ["**/*.js"],
    extends: [js.configs.recommended, tseslint.configs.disableTypeChecked],
    languageOptions: { globals: globals.node },
  },
]);
