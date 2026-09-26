import { fileURLToPath, URL } from "node:url";

import babel from "@rolldown/plugin-babel";
import tailwindcss from "@tailwindcss/vite";
import { tanstackRouter } from "@tanstack/router-plugin/vite";
import react, { reactCompilerPreset } from "@vitejs/plugin-react";
import type { Plugin } from "vite";
import { defineConfig } from "vitest/config";

// In development the API runs in the Host. Aspire passes its address; without Aspire the Host's
// launch profile port is used (docs/08-architecture.md §11).
const apiTarget = process.env.HOST_HTTP ?? "http://localhost:5080";

// Preloads the two Inter files so text renders in the right font on first load
// (docs/standards/ui.md §3.4). File names are hashed, so they are read from the build output.
function preloadInterFonts(): Plugin {
  return {
    name: "festos:preload-inter-fonts",
    apply: "build",
    transformIndexHtml: {
      order: "post",
      handler(_html, context) {
        return Object.keys(context.bundle ?? {})
          .filter((fileName) => /inter-latin(-ext)?-wght-normal-.*\.woff2$/.test(fileName))
          .map((fileName) => ({
            tag: "link",
            attrs: {
              rel: "preload",
              href: `/${fileName}`,
              as: "font",
              type: "font/woff2",
              crossorigin: "",
            },
            injectTo: "head",
          }));
      },
    },
  };
}

export default defineConfig({
  plugins: [
    // The router plugin must run before the React plugin (it generates src/routeTree.gen.ts).
    tanstackRouter({ target: "react", autoCodeSplitting: true }),
    react(),
    // React Compiler (docs/standards/code-style.md §5.4).
    babel({ presets: [reactCompilerPreset()] }),
    tailwindcss(),
    preloadInterFonts(),
  ],
  resolve: {
    alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) },
  },
  server: {
    proxy: {
      "/api": apiTarget,
      "/hubs": { target: apiTarget, ws: true },
    },
  },
  test: {
    environment: "jsdom",
    include: ["src/**/*.test.{ts,tsx}"],
    setupFiles: ["./src/test/setup.ts"],
    restoreMocks: true,
    // Vitest empties CSS imports by default; the token contrast test reads tokens.css as text.
    css: { include: [/styles\/tokens\.css/] },
  },
});
