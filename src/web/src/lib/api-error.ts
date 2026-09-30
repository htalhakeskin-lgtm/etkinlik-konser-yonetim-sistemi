import { z } from "zod";

/** One field error of a validation problem (api §8.2). */
export type ValidationProblem = {
  pointer: string;
  code: string;
  params: Record<string, unknown>;
};

/** The client's own code for a request that got no answer. */
export const networkErrorCode = "network";

const problemSchema = z.object({
  code: z.string(),
  detail: z.string().optional(),
  traceId: z.string().optional(),
  params: z.record(z.string(), z.unknown()).optional(),
  errors: z
    .array(
      z.object({
        pointer: z.string(),
        code: z.string(),
        params: z.record(z.string(), z.unknown()).default({}),
      }),
    )
    .optional(),
});

type ApiErrorInit = {
  status: number;
  code: string;
  detail?: string | undefined;
  params?: Record<string, unknown> | undefined;
  errors?: ValidationProblem[] | undefined;
  traceId?: string | undefined;
  cause?: unknown;
};

/**
 * An API call that did not succeed, read from its Problem Details (api §8): the code the translations
 * use (`errors:{code}`), the values for the message, the field errors of a validation problem and the
 * trace id the user can report. A request that got no answer has status 0 and the code `network`.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly params: Readonly<Record<string, unknown>>;
  readonly errors: readonly ValidationProblem[];
  readonly traceId: string | undefined;

  constructor(init: ApiErrorInit) {
    super(init.detail ?? init.code, { cause: init.cause });
    this.name = "ApiError";
    this.status = init.status;
    this.code = init.code;
    this.params = init.params ?? {};
    this.errors = init.errors ?? [];
    this.traceId = init.traceId;
  }

  /** Reads the Problem Details of a failed response; a response without them gets a code from its status. */
  static async fromResponse(response: Response): Promise<ApiError> {
    const problem = await readProblem(response);
    return new ApiError({
      status: response.status,
      code: problem?.code ?? codeOfStatus(response.status),
      detail: problem?.detail,
      params: problem?.params,
      errors: problem?.errors,
      traceId: problem?.traceId,
    });
  }

  /** A request that got no answer: the connection failed or the server could not be reached. */
  static network(cause: unknown): ApiError {
    return new ApiError({ status: 0, code: networkErrorCode, cause });
  }
}

async function readProblem(response: Response): Promise<z.infer<typeof problemSchema> | undefined> {
  if (!(response.headers.get("Content-Type") ?? "").includes("json")) {
    return undefined;
  }

  try {
    const parsed = problemSchema.safeParse(await response.json());
    return parsed.success ? parsed.data : undefined;
  } catch {
    return undefined;
  }
}

// Only for responses without Problem Details, e.g. an error page of a proxy.
function codeOfStatus(status: number): string {
  switch (status) {
    case 401:
      return "unauthorized";
    case 403:
      return "forbidden";
    case 404:
      return "notFound";
    default:
      return "internalError";
  }
}
