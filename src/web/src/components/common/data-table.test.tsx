import { createColumnHelper } from "@tanstack/react-table";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { DataTable, type DataTableProps } from "./data-table";
import type { dataTableFeatures } from "./data-table-model";

type Person = { id: string; name: string; city: string };

const helper = createColumnHelper<typeof dataTableFeatures, Person>();
const columns = helper.columns([
  helper.accessor("name", { header: "Ad" }),
  helper.accessor("city", { header: "Şehir" }),
]);
const people: Person[] = [
  { id: "1", name: "Ayşe", city: "İzmir" },
  { id: "2", name: "Can", city: "Bursa" },
];

function renderTable(overrides: Partial<DataTableProps<Person>> = {}) {
  const props: DataTableProps<Person> = {
    label: "Kişiler",
    columns,
    rows: people,
    getRowId: (row) => row.id,
    isLoading: false,
    empty: <p>Kayıt yok</p>,
    sort: "name",
    sortable: ["name", "city"],
    onSortChange: vi.fn(),
    page: 1,
    pageSize: 25,
    totalCount: 60,
    onPageChange: vi.fn(),
    onPageSizeChange: vi.fn(),
    ...overrides,
  };
  render(<DataTable {...props} />);
  return props;
}

describe("DataTable", () => {
  it("shows the page's rows under its column headers", () => {
    renderTable();

    const table = screen.getByRole("table", { name: "Kişiler" });
    expect(within(table).getAllByRole("row")).toHaveLength(3);
    expect(within(table).getByText("İzmir")).toBeInTheDocument();
  });

  it("marks the sorted column and reverses it on a second click", async () => {
    const { onSortChange } = renderTable();
    const name = screen.getByRole("columnheader", { name: /Ad/u });

    await userEvent.setup().click(within(name).getByRole("button"));

    expect(name).toHaveAttribute("aria-sort", "ascending");
    expect(screen.getByRole("columnheader", { name: /Şehir/u })).toHaveAttribute(
      "aria-sort",
      "none",
    );
    expect(onSortChange).toHaveBeenCalledWith("-name");
  });

  it("sorts by another column from the start", async () => {
    const { onSortChange } = renderTable({ sort: "-name" });

    await userEvent
      .setup()
      .click(within(screen.getByRole("columnheader", { name: /Şehir/u })).getByRole("button"));

    expect(onSortChange).toHaveBeenCalledWith("city");
  });

  it("says which rows of how many it shows, and pages forward", async () => {
    const { onPageChange } = renderTable({ page: 2 });

    await userEvent.setup().click(screen.getByRole("button", { name: "Sonraki sayfa" }));

    expect(screen.getByText("26–50 / 60 kayıt")).toBeInTheDocument();
    expect(screen.getByText("Sayfa 2 / 3")).toBeInTheDocument();
    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it("cannot go before the first page", () => {
    renderTable();

    expect(screen.getByRole("button", { name: "Önceki sayfa" })).toBeDisabled();
  });

  it("shows the empty state when there is nothing", () => {
    renderTable({ rows: [], totalCount: 0 });

    expect(screen.getByText("Kayıt yok")).toBeInTheDocument();
    expect(screen.queryByText(/kayıt$/u)).not.toBeInTheDocument();
  });

  it("offers to try again when the list could not load", async () => {
    const onRetry = vi.fn();
    renderTable({ rows: undefined, error: new Error("offline"), onRetry });

    await userEvent.setup().click(screen.getByRole("button", { name: "Yeniden dene" }));

    expect(onRetry).toHaveBeenCalledOnce();
  });
});
