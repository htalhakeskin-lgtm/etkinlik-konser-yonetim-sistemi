import { type RowData, useTable } from "@tanstack/react-table";
import { ArrowDown, ArrowUp, ArrowUpDown, ChevronLeft, ChevronRight } from "lucide-react";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

import { ErrorState } from "@/components/common/error-state";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { Button } from "@/components/ui/button";
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

import {
  type DataTableColumns,
  dataTableFeatures,
  type PageSize,
  pageSizes,
} from "./data-table-model";

export type DataTableProps<TData extends RowData> = {
  /** The table's name for assistive technology. */
  label: string;
  columns: DataTableColumns<TData>;
  rows: readonly TData[] | undefined;
  getRowId: (row: TData) => string;
  isLoading: boolean;
  error?: unknown;
  onRetry?: () => void;
  /** Shown instead of the rows when there are none. */
  empty: ReactNode;
  /** The sort parameter, e.g. `-fullName` (api §6.2). */
  sort: string | undefined;
  /** The column ids the server can sort by. */
  sortable: readonly string[];
  onSortChange: (sort: string) => void;
  page: number;
  pageSize: number;
  totalCount: number;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: PageSize) => void;
};

const noRows: never[] = [];
const skeletonRows = [0, 1, 2, 3, 4];

// A server-side list (ui §8): sorting, paging and filtering happen on the server, the state lives in
// the address, and the table shows one page with the total count.
export function DataTable<TData extends RowData>({
  label,
  columns,
  rows,
  getRowId,
  isLoading,
  error,
  onRetry,
  empty,
  sort,
  sortable,
  onSortChange,
  page,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: DataTableProps<TData>) {
  const { t } = useTranslation();
  const table = useTable({
    features: dataTableFeatures,
    columns,
    data: rows ?? noRows,
    getRowId: (row) => getRowId(row),
  });
  const sortField = sort?.replace(/^-/u, "");
  const isDescending = sort?.startsWith("-") === true;
  const columnCount = columns.length;

  if (error !== undefined && error !== null && rows === undefined) {
    return <ErrorState error={error} onRetry={onRetry} />;
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="overflow-auto rounded-md border" aria-busy={isLoading}>
        <Table aria-label={label}>
          <TableHeader className="sticky top-0 z-10 bg-card">
            {table.getHeaderGroups().map((group) => (
              <TableRow key={group.id}>
                {group.headers.map((header) => {
                  const id = header.column.id;
                  const isSortable = sortable.includes(id);
                  const direction =
                    sortField === id ? (isDescending ? "descending" : "ascending") : "none";
                  return (
                    <TableHead key={header.id} aria-sort={isSortable ? direction : undefined}>
                      {header.isPlaceholder ? null : isSortable ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          className="-ml-2"
                          onClick={() => {
                            onSortChange(direction === "ascending" ? `-${id}` : id);
                          }}
                        >
                          <table.FlexRender header={header} />
                          {direction === "ascending" ? (
                            <ArrowUp aria-hidden="true" />
                          ) : direction === "descending" ? (
                            <ArrowDown aria-hidden="true" />
                          ) : (
                            <ArrowUpDown aria-hidden="true" className="opacity-50" />
                          )}
                        </Button>
                      ) : (
                        <table.FlexRender header={header} />
                      )}
                    </TableHead>
                  );
                })}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {rows === undefined && isLoading
              ? skeletonRows.map((index) => (
                  <TableRow key={index}>
                    <TableCell colSpan={columnCount}>
                      <SkeletonBlock className="h-5 w-full" />
                    </TableCell>
                  </TableRow>
                ))
              : table.getRowModel().rows.map((row) => (
                  <TableRow key={row.id}>
                    {row.getAllCells().map((cell) => (
                      <TableCell key={cell.id}>
                        <table.FlexRender cell={cell} />
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
            {rows?.length === 0 && (
              <TableRow>
                <TableCell colSpan={columnCount}>{empty}</TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>
      {totalCount > 0 && (
        <Pagination
          page={page}
          pageSize={pageSize}
          totalCount={totalCount}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
        />
      )}
      {error !== undefined && error !== null && rows !== undefined && (
        <p role="alert" className="text-sm text-status-danger">
          {t("dataTable.refreshFailed")}
        </p>
      )}
    </div>
  );
}

type PaginationProps = Pick<
  DataTableProps<RowData>,
  "page" | "pageSize" | "totalCount" | "onPageChange" | "onPageSizeChange"
>;

function Pagination({
  page,
  pageSize,
  totalCount,
  onPageChange,
  onPageSizeChange,
}: PaginationProps) {
  const { t } = useTranslation();
  const pageCount = Math.max(1, Math.ceil(totalCount / pageSize));
  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, totalCount);
  const sizes = pageSizes.map((size) => ({ value: String(size), label: String(size) }));

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 text-sm">
      <p className="text-muted-foreground">
        {t("dataTable.range", { from, to, total: totalCount })}
      </p>
      <div className="flex items-center gap-2">
        <span className="text-muted-foreground" id="data-table-page-size">
          {t("dataTable.pageSize")}
        </span>
        <Select
          items={sizes}
          value={String(pageSize)}
          onValueChange={(value) => {
            onPageSizeChange(Number(value) as PageSize);
          }}
        >
          <SelectTrigger size="sm" aria-labelledby="data-table-page-size">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {sizes.map((size) => (
              <SelectItem key={size.value} value={size.value}>
                {size.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <span className="px-2">{t("dataTable.page", { page, pageCount })}</span>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label={t("dataTable.previous")}
          disabled={page <= 1}
          onClick={() => {
            onPageChange(page - 1);
          }}
        >
          <ChevronLeft aria-hidden="true" />
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          aria-label={t("dataTable.next")}
          disabled={page >= pageCount}
          onClick={() => {
            onPageChange(page + 1);
          }}
        >
          <ChevronRight aria-hidden="true" />
        </Button>
      </div>
    </div>
  );
}
