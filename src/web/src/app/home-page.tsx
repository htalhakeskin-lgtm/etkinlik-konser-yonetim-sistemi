import { useTranslation } from "react-i18next";

export function HomePage() {
  const { t } = useTranslation();

  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-2 bg-background p-4 text-foreground">
      <h1 className="text-3xl font-semibold text-primary">{t("app.name")}</h1>
      <p className="text-muted-foreground">{t("home.placeholder")}</p>
    </main>
  );
}
