import { CircleAlert } from "lucide-react";
import type { ReactNode } from "react";

export type FormAlertProps = {
  children: ReactNode;
};

// The box above a form's main button for an error that belongs to no single field (ui §7.4).
export function FormAlert({ children }: FormAlertProps) {
  return (
    <div
      role="alert"
      className="flex items-start gap-2 rounded-md border border-status-danger/40 bg-status-danger/10 p-3 text-sm"
    >
      <CircleAlert aria-hidden="true" className="mt-0.5 size-4 shrink-0 text-status-danger" />
      <div>{children}</div>
    </div>
  );
}
