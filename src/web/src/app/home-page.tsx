import { useSuspenseQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";

import { meQuery } from "@/modules/identity";

// A placeholder start screen until the role start screens arrive (11 §4).
export function HomePage() {
  const { t } = useTranslation();
  const { data: user } = useSuspenseQuery(meQuery);

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-2 p-4">
      <h1 className="text-3xl font-semibold text-primary">{t("app.name")}</h1>
      <p>{t("home.welcome", { name: user.fullName })}</p>
      <p className="text-muted-foreground">{t("home.placeholder")}</p>
    </div>
  );
}
