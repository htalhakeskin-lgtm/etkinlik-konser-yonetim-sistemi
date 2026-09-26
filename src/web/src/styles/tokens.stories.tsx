import type { Meta, StoryObj } from "@storybook/react-vite";

import { StatusBadge } from "@/components/common/status-badge";
import { type StatusTone, statusToneIcons } from "@/components/common/status-tone";

// Design tokens in the catalogue (docs/standards/ui.md §3, §18). Switch the theme in the toolbar.

const meta = {
  title: "Tasarım sistemi/Renkler ve tonlar",
  parameters: { layout: "padded" },
} satisfies Meta;

export default meta;

type Story = StoryObj<typeof meta>;

// Surface and its text colour; the sample text shows the pair the contrast test measures.
const surfaces: [string, string][] = [
  ["background", "foreground"],
  ["card", "card-foreground"],
  ["popover", "popover-foreground"],
  ["primary", "primary-foreground"],
  ["secondary", "secondary-foreground"],
  ["muted", "muted-foreground"],
  ["accent", "accent-foreground"],
  ["destructive", "destructive-foreground"],
];

const lines = ["border", "input", "ring"];

export const Surfaces: Story = {
  render: () => (
    <div className="grid max-w-3xl grid-cols-2 gap-3 sm:grid-cols-4">
      {surfaces.map(([surface, text]) => (
        <div key={surface} className="flex flex-col gap-1">
          <div
            className="flex h-16 items-center justify-center rounded-lg border text-sm font-medium"
            style={{ background: `var(--${surface})`, color: `var(--${text})` }}
          >
            Aa Işık
          </div>
          <code className="text-xs text-muted-foreground">--{surface}</code>
        </div>
      ))}
      {lines.map((line) => (
        <div key={line} className="flex flex-col gap-1">
          <div
            className="h-16 rounded-lg border-2 bg-card"
            style={{ borderColor: `var(--${line})` }}
          />
          <code className="text-xs text-muted-foreground">--{line}</code>
        </div>
      ))}
    </div>
  ),
};

const tones = Object.keys(statusToneIcons) as StatusTone[];

export const StatusTones: Story = {
  render: () => (
    <table className="text-sm">
      <thead>
        <tr className="text-left text-muted-foreground">
          <th className="py-2 pr-6 font-medium">Ton</th>
          <th className="py-2 pr-6 font-medium">Simge</th>
          <th className="py-2 font-medium">Rozet</th>
        </tr>
      </thead>
      <tbody>
        {tones.map((tone) => {
          const Icon = statusToneIcons[tone];
          return (
            <tr key={tone} className="border-t">
              <td className="py-2 pr-6">
                <code>{tone}</code>
              </td>
              <td className="py-2 pr-6">
                <Icon aria-hidden="true" className="size-4" />
              </td>
              <td className="py-2">
                <StatusBadge tone={tone} label={tone} />
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  ),
};
