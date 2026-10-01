import { QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createUser, editUser, getUser } from "@/api/endpoints/identity/identity";
import { listWarehouses } from "@/api/endpoints/inventory/inventory";
import type { UserDetails } from "@/api/model";
import { ApiError } from "@/lib/api-error";
import { createQueryClient } from "@/lib/query-client";

import { UserFormDialog } from "./user-form-dialog";

vi.mock("@/api/endpoints/inventory/inventory", () => ({
  listWarehouses: vi.fn(),
  getListWarehousesQueryKey: (params: unknown) => ["/api/v1/warehouses", params],
}));
vi.mock("@/api/endpoints/identity/identity", () => ({
  createUser: vi.fn(),
  editUser: vi.fn(),
  getUser: vi.fn(),
  getGetUserQueryKey: (id: string) => [`/api/v1/users/${id}`],
}));

const details: UserDetails = {
  id: "u-1",
  fullName: "Ali Bal",
  email: "ali@example.com",
  roles: ["bookingManager"],
  warehouseIds: [],
  mustChangePassword: false,
  lockedUntil: null,
  deactivatedAt: null,
  createdAt: "2027-01-04T06:00:00+00:00",
  updatedAt: "2027-01-04T06:00:00+00:00",
  version: 3,
};

function renderDialog(userId?: string) {
  const onCreated = vi.fn();
  const onSaved = vi.fn();
  render(
    <QueryClientProvider client={createQueryClient()}>
      <UserFormDialog
        open
        onOpenChange={vi.fn()}
        userId={userId}
        onCreated={onCreated}
        onSaved={onSaved}
      />
    </QueryClientProvider>,
  );
  return { onCreated, onSaved };
}

describe("UserFormDialog", () => {
  beforeEach(() => {
    vi.mocked(createUser).mockReset();
    vi.mocked(editUser).mockReset();
    vi.mocked(getUser).mockReset();
  });

  it("creates a user with a name, an email and the chosen roles", async () => {
    vi.mocked(createUser).mockResolvedValue({
      id: "u-2",
      temporaryPassword: "AAAA-BBBB-CCCC-DDDD",
    });
    const { onCreated } = renderDialog();
    const user = userEvent.setup();

    await user.click(screen.getByLabelText("Ad soyad *"));
    await user.paste("Banu Can");
    await user.click(screen.getByLabelText("E-posta *"));
    await user.paste("banu@example.com");
    await user.click(screen.getByRole("checkbox", { name: "Teknik müdür" }));
    await user.click(screen.getByRole("button", { name: "Kullanıcıyı oluştur" }));

    await vi.waitFor(() => {
      expect(onCreated).toHaveBeenCalledWith(
        { id: "u-2", temporaryPassword: "AAAA-BBBB-CCCC-DDDD" },
        "Banu Can",
      );
    });
    expect(createUser).toHaveBeenCalledWith({
      fullName: "Banu Can",
      email: "banu@example.com",
      roles: ["technicalManager"],
      warehouseIds: [],
    });
  });

  it("asks for at least one role", async () => {
    renderDialog();
    const user = userEvent.setup();

    await user.click(screen.getByLabelText("Ad soyad *"));
    await user.paste("Banu Can");
    await user.click(screen.getByLabelText("E-posta *"));
    await user.paste("banu@example.com");
    await user.click(screen.getByRole("button", { name: "Kullanıcıyı oluştur" }));

    expect(await screen.findByText("Bu alan zorunludur.")).toBeInTheDocument();
    expect(createUser).not.toHaveBeenCalled();
  });

  it("asks a warehouse manager for at least one active warehouse and sends the chosen one", async () => {
    const warehouseId = "0190f0c4-0000-7000-8000-0000000000b1";
    vi.mocked(listWarehouses).mockResolvedValue({
      items: [
        {
          id: warehouseId,
          name: "Merkez Depo",
          city: "İstanbul",
          address: "Adres",
          isActive: true,
          version: 1,
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
    });
    vi.mocked(createUser).mockResolvedValue({
      id: "u-2",
      temporaryPassword: "AAAA-BBBB-CCCC-DDDD",
    });
    renderDialog();
    const user = userEvent.setup();

    await user.click(screen.getByLabelText("Ad soyad *"));
    await user.paste("Banu Can");
    await user.click(screen.getByLabelText("E-posta *"));
    await user.paste("banu@example.com");
    await user.click(screen.getByRole("checkbox", { name: "Depo sorumlusu" }));
    await user.click(screen.getByRole("button", { name: "Kullanıcıyı oluştur" }));
    expect(await screen.findByText("Bu alan zorunludur.")).toBeInTheDocument();
    await user.click(screen.getByRole("checkbox", { name: "Merkez Depo · İstanbul" }));
    await user.click(screen.getByRole("button", { name: "Kullanıcıyı oluştur" }));

    await vi.waitFor(() => {
      expect(createUser).toHaveBeenCalledWith({
        fullName: "Banu Can",
        email: "banu@example.com",
        roles: ["warehouseManager"],
        warehouseIds: [warehouseId],
      });
    });
    expect(listWarehouses).toHaveBeenCalledWith(
      { status: "active", pageSize: 100 },
      expect.anything(),
    );
  });

  it("shows a taken email under its field", async () => {
    vi.mocked(createUser).mockRejectedValue(new ApiError({ status: 422, code: "BR-SYS-015" }));
    renderDialog();
    const user = userEvent.setup();

    await user.click(screen.getByLabelText("Ad soyad *"));
    await user.paste("Banu Can");
    await user.click(screen.getByLabelText("E-posta *"));
    await user.paste("ali@example.com");
    await user.click(screen.getByRole("checkbox", { name: "Teknik müdür" }));
    await user.click(screen.getByRole("button", { name: "Kullanıcıyı oluştur" }));

    expect(await screen.findByText(/başka bir kullanıcıya ait/u)).toBeInTheDocument();
    expect(screen.getByLabelText("E-posta *")).toHaveAttribute("aria-invalid", "true");
  });

  it("edits the user on the version it loaded", async () => {
    vi.mocked(getUser).mockResolvedValue(details);
    vi.mocked(editUser).mockResolvedValue({ ...details, fullName: "Ali Bal Kaya", version: 4 });
    const { onSaved } = renderDialog("u-1");
    const user = userEvent.setup();

    const name = await screen.findByLabelText("Ad soyad *");
    await user.clear(name);
    await user.type(name, "Ali Bal Kaya");
    await user.click(screen.getByRole("button", { name: "Kaydet" }));

    await vi.waitFor(() => {
      expect(onSaved).toHaveBeenCalledOnce();
    });
    expect(editUser).toHaveBeenCalledWith(
      "u-1",
      {
        fullName: "Ali Bal Kaya",
        email: "ali@example.com",
        roles: ["bookingManager"],
        warehouseIds: [],
      },
      { headers: { "If-Match": '"3"' } },
    );
  });
});
