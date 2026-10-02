import { Combobox } from "@base-ui/react/combobox";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { CheckIcon, ChevronDownIcon, XIcon } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";

/** A record a picker offers: what it shows and, below it, a hint such as a role or a phone. */
export type PickerOption = {
  id: string;
  label: string;
  description?: string | undefined;
};

export type EntityPickerProps = {
  id: string;
  value: PickerOption | null;
  onChange: (value: PickerOption | null) => void;
  /** The query key of a search, so the same search is cached once. */
  queryKey: (text: string) => readonly unknown[];
  /** Asks the server; the list endpoint with `status=active` (parties MD-03). */
  search: (text: string, signal: AbortSignal) => Promise<PickerOption[]>;
  placeholder?: string | undefined;
  invalid?: boolean | undefined;
  onBlur?: (() => void) | undefined;
};

const searchDelayMs = 300;

// A picker that searches as the user types (parties MD-03): the server filters, the popup shows the
// result window, and the chosen record stays among the items so its label stays in the input.
export function EntityPicker({
  id,
  value,
  onChange,
  queryKey,
  search,
  placeholder,
  invalid,
  onBlur,
}: EntityPickerProps) {
  const { t } = useTranslation();
  const [text, setText] = useState("");
  const [typed, setTyped] = useState("");
  const results = useQuery({
    queryKey: queryKey(typed),
    queryFn: ({ signal }) => search(typed, signal),
    placeholderData: keepPreviousData,
  });

  // The search waits until typing pauses, so each key press is not a request.
  useEffect(() => {
    const timer = setTimeout(() => {
      setTyped(text.trim());
    }, searchDelayMs);
    return () => {
      clearTimeout(timer);
    };
  }, [text]);

  const found = results.data ?? [];
  const items =
    value === null || found.some((option) => option.id === value.id) ? found : [...found, value];

  return (
    <Combobox.Root
      items={items}
      filteredItems={found}
      filter={null}
      value={value}
      itemToStringLabel={(option: PickerOption) => option.label}
      isItemEqualToValue={(item: PickerOption, selected: PickerOption) => item.id === selected.id}
      onValueChange={(next: PickerOption | null) => {
        onChange(next);
      }}
      onInputValueChange={(next, { reason }) => {
        if (reason !== "item-press") {
          setText(next);
        }
      }}
    >
      <div className="relative">
        <Combobox.Input
          id={id}
          placeholder={placeholder}
          aria-invalid={invalid}
          onBlur={onBlur}
          className="h-9 w-full min-w-0 rounded-md border border-input bg-transparent py-1 pr-16 pl-2.5 text-base shadow-xs transition-[color,box-shadow] outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20 md:text-sm dark:bg-input/30"
        />
        <div className="absolute inset-y-0 right-1 flex items-center gap-0.5">
          {value !== null && (
            <Combobox.Clear
              aria-label={t("picker.clear")}
              className="flex size-7 items-center justify-center rounded-sm text-muted-foreground hover:bg-accent"
            >
              <XIcon className="size-4" aria-hidden="true" />
            </Combobox.Clear>
          )}
          <Combobox.Trigger
            aria-label={t("picker.open")}
            className="flex size-7 items-center justify-center rounded-sm text-muted-foreground hover:bg-accent"
          >
            <ChevronDownIcon className="size-4" aria-hidden="true" />
          </Combobox.Trigger>
        </div>
      </div>
      <Combobox.Portal>
        <Combobox.Positioner sideOffset={4} className="isolate z-50">
          <Combobox.Popup
            aria-busy={results.isFetching || undefined}
            className="max-h-72 w-(--anchor-width) min-w-48 overflow-y-auto rounded-md bg-popover p-1 text-popover-foreground shadow-md ring-1 ring-foreground/10"
          >
            <Combobox.Status className="px-2 py-1.5 text-sm text-muted-foreground">
              {results.isFetching ? t("picker.searching") : null}
            </Combobox.Status>
            <Combobox.Empty className="px-2 py-1.5 text-sm text-muted-foreground">
              {results.isFetching ? null : t("picker.empty")}
            </Combobox.Empty>
            <Combobox.List>
              {(option: PickerOption) => (
                <Combobox.Item
                  key={option.id}
                  value={option}
                  className="relative flex cursor-default flex-col rounded-sm py-1.5 pr-8 pl-2 text-sm outline-hidden select-none data-highlighted:bg-accent data-highlighted:text-accent-foreground"
                >
                  <span>{option.label}</span>
                  {option.description !== undefined && (
                    <span className="text-xs text-muted-foreground">{option.description}</span>
                  )}
                  <Combobox.ItemIndicator className="absolute top-2 right-2">
                    <CheckIcon className="size-4" aria-hidden="true" />
                  </Combobox.ItemIndicator>
                </Combobox.Item>
              )}
            </Combobox.List>
          </Combobox.Popup>
        </Combobox.Positioner>
      </Combobox.Portal>
    </Combobox.Root>
  );
}
