import type { LucideIcon } from "lucide-react";
import { Inbox } from "lucide-react";
import type { ReactNode } from "react";

export type EmptyStateProps = {
  title: string;
  description?: string;
  icon?: LucideIcon;
  /** The next step, e.g. creating the first record or clearing the filters (ui §10.2). */
  action?: ReactNode;
};

// What the list will show once there is something, and the next step (ui §10.2).
export function EmptyState({ title, description, icon: Icon = Inbox, action }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center gap-3 px-4 py-12 text-center">
      <Icon aria-hidden="true" className="size-8 text-muted-foreground" />
      <div className="flex max-w-md flex-col gap-1">
        <p className="font-medium">{title}</p>
        {description !== undefined && (
          <p className="text-sm text-muted-foreground">{description}</p>
        )}
      </div>
      {action}
    </div>
  );
}
