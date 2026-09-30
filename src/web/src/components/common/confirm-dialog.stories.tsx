import type { Meta, StoryObj } from "@storybook/react-vite";
import { fn } from "storybook/test";

import { ConfirmDialog } from "./confirm-dialog";

const meta = {
  title: "Ortak/Onay diyaloğu",
  component: ConfirmDialog,
  args: {
    open: true,
    onOpenChange: fn(),
    onConfirm: fn(),
    title: "Yaz Festivali 2027 iptal edilsin mi?",
    description: "Onaylı 42 rezervasyon serbest bırakılacak; bu geri alınamaz.",
    confirmLabel: "Etkinliği iptal et",
  },
} satisfies Meta<typeof ConfirmDialog>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};

export const WithReason: Story = {
  args: { reason: { label: "İptal nedeni" } },
};
