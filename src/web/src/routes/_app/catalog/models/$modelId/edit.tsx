import { createFileRoute } from "@tanstack/react-router";

import { ModelFormPage } from "@/modules/catalog";

export const Route = createFileRoute("/_app/catalog/models/$modelId/edit")({
  component: function EditModelRoute() {
    const { modelId } = Route.useParams();
    const navigate = Route.useNavigate();
    const toModel = () => {
      void navigate({ to: "/catalog/models/$modelId", params: { modelId } });
    };
    return <ModelFormPage modelId={modelId} onSaved={toModel} onCancel={toModel} />;
  },
});
