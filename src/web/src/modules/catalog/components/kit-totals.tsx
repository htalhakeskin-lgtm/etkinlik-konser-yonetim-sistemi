import { useTranslation } from "react-i18next";

import type { KitTotals as Totals } from "@/api/model";
import { StatusBadge } from "@/components/common/status-badge";
import { formatDecimal } from "@/lib/format";

/** A kit's weight or power with the "eksik veri" mark when a model in it has no value (BR-EQP-003). */
export function KitTotal({ totals, of }: { totals: Totals; of: "weight" | "power" }) {
  const { t } = useTranslation("catalog");
  const isComplete = of === "weight" ? totals.isWeightComplete : totals.isPowerComplete;
  const value =
    of === "weight"
      ? `${formatDecimal(totals.weightKilograms)} kg`
      : `${totals.powerWatts.toLocaleString("tr-TR")} W`;

  return (
    <span className="inline-flex flex-wrap items-center gap-2 tabular-nums">
      {value}
      {!isComplete && <StatusBadge tone="warning" label={t("kits.missingData")} />}
    </span>
  );
}
