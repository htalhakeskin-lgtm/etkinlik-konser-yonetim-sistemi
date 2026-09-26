import type { defaultNS, resources } from "./lib/i18n";

// Binds translation keys to types: a misspelled key fails the build (docs/standards/naming.md §7.1).
declare module "i18next" {
  // eslint-disable-next-line @typescript-eslint/consistent-type-definitions -- module augmentation merges into an interface
  interface CustomTypeOptions {
    defaultNS: typeof defaultNS;
    resources: (typeof resources)["tr"];
  }
}
