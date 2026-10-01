import "@testing-library/jest-dom/vitest";
import "@/lib/i18n";

import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

import { applyServerValidationTexts } from "@/lib/schema-messages";

applyServerValidationTexts();

afterEach(() => {
  cleanup();
});
