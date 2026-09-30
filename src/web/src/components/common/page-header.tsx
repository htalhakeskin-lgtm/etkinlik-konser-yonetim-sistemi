import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

export type PageHeaderProps = {
  title: string;
  description?: string;
  /** The page path as links, e.g. Etkinlikler › Yaz Festivali 2027 (ui §5.2). */
  breadcrumb?: ReactNode;
  /** The page's main actions, the primary one last. */
  actions?: ReactNode;
};

// The page's title, path and main actions (ui §4, §5.2).
export function PageHeader({ title, description, breadcrumb, actions }: PageHeaderProps) {
  const { t } = useTranslation();

  return (
    <header className="flex flex-col gap-2 pb-6">
      {breadcrumb !== undefined && (
        <nav aria-label={t("pageHeader.breadcrumb")} className="text-sm text-muted-foreground">
          {breadcrumb}
        </nav>
      )}
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-semibold tracking-tight">{title}</h1>
          {description !== undefined && (
            <p className="text-sm text-muted-foreground">{description}</p>
          )}
        </div>
        {actions !== undefined && <div className="flex flex-wrap gap-2">{actions}</div>}
      </div>
    </header>
  );
}
