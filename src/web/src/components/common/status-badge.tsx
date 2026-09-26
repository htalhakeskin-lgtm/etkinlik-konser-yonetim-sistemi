import { cn } from "cn";

import { type StatusTone, statusToneIcons } from "./status-tone";

// Full class names, so Tailwind finds them when scanning the source.
const toneClasses: Record<StatusTone, string> = {
  neutral: "border-status-neutral-border bg-status-neutral-surface text-status-neutral",
  info: "border-status-info-border bg-status-info-surface text-status-info",
  active: "border-status-active-border bg-status-active-surface text-status-active",
  success: "border-status-success-border bg-status-success-surface text-status-success",
  warning: "border-status-warning-border bg-status-warning-surface text-status-warning",
  danger: "border-status-danger-border bg-status-danger-surface text-status-danger",
  muted: "border-status-muted-border bg-status-muted-surface text-status-muted",
};

export type StatusBadgeProps = {
  tone: StatusTone;
  label: string;
  className?: string;
};

// A status is a tone, an icon and a name; colour never carries the meaning alone (WCAG 1.4.1).
export function StatusBadge({ tone, label, className }: StatusBadgeProps) {
  const Icon = statusToneIcons[tone];

  return (
    <span
      data-tone={tone}
      className={cn(
        "inline-flex h-6 items-center gap-1 rounded-md border px-2 text-xs font-medium whitespace-nowrap",
        toneClasses[tone],
        className,
      )}
    >
      <Icon aria-hidden="true" className="size-3.5 shrink-0" />
      {label}
    </span>
  );
}
