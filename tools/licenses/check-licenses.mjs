// Licence check for the .NET and npm dependencies, direct and transitive
// (ADR-0005, docs/standards/ci.md §9). Needs restored .NET projects and installed pnpm packages.
// Usage: node tools/licenses/check-licenses.mjs [--only npm|nuget]
import { execFileSync, execSync } from "node:child_process";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, "..", "..");
const allowed = JSON.parse(readFileSync(join(here, "allowed-licenses.json"), "utf8"));
const { exceptions } = JSON.parse(readFileSync(join(here, "exceptions.json"), "utf8"));

const onlyIndex = process.argv.indexOf("--only");
const only = onlyIndex === -1 ? undefined : process.argv[onlyIndex + 1];

function globToRegExp(glob) {
  const escaped = glob.replace(/[.+^${}()|[\]\\]/g, "\\$&").replace(/\*/g, ".*");
  return new RegExp(`^${escaped}$`);
}

function matchesAny(name, globs) {
  return globs.some((glob) => globToRegExp(glob).test(name));
}

// "*" or a major version such as "4.x"; a new major version needs a new review.
function versionMatches(version, range) {
  if (range === "*") {
    return true;
  }
  const major = /^(\d+)\.x$/.exec(range)?.[1];
  if (major === undefined) {
    throw new Error(`Unsupported version range "${range}" in exceptions.json`);
  }
  return version.split(".")[0] === major;
}

// SPDX expressions: "A OR B" passes when one side passes, "A AND B" when both do.
function expressionAllowed(expression, isAllowed) {
  const cleaned = expression.replace(/[()]/g, " ").trim();
  if (/\sOR\s/i.test(cleaned)) {
    return cleaned.split(/\s+OR\s+/i).some((part) => expressionAllowed(part, isAllowed));
  }
  if (/\sAND\s/i.test(cleaned)) {
    return cleaned.split(/\s+AND\s+/i).every((part) => expressionAllowed(part, isAllowed));
  }
  return isAllowed(cleaned);
}

const usedExceptions = new Set();

function check(ecosystem, packages) {
  const violations = [];
  for (const { name, version, license } of packages) {
    const isFont = matchesAny(name, allowed.fonts.packages);
    const isAllowed = (id) =>
      allowed.licenses.includes(id) || (isFont && allowed.fonts.licenses.includes(id));
    if (license !== "" && expressionAllowed(license, isAllowed)) {
      continue;
    }
    const exception = exceptions.find(
      (candidate) =>
        candidate.ecosystem === ecosystem &&
        candidate.license === license &&
        matchesAny(name, candidate.packages) &&
        versionMatches(version, candidate.versions),
    );
    if (exception !== undefined) {
      usedExceptions.add(exception);
      continue;
    }
    violations.push(
      `${ecosystem}: ${name}@${version} (${license === "" ? "unknown licence" : license})`,
    );
  }
  return violations;
}

function run(command, args) {
  const options = { cwd: root, encoding: "utf8", maxBuffer: 64 * 1024 * 1024 };
  // pnpm is a .cmd shim on Windows, which needs a shell; the arguments are fixed strings.
  return process.platform === "win32"
    ? execSync([command, ...args].join(" "), options)
    : execFileSync(command, args, options);
}

function npmPackages() {
  const byLicense = JSON.parse(run("pnpm", ["licenses", "list", "--json"]));
  return Object.entries(byLicense).flatMap(([license, entries]) =>
    entries.flatMap((entry) =>
      entry.versions.map((version) => ({
        name: entry.name,
        version,
        license: license === "Unknown" ? "" : license,
      })),
    ),
  );
}

function nugetPackages() {
  const output = run("dotnet", [
    "nuget-license",
    "--input",
    "FestOS.slnx",
    "--include-transitive",
    "--output",
    "Json",
  ]);
  return JSON.parse(output.slice(output.indexOf("["))).map((item) => ({
    name: item.PackageId,
    version: item.PackageVersion,
    license: item.License ?? "",
  }));
}

const violations = [];
if (only !== "nuget") {
  violations.push(...check("npm", npmPackages()));
}
if (only !== "npm") {
  violations.push(...check("nuget", nugetPackages()));
}

for (const exception of exceptions) {
  const isChecked = only === undefined || exception.ecosystem === only;
  if (isChecked && !usedExceptions.has(exception)) {
    console.warn(
      `warning: unused exception for ${exception.packages.join(", ")} (${exception.license}); remove it if the package is gone.`,
    );
  }
}

if (violations.length > 0) {
  console.error(
    `Licences that need a review (docs/standards/ci.md §9):\n  ${violations.join("\n  ")}`,
  );
  process.exit(1);
}
console.log("All dependency licences are allowed.");
