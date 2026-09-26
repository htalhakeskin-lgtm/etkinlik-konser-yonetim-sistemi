import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import {
  applyThemePreference,
  followSystemTheme,
  readThemePreference,
  saveThemePreference,
} from "./theme";

type ChangeListener = () => void;

let isSystemDark = false;
let changeListeners: ChangeListener[] = [];

function mockSystemTheme() {
  vi.stubGlobal(
    "matchMedia",
    vi.fn(() => ({
      get matches() {
        return isSystemDark;
      },
      addEventListener: (_type: string, listener: ChangeListener) => {
        changeListeners.push(listener);
      },
      removeEventListener: (_type: string, listener: ChangeListener) => {
        changeListeners = changeListeners.filter((item) => item !== listener);
      },
    })),
  );
}

function changeSystemTheme(isDark: boolean) {
  isSystemDark = isDark;
  for (const listener of changeListeners) {
    listener();
  }
}

function isDarkApplied() {
  return document.documentElement.classList.contains("dark");
}

describe("theme", () => {
  beforeEach(() => {
    isSystemDark = false;
    changeListeners = [];
    mockSystemTheme();
    window.localStorage.clear();
    document.documentElement.classList.remove("dark");
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("follows the device setting when nothing is saved", () => {
    isSystemDark = true;

    applyThemePreference(readThemePreference());

    expect(readThemePreference()).toBe("system");
    expect(isDarkApplied()).toBe(true);
  });

  it("keeps a saved light theme on a dark device", () => {
    isSystemDark = true;

    saveThemePreference("light");

    expect(readThemePreference()).toBe("light");
    expect(isDarkApplied()).toBe(false);
  });

  it("forgets the saved choice when the device setting is chosen again", () => {
    saveThemePreference("dark");
    saveThemePreference("system");

    expect(window.localStorage.getItem("festos.theme")).toBeNull();
    expect(isDarkApplied()).toBe(false);
  });

  it("reacts to device changes only while the device setting is chosen", () => {
    const stopFollowing = followSystemTheme();

    changeSystemTheme(true);
    expect(isDarkApplied()).toBe(true);

    saveThemePreference("light");
    changeSystemTheme(true);
    expect(isDarkApplied()).toBe(false);

    stopFollowing();
    expect(changeListeners).toHaveLength(0);
  });
});
