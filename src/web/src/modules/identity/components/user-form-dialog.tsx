import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Controller, useForm, useWatch } from "react-hook-form";
import { useTranslation } from "react-i18next";
import { z } from "zod";

import {
  createUser,
  editUser,
  getGetUserQueryKey,
  getUser,
} from "@/api/endpoints/identity/identity";
import { getListWarehousesQueryKey, listWarehouses } from "@/api/endpoints/inventory/inventory";
import { type ListWarehousesParams, Role, type UserCreated, type UserDetails } from "@/api/model";
import { CreateUserBody } from "@/api/zod/identity/identity.zod";
import { FormAlert } from "@/components/common/form-alert";
import { SkeletonBlock } from "@/components/common/skeleton-block";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ifMatch } from "@/lib/api-client";
import { ApiError } from "@/lib/api-error";
import { errorMessage, fieldErrors } from "@/lib/api-error-messages";
import { i18n } from "@/lib/i18n";

const schema = CreateUserBody.extend({
  fullName: CreateUserBody.shape.fullName.trim().min(1).max(200),
  email: z.email().max(320),
  roles: CreateUserBody.shape.roles.min(1),
}).superRefine((values, context) => {
  // A warehouse manager works in at least one warehouse; the server also checks that it is active.
  if (values.roles.includes(Role.warehouseManager) && values.warehouseIds.length === 0) {
    context.addIssue({
      code: "custom",
      path: ["warehouseIds"],
      message: i18n.t("validation:required"),
    });
  }
});

// The warehouses a warehouse manager can be given: the active ones, which are few (inventory §1).
const activeWarehouses: ListWarehousesParams = { status: "active", pageSize: 100 };

type UserFormValues = z.infer<typeof schema>;

const formFields = ["fullName", "email", "roles", "warehouseIds"] as const;

type SaveResult = { kind: "created"; created: UserCreated } | { kind: "saved"; saved: UserDetails };

export type UserFormDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  /** The user being edited; a new user without it. */
  userId?: string | undefined;
  onCreated: (created: UserCreated, fullName: string) => void;
  onSaved: (user: UserDetails) => void;
};

// Creating or editing a user (US-SYS-001): name, email, at least one role and, for a warehouse
// manager, the warehouses they work in.
export function UserFormDialog({
  open,
  onOpenChange,
  userId,
  onCreated,
  onSaved,
}: UserFormDialogProps) {
  const { t } = useTranslation("identity");
  const isEditing = userId !== undefined;
  const user = useQuery({
    queryKey: getGetUserQueryKey(userId ?? ""),
    queryFn: ({ signal }) => getUser(userId ?? "", { signal }),
    enabled: open && isEditing,
  });

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("userForm.editTitle") : t("userForm.createTitle")}
          </DialogTitle>
          <DialogDescription>{t("userForm.required")}</DialogDescription>
        </DialogHeader>
        {isEditing && user.data === undefined ? (
          user.isError ? (
            <FormAlert>{errorMessage(user.error)}</FormAlert>
          ) : (
            <SkeletonBlock className="h-48 w-full" />
          )
        ) : (
          <UserForm
            key={user.data?.version ?? "new"}
            user={user.data}
            onCancel={() => {
              onOpenChange(false);
            }}
            onCreated={onCreated}
            onSaved={onSaved}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}

type UserFormProps = {
  user: UserDetails | undefined;
  onCancel: () => void;
  onCreated: (created: UserCreated, fullName: string) => void;
  onSaved: (user: UserDetails) => void;
};

function UserForm({ user, onCancel, onCreated, onSaved }: UserFormProps) {
  const { t } = useTranslation("identity");
  const { control, handleSubmit, setError } = useForm<UserFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      fullName: user?.fullName ?? "",
      email: user?.email ?? "",
      roles: user?.roles ?? [],
      warehouseIds: user?.warehouseIds ?? [],
    },
    mode: "onTouched",
    reValidateMode: "onChange",
  });
  const save = useMutation({
    mutationFn: async (values: UserFormValues): Promise<SaveResult> =>
      user === undefined
        ? { kind: "created", created: await createUser(values) }
        : { kind: "saved", saved: await editUser(user.id, values, ifMatch(user.version)) },
    onSuccess: (result, values) => {
      if (result.kind === "created") {
        onCreated(result.created, values.fullName);
      } else {
        onSaved(result.saved);
      }
    },
    onError: (error) => {
      if (!(error instanceof ApiError)) {
        return;
      }

      if (error.code === "BR-SYS-015") {
        setError("email", { message: errorMessage(error) }, { shouldFocus: true });
      }

      if (error.code === "BR-SYS-014") {
        setError("warehouseIds", { message: errorMessage(error) }, { shouldFocus: true });
      }

      for (const field of fieldErrors(error)) {
        const name = formFields.find((known) => known === field.path);
        if (name !== undefined) {
          setError(name, { message: field.message }, { shouldFocus: true });
        }
      }
    },
  });
  const isFieldError =
    save.error instanceof ApiError &&
    (save.error.code === "BR-SYS-015" ||
      save.error.code === "BR-SYS-014" ||
      save.error.status === 400);
  const roles = useWatch({ control, name: "roles" });
  const isWarehouseManager = roles.includes(Role.warehouseManager);
  const warehouses = useQuery({
    queryKey: getListWarehousesQueryKey(activeWarehouses),
    queryFn: ({ signal }) => listWarehouses(activeWarehouses, { signal }),
    enabled: isWarehouseManager,
  });

  return (
    <form
      noValidate
      className="flex flex-col gap-6"
      onSubmit={(event) =>
        void handleSubmit((values) => {
          // Warehouses belong to the warehouse manager role; the server keeps none for other roles.
          save.mutate({ ...values, warehouseIds: isWarehouseManager ? values.warehouseIds : [] });
        })(event)
      }
    >
      <FieldGroup>
        <Controller
          control={control}
          name="fullName"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="user-full-name">{t("userForm.fullName")} *</FieldLabel>
              <Input
                {...field}
                id="user-full-name"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="email"
          render={({ field, fieldState }) => (
            <Field data-invalid={fieldState.invalid}>
              <FieldLabel htmlFor="user-email">{t("userForm.email")} *</FieldLabel>
              <Input
                {...field}
                id="user-email"
                type="email"
                autoComplete="off"
                aria-invalid={fieldState.invalid}
              />
              {user !== undefined && (
                <FieldDescription>{t("userForm.emailChange")}</FieldDescription>
              )}
              <FieldError errors={[fieldState.error]} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="roles"
          render={({ field, fieldState }) => (
            <FieldSet data-invalid={fieldState.invalid}>
              <FieldLegend variant="label">{t("userForm.roles")} *</FieldLegend>
              {Object.values(Role).map((role) => (
                <Field key={role} orientation="horizontal">
                  <Checkbox
                    id={`user-role-${role}`}
                    checked={field.value.includes(role)}
                    onCheckedChange={(checked) => {
                      field.onChange(
                        checked
                          ? [...field.value, role]
                          : field.value.filter((held) => held !== role),
                      );
                    }}
                    onBlur={field.onBlur}
                  />
                  <FieldLabel htmlFor={`user-role-${role}`} className="font-normal">
                    {t(`roles.${role}`)}
                  </FieldLabel>
                </Field>
              ))}
              <FieldError errors={[fieldState.error]} />
            </FieldSet>
          )}
        />
        {isWarehouseManager && (
          <Controller
            control={control}
            name="warehouseIds"
            render={({ field, fieldState }) => (
              <FieldSet data-invalid={fieldState.invalid}>
                <FieldLegend variant="label">{t("userForm.warehouses")} *</FieldLegend>
                <FieldDescription>{t("userForm.warehousesHelp")}</FieldDescription>
                {warehouses.data === undefined ? (
                  <SkeletonBlock className="h-16 w-full" />
                ) : warehouses.data.items.length === 0 ? (
                  <p className="text-sm text-muted-foreground">{t("userForm.noWarehouses")}</p>
                ) : (
                  warehouses.data.items.map((warehouse) => (
                    <Field key={warehouse.id} orientation="horizontal">
                      <Checkbox
                        id={`user-warehouse-${warehouse.id}`}
                        checked={field.value.includes(warehouse.id)}
                        onCheckedChange={(checked) => {
                          field.onChange(
                            checked
                              ? [...field.value, warehouse.id]
                              : field.value.filter((held) => held !== warehouse.id),
                          );
                        }}
                        onBlur={field.onBlur}
                      />
                      <FieldLabel
                        htmlFor={`user-warehouse-${warehouse.id}`}
                        className="font-normal"
                      >
                        {warehouse.name} · {warehouse.city}
                      </FieldLabel>
                    </Field>
                  ))
                )}
                <FieldError errors={[fieldState.error]} />
              </FieldSet>
            )}
          />
        )}
      </FieldGroup>
      {save.isError && !isFieldError && <FormAlert>{errorMessage(save.error)}</FormAlert>}
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onCancel}>
          {t("changePassword.cancel")}
        </Button>
        <Button type="submit" isLoading={save.isPending}>
          {user === undefined ? t("userForm.create") : t("userForm.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
