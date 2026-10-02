import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "@tanstack/react-router";
import { ArrowDown, ArrowUp, Lock, Plus, SearchX, Trash2 } from "lucide-react";
import { type ReactNode, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "sonner";

import {
  activateKit,
  deactivateKit,
  editKit,
  getGetKitQueryKey,
  getKit,
  getListEquipmentModelsQueryKey,
  getListKitsQueryKey,
  listEquipmentModels,
  listKits,
} from "@/api/endpoints/catalog/catalog";
import type { KitDetails } from "@/api/model";
import { ConfirmDialog } from "@/components/common/confirm-dialog";
import { EmptyState } from "@/components/common/empty-state";
import { EntityPicker } from "@/components/common/entity-picker";
import { ErrorState } from "@/components/common/error-state";
import { FormAlert } from "@/components/common/form-alert";
import { PageHeader } from "@/components/common/page-header";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { StatusBadge } from "@/components/common/status-badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage } from "@/lib/api-error-messages";
import { auditPermissions, HistoryTab } from "@/modules/audit";
import { hasPermission, useSignedInUser } from "@/modules/identity";

import { KitNameDialog } from "../components/kit-name-dialog";
import { KitTotal } from "../components/kit-totals";
import {
  invalidDraftKeys,
  type KitLineDraft,
  kitLineDrafts,
  kitLineRequests,
  moveDraft,
} from "../kit-lines";
import { catalogPermissions } from "../permissions";

export type KitDetailTab = "general" | "history";

export type KitDetailPageProps = {
  kitId: string;
  tab: KitDetailTab;
};

const pickerPageSize = 10;

// A kit's page (11 §3): its lines, edited as a whole and saved on the kit's version (catalog CT-04), the
// contents opened down to models with the totals (BR-EQP-003), and its change history.
export function KitDetailPage({ kitId, tab }: KitDetailPageProps) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const canView = hasPermission(user, catalogPermissions.viewKits);
  const kit = useQuery({
    queryKey: getGetKitQueryKey(kitId),
    queryFn: ({ signal }) => getKit(kitId, { signal }),
    enabled: canView,
  });

  if (!canView) {
    return (
      <div className="p-6">
        <EmptyState icon={Lock} title={t("kits.forbidden")} />
      </div>
    );
  }

  if (kit.isError) {
    return (
      <div className="p-6">
        {kit.error instanceof ApiError && kit.error.status === 404 ? (
          <EmptyState icon={SearchX} title={t("kitDetail.notFound")} />
        ) : (
          <ErrorState
            error={kit.error}
            onRetry={() => {
              void kit.refetch();
            }}
          />
        )}
      </div>
    );
  }

  if (kit.data === undefined) {
    return (
      <div className="p-6">
        <SkeletonBlock className="h-64 w-full" />
      </div>
    );
  }

  return <KitDetail key={kit.data.version} kit={kit.data} tab={tab} />;
}

function KitDetail({ kit, tab }: { kit: KitDetails; tab: KitDetailTab }) {
  const { t } = useTranslation("catalog");
  const user = useSignedInUser();
  const queryClient = useQueryClient();
  const canEdit = hasPermission(user, catalogPermissions.editKits);
  const canDeactivate = hasPermission(user, catalogPermissions.deactivateKits);
  const canViewHistory = hasPermission(user, auditPermissions.viewEntries);
  const [drafts, setDrafts] = useState<KitLineDraft[]>(() => kitLineDrafts(kit));
  const [isDirty, setIsDirty] = useState(false);
  const [shouldShowProblems, setShouldShowProblems] = useState(false);
  const [isRenaming, setIsRenaming] = useState(false);
  const [isConfirming, setIsConfirming] = useState(false);
  const isActive = kit.deactivatedAt === null;
  const invalid = invalidDraftKeys(drafts);

  const show = (saved: KitDetails) => {
    queryClient.setQueryData(getGetKitQueryKey(saved.id), saved);
    void queryClient.invalidateQueries({ queryKey: getListKitsQueryKey() });
  };
  const saveLines = useMutation({
    mutationFn: () =>
      editKit(kit.id, { name: kit.name, lines: kitLineRequests(drafts) }, ifMatch(kit.version)),
    onSuccess: (saved) => {
      show(saved);
      toast.success(t("kits.done.saved"));
    },
  });
  const status = useMutation({
    mutationFn: () => (isActive ? deactivateKit : activateKit)(kit.id, ifMatch(kit.version)),
    onSuccess: (saved) => {
      setIsConfirming(false);
      show(saved);
      toast.success(t(isActive ? "kits.done.deactivated" : "kits.done.activated"));
    },
    onError: (error) => {
      setIsConfirming(false);
      toast.error(errorMessage(error));
    },
  });
  const change = (next: KitLineDraft[]) => {
    setDrafts(next);
    setIsDirty(true);
  };
  const update = (key: string, patch: Partial<KitLineDraft>) => {
    change(drafts.map((draft) => (draft.key === key ? { ...draft, ...patch } : draft)));
  };

  return (
    <div className="flex flex-col gap-4 p-6">
      <PageHeader
        title={kit.name}
        description={t("kitDetail.description", { count: kit.lines.length })}
        breadcrumb={
          <Link to="/catalog/kits" className="hover:underline">
            {t("kits.title")}
          </Link>
        }
        actions={
          <>
            {!isActive && <StatusBadge tone="muted" label={t("kits.status.inactive")} />}
            {canDeactivate && (
              <Button
                variant="outline"
                isLoading={status.isPending && !isConfirming}
                onClick={() => {
                  if (isActive) {
                    setIsConfirming(true);
                  } else {
                    status.mutate();
                  }
                }}
              >
                {isActive ? t("kits.actions.deactivate") : t("kits.actions.activate")}
              </Button>
            )}
            {canEdit && (
              <Button
                variant="outline"
                onClick={() => {
                  setIsRenaming(true);
                }}
              >
                {t("kits.actions.rename")}
              </Button>
            )}
          </>
        }
      />
      <nav aria-label={t("kitDetail.tabs")} className="flex gap-1 border-b">
        <TabLink to="/catalog/kits/$kitId" kitId={kit.id} isCurrent={tab === "general"}>
          {t("kitDetail.general")}
        </TabLink>
        {canViewHistory && (
          <TabLink to="/catalog/kits/$kitId/history" kitId={kit.id} isCurrent={tab === "history"}>
            {t("kitDetail.history")}
          </TabLink>
        )}
      </nav>
      {tab === "history" ? (
        <HistoryTab rootType="Kit" rootId={kit.id} />
      ) : (
        <div className="grid gap-4 xl:grid-cols-[2fr_1fr]">
          <Section title={t("kitDetail.lines")}>
            {drafts.length === 0 && (
              <p className="text-sm text-muted-foreground">{t("kitDetail.noLines")}</p>
            )}
            {drafts.length > 0 && (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-28">{t("kitDetail.kind")}</TableHead>
                    <TableHead>{t("kitDetail.target")}</TableHead>
                    <TableHead className="w-24">{t("kitDetail.quantity")}</TableHead>
                    {canEdit && (
                      <TableHead>
                        <span className="sr-only">{t("kits.columns.actions")}</span>
                      </TableHead>
                    )}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {drafts.map((draft, index) => (
                    <TableRow
                      key={draft.key}
                      data-invalid={shouldShowProblems && invalid.has(draft.key) ? true : undefined}
                      className="data-invalid:bg-destructive/5"
                    >
                      <TableCell>
                        {canEdit ? (
                          <Select
                            items={kinds(t)}
                            value={draft.kind}
                            onValueChange={(value) => {
                              update(draft.key, {
                                kind: value === "kit" ? "kit" : "model",
                                target: null,
                              });
                            }}
                          >
                            <SelectTrigger
                              aria-label={t("kitDetail.kindOf", { number: index + 1 })}
                            >
                              <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                              {kinds(t).map((option) => (
                                <SelectItem key={option.value} value={option.value}>
                                  {option.label}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        ) : (
                          t(`kitDetail.kinds.${draft.kind}`)
                        )}
                      </TableCell>
                      <TableCell className="min-w-64 whitespace-normal">
                        {canEdit ? (
                          <TargetPicker
                            id={`kit-line-${draft.key}`}
                            kitId={kit.id}
                            draft={draft}
                            onChange={(target) => {
                              update(draft.key, { target });
                            }}
                          />
                        ) : (
                          draft.target?.label
                        )}
                      </TableCell>
                      <TableCell>
                        {canEdit ? (
                          <Input
                            inputMode="numeric"
                            aria-label={t("kitDetail.quantityOf", { number: index + 1 })}
                            value={draft.quantity}
                            onChange={(event) => {
                              update(draft.key, { quantity: event.target.value });
                            }}
                          />
                        ) : (
                          draft.quantity
                        )}
                      </TableCell>
                      {canEdit && (
                        <TableCell className="text-right whitespace-nowrap">
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            aria-label={t("kitDetail.moveUp", { number: index + 1 })}
                            disabled={index === 0}
                            onClick={() => {
                              change(moveDraft(drafts, index, -1));
                            }}
                          >
                            <ArrowUp aria-hidden="true" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            aria-label={t("kitDetail.moveDown", { number: index + 1 })}
                            disabled={index === drafts.length - 1}
                            onClick={() => {
                              change(moveDraft(drafts, index, 1));
                            }}
                          >
                            <ArrowDown aria-hidden="true" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon-sm"
                            aria-label={t("kitDetail.remove", { number: index + 1 })}
                            onClick={() => {
                              change(drafts.filter((other) => other.key !== draft.key));
                            }}
                          >
                            <Trash2 aria-hidden="true" />
                          </Button>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
            {shouldShowProblems && invalid.size > 0 && (
              <FormAlert>{t("kitDetail.invalidLines")}</FormAlert>
            )}
            {saveLines.isError && <FormAlert>{errorMessage(saveLines.error)}</FormAlert>}
            {canEdit && (
              <div className="flex flex-wrap justify-between gap-2">
                <Button
                  variant="outline"
                  onClick={() => {
                    change([
                      ...drafts,
                      { key: crypto.randomUUID(), kind: "model", target: null, quantity: "1" },
                    ]);
                  }}
                >
                  <Plus aria-hidden="true" />
                  {t("kitDetail.addLine")}
                </Button>
                <div className="flex gap-2">
                  {isDirty && (
                    <Button
                      variant="outline"
                      onClick={() => {
                        setDrafts(kitLineDrafts(kit));
                        setIsDirty(false);
                        setShouldShowProblems(false);
                      }}
                    >
                      {t("kitDetail.undo")}
                    </Button>
                  )}
                  <Button
                    disabled={!isDirty}
                    isLoading={saveLines.isPending}
                    onClick={() => {
                      setShouldShowProblems(true);
                      if (invalid.size === 0) {
                        saveLines.mutate();
                      }
                    }}
                  >
                    {t("kitDetail.save")}
                  </Button>
                </div>
              </div>
            )}
          </Section>
          <Section title={t("kitDetail.contents")}>
            <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1 text-sm">
              <dt className="text-muted-foreground">{t("kits.columns.weight")}</dt>
              <dd>
                <KitTotal totals={kit.totals} of="weight" />
              </dd>
              <dt className="text-muted-foreground">{t("kits.columns.power")}</dt>
              <dd>
                <KitTotal totals={kit.totals} of="power" />
              </dd>
            </dl>
            {kit.contents.length === 0 ? (
              <p className="text-sm text-muted-foreground">{t("kitDetail.noLines")}</p>
            ) : (
              <ul className="flex flex-col gap-1 text-sm">
                {kit.contents.map((item) => (
                  <li key={item.modelId} className="flex justify-between gap-2">
                    <Link
                      to="/catalog/models/$modelId"
                      params={{ modelId: item.modelId }}
                      className="hover:underline"
                    >
                      {item.name}
                    </Link>
                    <span className="tabular-nums">× {item.quantity}</span>
                  </li>
                ))}
              </ul>
            )}
          </Section>
        </div>
      )}
      <KitNameDialog
        open={isRenaming}
        kit={kit}
        onClose={() => {
          setIsRenaming(false);
        }}
        onSaved={(saved) => {
          setIsRenaming(false);
          show(saved);
          toast.success(t("kits.done.saved"));
        }}
      />
      <ConfirmDialog
        open={isConfirming}
        onOpenChange={setIsConfirming}
        title={t("kits.confirm.deactivateTitle", { name: kit.name })}
        description={t("kits.confirm.deactivateDescription")}
        confirmLabel={t("kits.actions.deactivate")}
        isPending={status.isPending}
        onConfirm={() => {
          status.mutate();
        }}
      />
    </div>
  );
}

function kinds(t: (key: "kitDetail.kinds.model" | "kitDetail.kinds.kit") => string) {
  return [
    { value: "model", label: t("kitDetail.kinds.model") },
    { value: "kit", label: t("kitDetail.kinds.kit") },
  ];
}

// A model or a kit, searched among the active ones (BR-SYS-001); a kit is never offered to itself.
function TargetPicker({
  id,
  kitId,
  draft,
  onChange,
}: {
  id: string;
  kitId: string;
  draft: KitLineDraft;
  onChange: (target: KitLineDraft["target"]) => void;
}) {
  const { t } = useTranslation("catalog");
  return draft.kind === "model" ? (
    <EntityPicker
      id={id}
      value={draft.target}
      onChange={onChange}
      placeholder={t("kitDetail.searchModel")}
      queryKey={(q) =>
        getListEquipmentModelsQueryKey({ q, status: "active", pageSize: pickerPageSize })
      }
      search={async (q, signal) =>
        (
          await listEquipmentModels(
            { q: q === "" ? undefined : q, status: "active", pageSize: pickerPageSize },
            { signal },
          )
        ).items.map((model) => ({
          id: model.id,
          label: `${model.brand} ${model.name}`,
          description: model.categoryPath.join(" › "),
        }))
      }
    />
  ) : (
    <EntityPicker
      id={id}
      value={draft.target}
      onChange={onChange}
      placeholder={t("kitDetail.searchKit")}
      queryKey={(q) => getListKitsQueryKey({ q, status: "active", pageSize: pickerPageSize })}
      search={async (q, signal) =>
        (
          await listKits(
            { q: q === "" ? undefined : q, status: "active", pageSize: pickerPageSize },
            { signal },
          )
        ).items
          .filter((kit) => kit.id !== kitId)
          .map((kit) => ({ id: kit.id, label: kit.name }))
      }
    />
  );
}

function Section({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4">
      <h2 className="text-base font-semibold">{title}</h2>
      {children}
    </section>
  );
}

type TabLinkProps = {
  to: "/catalog/kits/$kitId" | "/catalog/kits/$kitId/history";
  kitId: string;
  isCurrent: boolean;
  children: ReactNode;
};

function TabLink({ to, kitId, isCurrent, children }: TabLinkProps) {
  return (
    <Link
      to={to}
      params={{ kitId }}
      aria-current={isCurrent ? "page" : undefined}
      className={
        isCurrent
          ? "-mb-px border-b-2 border-primary px-3 py-2 text-sm font-medium"
          : "-mb-px border-b-2 border-transparent px-3 py-2 text-sm text-muted-foreground hover:text-foreground"
      }
    >
      {children}
    </Link>
  );
}
