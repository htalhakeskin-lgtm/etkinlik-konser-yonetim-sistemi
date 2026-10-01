import { type ColumnDef, type RowData, tableFeatures } from "@tanstack/react-table";

/** The table features every data table uses; sorting and paging happen on the server (ui §8). */
export const dataTableFeatures = tableFeatures({});

/** The columns of a data table, as the column helper makes them. */
export type DataTableColumns<TData extends RowData> = ColumnDef<
  typeof dataTableFeatures,
  TData,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any -- each column has its own value type, as in the helper's signature
  any
>[];

/** The page sizes the tables offer (ui §8). */
export const pageSizes = [25, 50, 100] as const;

/** A page size the tables offer. */
export type PageSize = (typeof pageSizes)[number];
