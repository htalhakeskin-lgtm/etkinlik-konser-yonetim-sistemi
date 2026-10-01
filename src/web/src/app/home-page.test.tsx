import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { createQueryClient } from "@/lib/query-client";
import { meQuery } from "@/modules/identity";
import { aUser } from "@/test/identity-fixtures";

import { HomePage } from "./home-page";

vi.mock("@tanstack/react-router", () => ({ useNavigate: () => vi.fn() }));

describe("HomePage", () => {
  it("greets the signed-in user under the product name", () => {
    const client = createQueryClient();
    client.setQueryData(meQuery.queryKey, aUser());

    render(
      <QueryClientProvider client={client}>
        <HomePage />
      </QueryClientProvider>,
    );

    expect(screen.getByRole("heading", { level: 1, name: "FestOS" })).toBeInTheDocument();
    expect(screen.getByText("Hoş geldiniz, Ayşe Kaya.")).toBeInTheDocument();
  });
});
