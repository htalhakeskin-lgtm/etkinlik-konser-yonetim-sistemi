import type { Meta, StoryObj } from "@storybook/react-vite";

import { Button } from "@/components/ui/button";

import { PageHeader } from "./page-header";

const meta = {
  title: "Ortak/Sayfa başlığı",
  component: PageHeader,
  args: {
    title: "Yaz Festivali 2027",
    description: "12–14 Temmuz 2027 · Kuruçeşme Arena",
    breadcrumb: (
      <ol className="flex gap-1">
        <li>
          <a href="/events" className="underline-offset-4 hover:underline">
            Etkinlikler
          </a>
        </li>
        <li aria-hidden="true">›</li>
        <li aria-current="page">Yaz Festivali 2027</li>
      </ol>
    ),
    actions: (
      <>
        <Button variant="outline">Düzenle</Button>
        <Button>Onayla</Button>
      </>
    ),
  },
} satisfies Meta<typeof PageHeader>;

export default meta;

type Story = StoryObj<typeof meta>;

export const DetailPage: Story = {};

export const ListPage: Story = {
  args: {
    title: "Etkinlikler",
    description: undefined,
    breadcrumb: undefined,
    actions: <Button>Etkinlik oluştur</Button>,
  },
};
