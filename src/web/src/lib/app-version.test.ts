import { afterEach, describe, expect, it, vi } from "vitest";

// The build's version is read once, when the module loads.
async function loadWithBuildVersion(version: string | undefined) {
  vi.resetModules();
  vi.stubEnv("VITE_APP_VERSION", version);
  return import("./app-version");
}

describe("app version", () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it("detects a newer release once and tells the listeners", async () => {
    const version = await loadWithBuildVersion("1.2.0");
    const listener = vi.fn();
    version.subscribeToNewVersion(listener);

    version.reportServerVersion("1.2.0");
    expect(version.hasNewVersion()).toBe(false);

    version.reportServerVersion("1.3.0");
    version.reportServerVersion("1.3.0");
    expect(version.hasNewVersion()).toBe(true);
    expect(listener).toHaveBeenCalledOnce();
  });

  it("ignores responses without a version", async () => {
    const version = await loadWithBuildVersion("1.2.0");

    version.reportServerVersion(null);

    expect(version.hasNewVersion()).toBe(false);
  });

  it("never differs in a development build, which has no version", async () => {
    const version = await loadWithBuildVersion(undefined);

    version.reportServerVersion("1.3.0");

    expect(version.hasNewVersion()).toBe(false);
  });
});
