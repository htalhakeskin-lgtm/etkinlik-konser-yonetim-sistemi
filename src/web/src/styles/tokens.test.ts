import { describe, expect, it } from "vitest";

import { contrastRatio, parseOklch } from "@/test/color-contrast";

import tokensCss from "./tokens.css?raw";

// Every text / surface pair meets WCAG 2.2 AA in both themes (docs/standards/ui.md §3.2, §18).
const TEXT_PAIRS: [string, string][] = [
  ["foreground", "background"],
  ["card-foreground", "card"],
  ["popover-foreground", "popover"],
  ["primary-foreground", "primary"],
  ["primary", "background"],
  ["primary", "card"],
  ["secondary-foreground", "secondary"],
  ["muted-foreground", "background"],
  ["muted-foreground", "card"],
  ["muted-foreground", "muted"],
  ["accent-foreground", "accent"],
  ["destructive-foreground", "destructive"],
  ["destructive", "background"],
  ["destructive", "card"],
  ...["neutral", "info", "active", "success", "warning", "danger", "muted"].map(
    (tone): [string, string] => [`status-${tone}`, `status-${tone}-surface`],
  ),
];

// Outlines that identify a control need 3:1 (WCAG 1.4.11).
const CONTROL_PAIRS: [string, string][] = [
  ["input", "background"],
  ["input", "card"],
  ["input", "popover"],
  ["ring", "background"],
  ["ring", "card"],
];

function readDeclarations(selector: string): Map<string, string> {
  const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const block = new RegExp(`${escaped}\\s*\\{([^}]*)\\}`).exec(tokensCss)?.[1];
  if (block === undefined) {
    throw new Error(`Selector ${selector} not found in tokens.css`);
  }
  const declarations = new Map<string, string>();
  for (const match of block.matchAll(/--([\w-]+):\s*([^;]+);/g)) {
    const [, name = "", value = ""] = match;
    declarations.set(name, value.trim());
  }
  return declarations;
}

const lightTokens = readDeclarations(":root");
const themes = {
  light: lightTokens,
  // The dark block only overrides; palette values come from :root.
  dark: new Map([...lightTokens, ...readDeclarations(".dark")]),
};

function resolveColor(tokens: Map<string, string>, name: string): string {
  let value = tokens.get(name);
  for (let depth = 0; value?.startsWith("var(") === true && depth < 10; depth++) {
    value = tokens.get(value.slice("var(--".length, -")".length));
  }
  if (value === undefined) {
    throw new Error(`Token --${name} is not defined`);
  }
  return value;
}

describe.each(Object.entries(themes))("%s theme", (_theme, tokens) => {
  const measure = (foreground: string, background: string) =>
    contrastRatio(
      parseOklch(resolveColor(tokens, foreground)),
      parseOklch(resolveColor(tokens, background)),
    );

  it.each(TEXT_PAIRS)("%s on %s reaches 4.5:1", (foreground, background) => {
    expect(measure(foreground, background)).toBeGreaterThanOrEqual(4.5);
  });

  it.each(CONTROL_PAIRS)("%s against %s reaches 3:1", (outline, background) => {
    expect(measure(outline, background)).toBeGreaterThanOrEqual(3);
  });
});
