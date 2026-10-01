import { Eye, EyeOff } from "lucide-react";
import { type ComponentProps, useState } from "react";
import { useTranslation } from "react-i18next";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export type PasswordInputProps = Omit<ComponentProps<typeof Input>, "type">;

// A password field the user can reveal, so a long password can be checked before sending it.
export function PasswordInput(props: PasswordInputProps) {
  const { t } = useTranslation("identity");
  const [isVisible, setIsVisible] = useState(false);

  return (
    <div className="flex gap-2">
      <Input {...props} type={isVisible ? "text" : "password"} />
      <Button
        type="button"
        variant="outline"
        aria-pressed={isVisible}
        aria-label={isVisible ? t("password.hide") : t("password.show")}
        onClick={() => {
          setIsVisible((visible) => !visible);
        }}
      >
        {isVisible ? <EyeOff aria-hidden="true" /> : <Eye aria-hidden="true" />}
      </Button>
    </div>
  );
}
