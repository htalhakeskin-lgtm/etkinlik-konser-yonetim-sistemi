import { createInstance } from "i18next";
import { initReactI18next } from "react-i18next";

import audit from "@/locales/tr/audit.json";
import common from "@/locales/tr/common.json";
import errors from "@/locales/tr/errors.json";
import identity from "@/locales/tr/identity.json";
import inventory from "@/locales/tr/inventory.json";
import parties from "@/locales/tr/parties.json";
import validation from "@/locales/tr/validation.json";

export const defaultNS = "common";

export const resources = {
  tr: { common, errors, validation, identity, inventory, audit, parties },
} as const;

// S1 ships Turkish only; the setup keeps adding languages possible (docs/standards/naming.md §7.1).
export const i18n = createInstance({
  lng: "tr",
  fallbackLng: "tr",
  defaultNS,
  ns: [defaultNS, "errors", "validation", "identity", "inventory", "audit", "parties"],
  resources,
  // Resources are bundled, so initialization completes synchronously.
  initAsync: false,
  // React already escapes rendered values.
  interpolation: { escapeValue: false },
});

void i18n.use(initReactI18next).init();
