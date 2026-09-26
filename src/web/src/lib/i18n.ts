import { createInstance } from "i18next";
import { initReactI18next } from "react-i18next";

import common from "@/locales/tr/common.json";

export const defaultNS = "common";

export const resources = {
  tr: { common },
} as const;

// S1 ships Turkish only; the setup keeps adding languages possible (docs/standards/naming.md §7.1).
export const i18n = createInstance({
  lng: "tr",
  fallbackLng: "tr",
  defaultNS,
  ns: [defaultNS],
  resources,
  // Resources are bundled, so initialization completes synchronously.
  initAsync: false,
  // React already escapes rendered values.
  interpolation: { escapeValue: false },
});

void i18n.use(initReactI18next).init();
