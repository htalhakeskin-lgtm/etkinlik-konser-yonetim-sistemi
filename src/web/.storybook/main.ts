import type { StorybookConfig } from "@storybook/react-vite";

// Design system catalogue (docs/standards/ui.md §18): the shared components and the design tokens.
const config: StorybookConfig = {
  stories: ["../src/components/**/*.stories.tsx", "../src/styles/**/*.stories.tsx"],
  addons: ["@storybook/addon-vitest", "@storybook/addon-a11y", "@storybook/addon-docs"],
  framework: "@storybook/react-vite",
  core: { disableTelemetry: true },
};

export default config;
