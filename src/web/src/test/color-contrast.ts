// WCAG 2.2 contrast ratio for OKLCH colours, used by the design token tests (docs/standards/ui.md §18).

export type Oklch = {
  lightness: number;
  chroma: number;
  hue: number;
};

const OKLCH_PATTERN = /^oklch\(\s*([\d.]+)\s+([\d.]+)\s+([\d.]+)\s*\)$/;

export function parseOklch(value: string): Oklch {
  const match = OKLCH_PATTERN.exec(value.trim());
  if (match === null) {
    throw new Error(`Not an opaque oklch() colour: ${value}`);
  }
  const [, lightness = "", chroma = "", hue = ""] = match;
  return { lightness: Number(lightness), chroma: Number(chroma), hue: Number(hue) };
}

// OKLCH -> OKLab -> linear sRGB (https://bottosson.github.io/posts/oklab/), clamped to the gamut.
function toLinearSrgb({ lightness, chroma, hue }: Oklch): [number, number, number] {
  const hueRadians = (hue * Math.PI) / 180;
  const a = chroma * Math.cos(hueRadians);
  const b = chroma * Math.sin(hueRadians);

  const l = (lightness + 0.3963377774 * a + 0.2158037573 * b) ** 3;
  const m = (lightness - 0.1055613458 * a - 0.0638541728 * b) ** 3;
  const s = (lightness - 0.0894841775 * a - 1.291485548 * b) ** 3;

  const clamp = (channel: number) => Math.min(1, Math.max(0, channel));
  return [
    clamp(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s),
    clamp(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s),
    clamp(-0.0041960863 * l - 0.7034186147 * m + 1.707614701 * s),
  ];
}

export function relativeLuminance(color: Oklch): number {
  const [red, green, blue] = toLinearSrgb(color);
  return 0.2126 * red + 0.7152 * green + 0.0722 * blue;
}

export function contrastRatio(first: Oklch, second: Oklch): number {
  const firstLuminance = relativeLuminance(first);
  const secondLuminance = relativeLuminance(second);
  const lighter = Math.max(firstLuminance, secondLuminance);
  const darker = Math.min(firstLuminance, secondLuminance);
  return (lighter + 0.05) / (darker + 0.05);
}
