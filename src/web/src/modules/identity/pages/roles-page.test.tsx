import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen, within } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { listRoles } from "@/api/endpoints/identity/identity";
import { createQueryClient } from "@/lib/query-client";
import { aUser } from "@/test/identity-fixtures";

import { meQuery } from "../session";
import { RolesPage } from "./roles-page";

vi.mock("@/api/endpoints/identity/identity", () => ({
  listRoles: vi.fn(),
  getListRolesQueryKey: () => ["/api/v1/roles"],
  getMe: vi.fn(),
  getGetMeQueryKey: () => ["/api/v1/me"],
}));

const listRolesMock = vi.mocked(listRoles);

function renderPage(permissions = ["Identity.Roles.View"]) {
  const client = createQueryClient();
  client.setQueryData(meQuery.queryKey, aUser({ permissions }));
  render(
    <QueryClientProvider client={client}>
      <RolesPage />
    </QueryClientProvider>,
  );
}

describe("RolesPage", () => {
  beforeEach(() => {
    listRolesMock.mockReset();
  });

  it("shows the roles as columns and each module's permissions as rows", async () => {
    listRolesMock.mockResolvedValue({
      roles: ["systemAdministrator", "generalManager"],
      modules: [
        {
          module: "Identity",
          permissions: [
            { code: "Identity.Users.View", roles: ["systemAdministrator", "generalManager"] },
            { code: "Identity.Users.Create", roles: ["systemAdministrator"] },
          ],
        },
        { module: "Planning", permissions: [{ code: "Planning.Conflicts.View", roles: [] }] },
      ],
    });

    renderPage();

    expect(await screen.findByRole("columnheader", { name: "Genel müdür" })).toBeInTheDocument();
    expect(screen.getByRole("rowheader", { name: "Kullanıcılar ve yetkiler" })).toBeInTheDocument();
    const create = screen.getByRole("row", { name: /Kullanıcı oluşturma/u });
    expect(within(create).getAllByText("Var")).toHaveLength(1);
    expect(within(create).getAllByText("Yok")).toHaveLength(1);
    // A code without a Turkish name yet is still listed, as it is.
    expect(screen.getByText("Planning.Conflicts.View")).toBeInTheDocument();
  });

  it("tells a user without the permission and asks the server nothing", () => {
    renderPage(["Identity.Users.View"]);

    expect(screen.getByText("Bu ekranı görme yetkiniz yok.")).toBeInTheDocument();
    expect(listRolesMock).not.toHaveBeenCalled();
  });
});
