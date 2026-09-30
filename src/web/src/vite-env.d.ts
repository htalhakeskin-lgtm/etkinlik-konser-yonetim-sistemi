/// <reference types="vite/client" />

// Build-time values the front end reads (docs/standards/api.md §12).
// eslint-disable-next-line @typescript-eslint/consistent-type-definitions -- module augmentation merges into an interface
interface ImportMetaEnv {
  /** The release this build belongs to; set by the release build, absent in development. */
  readonly VITE_APP_VERSION?: string;
}
