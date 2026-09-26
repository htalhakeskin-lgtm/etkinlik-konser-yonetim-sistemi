import "../src/styles/app.css";
import "../src/lib/i18n";

import type { Decorator, Preview } from "@storybook/react-vite";

// Shows every story in the light or the dark theme, chosen in the toolbar (docs/standards/ui.md §3.8).
const withTheme: Decorator = (Story, context) => {
  document.documentElement.classList.toggle("dark", context.globals.theme === "dark");
  return <Story />;
};

const preview: Preview = {
  decorators: [withTheme],
  globalTypes: {
    theme: {
      description: "Tema",
      toolbar: {
        title: "Tema",
        icon: "mirror",
        items: [
          { value: "light", title: "Açık" },
          { value: "dark", title: "Koyu" },
        ],
        dynamicTitle: true,
      },
    },
  },
  initialGlobals: { theme: "light" },
  parameters: {
    layout: "padded",
    controls: { matchers: { color: /(background|color)$/i, date: /Date$/i } },
    // Accessibility violations fail the story tests (docs/standards/ui.md §18).
    a11y: { test: "error" },
  },
};

export default preview;
