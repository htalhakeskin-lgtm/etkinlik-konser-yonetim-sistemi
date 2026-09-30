import type { Meta, StoryObj } from "@storybook/react-vite";
import { fn } from "storybook/test";

import { ApiError } from "@/lib/api-error";

import { ErrorState } from "./error-state";

const meta = {
  title: "Ortak/Hata durumu",
  component: ErrorState,
  args: {
    error: new ApiError({
      status: 500,
      code: "internalError",
      traceId: "0af7651916cd43dd8448eb211c80319c",
    }),
    onRetry: fn(),
  },
} satisfies Meta<typeof ErrorState>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Unexpected: Story = {};

export const NoConnection: Story = {
  args: { error: ApiError.network(new TypeError("Failed to fetch")) },
};
