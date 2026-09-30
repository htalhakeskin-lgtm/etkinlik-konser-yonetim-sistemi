import type { Meta, StoryObj } from "@storybook/react-vite";

import { ConnectionIndicatorContent } from "./connection-indicator";

const meta = {
  title: "Ortak/Bağlantı göstergesi",
  component: ConnectionIndicatorContent,
  args: { state: "disconnected" },
} satisfies Meta<typeof ConnectionIndicatorContent>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Offline: Story = {};

export const Reconnecting: Story = {
  args: { state: "reconnecting", warningDelayMs: 0 },
};
