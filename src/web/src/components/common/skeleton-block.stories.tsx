import type { Meta, StoryObj } from "@storybook/react-vite";

import { SkeletonBlock } from "./skeleton-block";

const meta = {
  title: "Ortak/İskelet",
  component: SkeletonBlock,
  args: { className: "h-4 w-64" },
} satisfies Meta<typeof SkeletonBlock>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Line: Story = {};

// The shape of a detail card while it loads (ui §10.1).
export const Card: Story = {
  render: () => (
    <div aria-busy="true" className="flex w-80 flex-col gap-3 rounded-lg border p-4">
      <SkeletonBlock className="h-6 w-40" />
      <SkeletonBlock className="w-full" />
      <SkeletonBlock className="w-3/4" />
    </div>
  ),
};
