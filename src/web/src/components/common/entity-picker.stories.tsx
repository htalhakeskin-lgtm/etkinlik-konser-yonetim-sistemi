import type { Meta, StoryObj } from "@storybook/react-vite";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";
import { expect, userEvent, within } from "storybook/test";

import { EntityPicker, type PickerOption } from "./entity-picker";

const people: PickerOption[] = [
  { id: "1", label: "Şebnem Ilgaz", description: "0532 111 22 33" },
  { id: "2", label: "Ayşe Kaya", description: "ayse@example.com" },
  { id: "3", label: "Işıl Demir" },
];

// Stands in for the list endpoint: matches without letter case, as the server's search key does.
function search(text: string): Promise<PickerOption[]> {
  const key = text.toLocaleLowerCase("tr");
  return Promise.resolve(
    people.filter((person) => person.label.toLocaleLowerCase("tr").includes(key)),
  );
}

function Picker({ initial }: { initial: PickerOption | null }) {
  const [client] = useState(() => new QueryClient());
  const [value, setValue] = useState(initial);
  return (
    <QueryClientProvider client={client}>
      <div className="flex w-80 flex-col gap-2">
        <label htmlFor="picker" className="text-sm font-medium">
          Kişi
        </label>
        <EntityPicker
          id="picker"
          value={value}
          onChange={setValue}
          queryKey={(text) => ["people", text]}
          search={search}
          placeholder="Ad yazın"
        />
        <p className="text-sm text-muted-foreground">Seçili: {value?.label ?? "—"}</p>
      </div>
    </QueryClientProvider>
  );
}

const meta = {
  title: "Ortak/Arayarak seçim",
  component: Picker,
  args: { initial: null },
} satisfies Meta<typeof Picker>;

export default meta;

type Story = StoryObj<typeof meta>;

export const Empty: Story = {
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await userEvent.type(canvas.getByRole("combobox", { name: "Kişi" }), "işıl");
    const body = within(canvasElement.ownerDocument.body);
    await userEvent.click(await body.findByRole("option", { name: "Işıl Demir" }));
    await expect(canvas.getByText("Seçili: Işıl Demir")).toBeInTheDocument();
  },
};

export const WithAChoice: Story = {
  args: { initial: people[0] ?? null },
};
