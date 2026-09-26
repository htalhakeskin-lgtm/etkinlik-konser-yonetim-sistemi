import type { Meta, StoryObj } from "@storybook/react-vite";

import { StatusBadge } from "./status-badge";
import type { StatusTone } from "./status-tone";

const meta = {
  title: "Ortak/Durum rozeti",
  component: StatusBadge,
  args: { tone: "success", label: "Onaylı" },
} satisfies Meta<typeof StatusBadge>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Default: Story = {};

// One example per tone (docs/standards/ui.md §3.3).
const toneExamples: [StatusTone, string][] = [
  ["neutral", "Talep"],
  ["info", "Opsiyonda"],
  ["active", "Hazırlık"],
  ["success", "Onaylı"],
  ["warning", "Güncel değil"],
  ["danger", "Açık çakışma"],
  ["muted", "Kapandı"],
];

export const AllTones: Story = {
  render: () => (
    <div className="flex flex-wrap gap-2">
      {toneExamples.map(([tone, label]) => (
        <StatusBadge key={tone} tone={tone} label={label} />
      ))}
    </div>
  ),
};

// Event statuses and the markers shown next to them (ui.md §3.3).
const eventStatuses: [StatusTone, string][] = [
  ["neutral", "Talep"],
  ["info", "Opsiyonda"],
  ["info", "Müzakere"],
  ["success", "Onaylı"],
  ["active", "Hazırlık"],
  ["active", "Kurulum"],
  ["active", "Canlı"],
  ["active", "Söküm"],
  ["info", "Hesaplaşma"],
  ["muted", "Kapandı"],
  ["muted", "İptal"],
];

export const EventStatuses: Story = {
  render: () => (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap gap-2">
        {eventStatuses.map(([tone, label]) => (
          <StatusBadge key={label} tone={tone} label={label} />
        ))}
      </div>
      <div className="flex flex-wrap gap-2">
        <StatusBadge tone="warning" label="Opsiyon süresi yaklaşıyor" />
        <StatusBadge tone="danger" label="Opsiyon süresi geçti" />
      </div>
    </div>
  ),
};
