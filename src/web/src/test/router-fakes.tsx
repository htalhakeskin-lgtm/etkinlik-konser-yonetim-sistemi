import type { ReactNode } from "react";

type FakeLinkProps = {
  to: string;
  params?: Record<string, string>;
  className?: string;
  "aria-current"?: "page" | undefined;
  children?: ReactNode;
};

/**
 * A router link for component tests: a plain anchor with the path's parameters filled in. A test mocks
 * the router with it: `vi.mock("@tanstack/react-router", async () => ({ Link: (await import("@/test/router-fakes")).FakeLink }))`.
 */
export function FakeLink({ to, params = {}, children, ...rest }: FakeLinkProps) {
  const href = Object.entries(params).reduce(
    (path, [name, value]) => path.replace(`$${name}`, value),
    to,
  );
  return (
    <a href={href} {...rest}>
      {children}
    </a>
  );
}
