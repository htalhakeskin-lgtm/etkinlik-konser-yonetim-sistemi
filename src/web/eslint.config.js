// ESLint configuration (docs/standards/code-style.md §5.3). No formatting rules: Prettier owns formatting.
import comments from "@eslint-community/eslint-plugin-eslint-comments/configs";
import js from "@eslint/js";
import pluginQuery from "@tanstack/eslint-plugin-query";
import pluginRouter from "@tanstack/eslint-plugin-router";
import boundaries from "eslint-plugin-boundaries";
import jsxA11y from "eslint-plugin-jsx-a11y";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import simpleImportSort from "eslint-plugin-simple-import-sort";
import { defineConfig, globalIgnores } from "eslint/config";
import globals from "globals";
import tseslint from "typescript-eslint";

export default defineConfig([
  // Generated or copied code is not linted (docs/08-architecture.md §11).
  globalIgnores([
    "dist",
    "coverage",
    "storybook-static",
    "src/api",
    "src/components/ui",
    "src/routeTree.gen.ts",
  ]),

  {
    linterOptions: { reportUnusedDisableDirectives: "error" },
  },

  {
    files: ["**/*.{ts,tsx}"],
    extends: [
      js.configs.recommended,
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
      jsxA11y.flatConfigs.recommended,
      pluginQuery.configs["flat/recommended"],
      pluginRouter.configs["flat/recommended"],
      comments.recommended,
    ],
    languageOptions: {
      globals: globals.browser,
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    plugins: {
      boundaries,
      "simple-import-sort": simpleImportSort,
    },
    settings: {
      "import/resolver": { typescript: { project: "./tsconfig.app.json" } },
      "boundaries/elements": [{ type: "module", pattern: "src/modules/*", capture: ["name"] }],
    },
    rules: {
      // A module imports another module only through its index.ts (docs/08-architecture.md §11).
      "boundaries/dependencies": [
        "error",
        {
          default: "allow",
          policies: [
            {
              from: { element: { type: "module" } },
              disallow: { to: { element: { type: "module" } } },
            },
            {
              from: { element: { type: "module" } },
              allow: {
                to: {
                  element: {
                    type: "module",
                    captured: { name: "{{ from.element.captured.name }}" },
                  },
                },
              },
            },
            {
              from: { element: { type: "module" } },
              allow: { to: { element: { type: "module", fileInternalPath: "index.ts" } } },
            },
          ],
        },
      ],

      // Route files export `Route`, which Vite's fast refresh handles.
      "react-refresh/only-export-components": ["error", { allowExportNames: ["Route"] }],

      "simple-import-sort/imports": "error",
      "simple-import-sort/exports": "error",

      "@eslint-community/eslint-comments/require-description": "error",
      "@eslint-community/eslint-comments/no-unused-disable": "error",

      // Naming (docs/standards/naming.md §7).
      "@typescript-eslint/naming-convention": [
        "error",
        { selector: "default", format: ["camelCase"] },
        { selector: "import", format: ["camelCase", "PascalCase"] },
        { selector: "variable", format: ["camelCase", "PascalCase", "UPPER_CASE"] },
        {
          selector: "variable",
          types: ["boolean"],
          format: ["PascalCase"],
          prefix: ["is", "has", "can", "should"],
        },
        { selector: "function", format: ["camelCase", "PascalCase"] },
        { selector: "parameter", format: ["camelCase"], leadingUnderscore: "allow" },
        { selector: "typeLike", format: ["PascalCase"] },
        // Object keys often mirror external shapes (HTTP headers, JSON, proxy paths).
        { selector: ["objectLiteralProperty", "typeProperty"], format: null },
      ],
      "id-match": [
        "error",
        "^[A-Za-z_$][A-Za-z0-9_$]*$",
        { properties: true, onlyDeclarations: true },
      ],
      "no-restricted-exports": [
        "error",
        {
          restrictDefaultExports: {
            direct: true,
            named: true,
            defaultFrom: true,
            namedFrom: true,
            namespaceFrom: true,
          },
        },
      ],
      "@typescript-eslint/consistent-type-definitions": ["error", "type"],
      "@typescript-eslint/consistent-type-imports": "error",
      "no-restricted-syntax": [
        "error",
        {
          selector: "CallExpression[callee.property.name='toLocaleLowerCase'][arguments.length=0]",
          message: "Pass the locale: toLocaleLowerCase('tr-TR') (code-style §5.5).",
        },
        {
          selector: "CallExpression[callee.property.name='toLocaleUpperCase'][arguments.length=0]",
          message: "Pass the locale: toLocaleUpperCase('tr-TR') (code-style §5.5).",
        },
        {
          selector: "CallExpression[callee.property.name='localeCompare'][arguments.length<2]",
          message: "Pass the locale: localeCompare(other, 'tr') (code-style §5.5).",
        },
        {
          selector: "JSXAttribute[name.name='dangerouslySetInnerHTML']",
          message: "Rendering raw HTML is not allowed (security §9).",
        },
      ],
      "no-restricted-globals": [
        "error",
        {
          name: "fetch",
          message: "Call the server through the generated API client (code-style §5.3).",
        },
      ],
      "no-console": ["error", { allow: ["warn", "error"] }],
      eqeqeq: ["error", "always"],
    },
  },

  {
    // Tools require a default export from their configuration files.
    files: ["vite.config.ts", "eslint.config.js"],
    rules: { "no-restricted-exports": "off" },
  },

  {
    // Classic browser scripts loaded directly by index.html.
    files: ["public/**/*.js"],
    extends: [js.configs.recommended],
    languageOptions: { globals: globals.browser, sourceType: "script" },
  },

  {
    files: ["**/*.js"],
    ignores: ["public/**"],
    extends: [js.configs.recommended, tseslint.configs.disableTypeChecked],
    languageOptions: { globals: globals.node },
  },
]);
