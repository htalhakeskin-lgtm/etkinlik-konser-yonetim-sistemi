// Commit message rules (docs/standards/git.md §4).
export default {
  extends: ["@commitlint/config-conventional"],
  // Dependabot writes "build(deps): Bump …" titles that cannot be configured to fit the length and
  // case rules. They are accepted here; the squash commit gets a title that follows the rules
  // when the PR is merged (git.md §9).
  ignores: [(message) => /^(build|ci)\(deps(-dev)?\): Bump /.test(message)],
  rules: {
    "type-enum": [
      2,
      "always",
      ["feat", "fix", "perf", "refactor", "test", "docs", "build", "ci", "chore", "revert"],
    ],
    "scope-enum": [
      2,
      "always",
      [
        "identity",
        "audit",
        "parties",
        "catalog",
        "venues",
        "riders",
        "booking",
        "planning",
        "inventory",
        "procurement",
        "host",
        "building-blocks",
        "web",
        "database",
        "tests",
        "deps",
        "deps-dev",
        "adr",
        "standards",
        "release",
      ],
    ],
    "header-max-length": [2, "always", 72],
  },
};
