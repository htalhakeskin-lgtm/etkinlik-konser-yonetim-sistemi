import type { LucideIcon } from "lucide-react";
import {
  CircleCheck,
  CircleDashed,
  CircleDot,
  CircleSlash,
  OctagonAlert,
  Play,
  TriangleAlert,
} from "lucide-react";

// The seven status tones; their meaning is the same on every screen (docs/standards/ui.md §3.3).
export type StatusTone = "neutral" | "info" | "active" | "success" | "warning" | "danger" | "muted";

export const statusToneIcons: Record<StatusTone, LucideIcon> = {
  neutral: CircleDashed,
  info: CircleDot,
  active: Play,
  success: CircleCheck,
  warning: TriangleAlert,
  danger: OctagonAlert,
  muted: CircleSlash,
};
