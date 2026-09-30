import { cn } from "cn";
import { useEffect, useState } from "react";

export type SkeletonBlockProps = {
  /** The size of the content it stands for, so nothing shifts when it arrives (ui §10.1). */
  className?: string;
};

// Waits shown for less than 300 ms show nothing (ui §10.1).
const delayMs = 300;

// The shape of content that is loading. It keeps its size from the start but appears only after
// 300 ms; the section that loads marks itself busy, so the block itself is hidden from assistive technology.
export function SkeletonBlock({ className }: SkeletonBlockProps) {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setIsVisible(true);
    }, delayMs);
    return () => {
      clearTimeout(timer);
    };
  }, []);

  return (
    <div
      aria-hidden="true"
      data-visible={isVisible}
      className={cn(
        "h-4 rounded-md bg-muted motion-safe:animate-pulse",
        !isVisible && "invisible",
        className,
      )}
    />
  );
}
