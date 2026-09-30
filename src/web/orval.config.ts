import { defineConfig, type Options } from "orval";

// The API client is generated from the OpenAPI document the Host's build writes (api §14.2): TanStack
// Query hooks and Zod schemas, one folder per module tag. The generated files are committed and never
// edited; CI generates them again and fails on a difference. The contract target generates a client
// from sample endpoints only, so a format the generator cannot handle fails the type check (api §14.1).

function query(input: string, output: string): Options {
  return {
    input: { target: input },
    output: {
      mode: "tags-split",
      target: `${output}/endpoints`,
      schemas: `${output}/model`,
      client: "react-query",
      httpClient: "fetch",
      clean: true,
      override: {
        mutator: { path: "./src/lib/api-client.ts", name: "apiClient" },
        // The request wrapper returns the body and throws on errors, so hooks carry the body type.
        fetch: { includeHttpResponseReturnType: false },
      },
    },
  };
}

function zod(input: string, output: string): Options {
  return {
    input: { target: input },
    output: {
      mode: "tags-split",
      target: `${output}/zod`,
      client: "zod",
      fileExtension: ".zod.ts",
      clean: true,
    },
  };
}

export default defineConfig({
  festos: query("./openapi/festos.json", "./src/api"),
  festosZod: zod("./openapi/festos.json", "./src/api"),
  contract: query("./openapi/contract.json", "./src/test/contract"),
  contractZod: zod("./openapi/contract.json", "./src/test/contract"),
});
