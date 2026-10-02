import { createFileRoute } from "@tanstack/react-router";

import { ModelFormPage } from "@/modules/catalog";

export const Route = createFileRoute("/_app/catalog/models/new")({
  component: function NewModelRoute() {
    const navigate = Route.useNavigate();
    return (
      <ModelFormPage
        onSaved={(model) => {
          void navigate({ to: "/catalog/models/$modelId", params: { modelId: model.id } });
        }}
        onCancel={() => {
          void navigate({ to: "/catalog/models" });
        }}
      />
    );
  },
});
