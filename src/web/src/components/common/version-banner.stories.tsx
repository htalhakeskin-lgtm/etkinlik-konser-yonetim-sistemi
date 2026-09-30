import type { Meta, StoryObj } from "@storybook/react-vite";
import { fn } from "storybook/test";

import { VersionBannerContent } from "./version-banner";

const meta = {
  title: "Ortak/Sürüm şeridi",
  component: VersionBannerContent,
  args: { onReload: fn() },
} satisfies Meta<typeof VersionBannerContent>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};
