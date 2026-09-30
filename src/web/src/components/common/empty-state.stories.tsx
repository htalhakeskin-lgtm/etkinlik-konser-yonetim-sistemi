import type { Meta, StoryObj } from "@storybook/react-vite";
import { CircleCheck, SearchX } from "lucide-react";

import { Button } from "@/components/ui/button";

import { EmptyState } from "./empty-state";

const meta = {
  title: "Ortak/Boş durum",
  component: EmptyState,
  args: {
    title: "Henüz etkinlik yok.",
    description: "Oluşturduğunuz etkinlikler burada listelenir.",
    action: <Button>Etkinlik oluştur</Button>,
  },
} satisfies Meta<typeof EmptyState>;

export default meta;

type Story = StoryObj<typeof meta>;

export const NoRecordsYet: Story = {};

export const NoFilterMatch: Story = {
  args: {
    icon: SearchX,
    title: "Bu filtrelere uyan kayıt yok.",
    description: undefined,
    action: <Button variant="outline">Filtreleri temizle</Button>,
  },
};

export const AllDone: Story = {
  args: {
    icon: CircleCheck,
    title: "Bu etkinliğin tüm çıkışları tamamlandı.",
    description: undefined,
    action: undefined,
  },
};
