import { describe, expect, it } from "vitest";

import { partyRequest, partySchema, withPrimary } from "./party-form";

const organization = {
  kind: "organization" as const,
  name: "Işık Ses",
  firstName: "",
  lastName: "",
  legalName: "",
  roles: ["supplier" as const],
  contactPoints: [],
};

describe("partySchema", () => {
  it("asks a person for a first and last name, and an organization for neither", () => {
    const person = partySchema.safeParse({ ...organization, kind: "person" });

    expect(person.success).toBe(false);
    expect(person.error?.issues.map((issue) => issue.path.join("."))).toEqual([
      "firstName",
      "lastName",
    ]);
    expect(partySchema.safeParse(organization).success).toBe(true);
  });

  it("checks the phone and e-mail formats the server checks", () => {
    const result = partySchema.safeParse({
      ...organization,
      contactPoints: [
        { kind: "phone", value: "0212 11", label: "", isPrimary: false },
        { kind: "email", value: "not-an-address", label: "", isPrimary: false },
        { kind: "phone", value: "+90 (212) 111-22-33", label: "", isPrimary: false },
      ],
    });

    expect(result.error?.issues.map((issue) => issue.path.join("."))).toEqual([
      "contactPoints.0.value",
      "contactPoints.1.value",
    ]);
  });
});

describe("partyRequest", () => {
  it("leaves out the names the kind does not use and empty labels", () => {
    const request = partyRequest({
      ...organization,
      firstName: "Ayşe",
      legalName: "Işık Ses Sistemleri A.Ş.",
      contactPoints: [
        { id: "p-1", kind: "phone", value: "0212 111 22 33", label: "", isPrimary: true },
      ],
    });

    expect(request).toEqual({
      kind: "organization",
      name: "Işık Ses",
      firstName: null,
      lastName: null,
      legalName: "Işık Ses Sistemleri A.Ş.",
      roles: ["supplier"],
      contactPoints: [
        { id: "p-1", kind: "phone", value: "0212 111 22 33", label: null, isPrimary: true },
      ],
    });
  });
});

describe("withPrimary", () => {
  it("keeps one primary per kind and leaves the other kinds alone", () => {
    const points = withPrimary(
      [
        { kind: "phone", value: "1", label: "", isPrimary: true },
        { kind: "email", value: "a@example.com", label: "", isPrimary: true },
        { kind: "phone", value: "2", label: "", isPrimary: false },
      ],
      2,
    );

    expect(points.map((point) => point.isPrimary)).toEqual([false, true, true]);
  });
});
