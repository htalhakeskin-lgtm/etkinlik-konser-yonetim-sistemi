import { describe, expect, it } from "vitest";

import { ApiError } from "./api-error";
import { errorMessage, fieldErrors, pointerToPath } from "./api-error-messages";

describe("ApiError", () => {
  it("reads the code, values, field errors and trace id of Problem Details", async () => {
    const response = new Response(
      JSON.stringify({
        type: "urn:festos:problem:validation",
        status: 400,
        code: "validation",
        detail: "Validation failed.",
        errors: [{ pointer: "/notes", code: "maxLength", params: { max: 2000 } }],
        traceId: "0af7651916cd43dd8448eb211c80319c",
      }),
      { status: 400, headers: { "Content-Type": "application/problem+json" } },
    );

    const error = await ApiError.fromResponse(response);

    expect(error).toMatchObject({
      status: 400,
      code: "validation",
      message: "Validation failed.",
      errors: [{ pointer: "/notes", code: "maxLength", params: { max: 2000 } }],
      traceId: "0af7651916cd43dd8448eb211c80319c",
    });
  });

  it.each([
    [404, "notFound"],
    [403, "forbidden"],
    [502, "internalError"],
  ])("takes the code of a %i without Problem Details from its status", async (status, code) => {
    const error = await ApiError.fromResponse(
      new Response("<html>Bad gateway</html>", {
        status,
        headers: { "Content-Type": "text/html" },
      }),
    );

    expect(error.code).toBe(code);
  });
});

describe("errorMessage", () => {
  it("shows the text of a technical code", () => {
    expect(errorMessage(new ApiError({ status: 412, code: "concurrencyConflict" }))).toBe(
      "Bu kayıt siz düzenlerken başka biri tarafından değiştirildi.",
    );
  });

  it("shows the network text for a request that got no answer", () => {
    expect(errorMessage(ApiError.network(new TypeError("Failed to fetch")))).toBe(
      "Sunucuya ulaşılamadı. Bağlantınızı kontrol edip yeniden deneyin.",
    );
  });

  it("falls back to the unexpected error text for an unknown code or error", () => {
    const unexpected = "Beklenmeyen bir hata oluştu. Sorun sürerse iz numarasıyla bildirin.";
    expect(errorMessage(new ApiError({ status: 422, code: "SAMPLE-UNKNOWN" }))).toBe(unexpected);
    expect(errorMessage(new Error("boom"))).toBe(unexpected);
  });
});

describe("fieldErrors", () => {
  it("puts each validation message on its form path with the server's values", () => {
    const error = new ApiError({
      status: 400,
      code: "validation",
      errors: [
        { pointer: "/units/3/serialNumber", code: "maxLength", params: { max: 20 } },
        { pointer: "/name", code: "someNewRule", params: {} },
      ],
    });

    expect(fieldErrors(error)).toEqual([
      { path: "units.3.serialNumber", message: "En fazla 20 karakter olabilir." },
      { path: "name", message: "Geçersiz değer." },
    ]);
  });
});

describe("pointerToPath", () => {
  it.each([
    ["/name", "name"],
    ["/units/3/serialNumber", "units.3.serialNumber"],
    ["/a~1b/c~0d", "a/b.c~d"],
    ["", ""],
  ])("turns %s into %s", (pointer, path) => {
    expect(pointerToPath(pointer)).toBe(path);
  });
});
