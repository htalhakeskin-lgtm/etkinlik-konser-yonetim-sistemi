import type { Meta, StoryObj } from "@storybook/react-vite";
import { Plus } from "lucide-react";

import { Button } from "./button";

const meta = {
  title: "Temel/Düğme",
  component: Button,
  args: { children: "Kaydet" },
} satisfies Meta<typeof Button>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Primary: Story = {};

export const Variants: Story = {
  render: (args) => (
    <div className="flex flex-wrap items-center gap-3">
      <Button {...args}>Kaydet</Button>
      <Button {...args} variant="secondary">
        Onayla
      </Button>
      <Button {...args} variant="outline">
        Vazgeç
      </Button>
      <Button {...args} variant="ghost">
        Filtreleri temizle
      </Button>
      <Button {...args} variant="destructive">
        İptal et
      </Button>
      <Button {...args} variant="link">
        Ayrıntılar
      </Button>
    </div>
  ),
};

export const Sizes: Story = {
  render: (args) => (
    <div className="flex flex-wrap items-center gap-3">
      <Button {...args} size="sm">
        Küçük
      </Button>
      <Button {...args}>Varsayılan</Button>
      <Button {...args} size="lg">
        Büyük
      </Button>
    </div>
  ),
};

export const WithIcon: Story = {
  render: (args) => (
    <Button {...args}>
      <Plus data-icon="inline-start" aria-hidden="true" />
      Yeni etkinlik
    </Button>
  ),
};

export const Disabled: Story = { args: { disabled: true } };

export const Loading: Story = { args: { isLoading: true } };

export const LongTurkishLabel: Story = {
  args: { children: "Rezervasyon önerilerini onayla ve depolara bildir" },
};
