import { describe, expect, it } from "vitest";
import { z } from "zod";

describe("server validation texts", () => {
  it("gives schema errors the Turkish texts of the server's codes", () => {
    const schema = z.object({ name: z.string().min(1).max(3), email: z.email() });

    const result = schema.safeParse({ name: "", email: "kimse" });
    const tooLong = schema.safeParse({ name: "dört", email: "a@b.co" });

    expect(result.error?.issues.map((issue) => issue.message)).toEqual([
      "Bu alan zorunludur.",
      "Geçerli bir e-posta adresi girin.",
    ]);
    expect(tooLong.error?.issues[0]?.message).toBe("En fazla 3 karakter olabilir.");
    expect(z.array(z.string()).min(1).safeParse([]).error?.issues[0]?.message).toBe(
      "Bu alan zorunludur.",
    );
  });
});
