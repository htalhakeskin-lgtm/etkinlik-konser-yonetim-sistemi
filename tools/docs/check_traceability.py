"""Rule, transition and story traceability between the docs, the code and the tests.

Checks TR-01...TR-04 and reports TR-05 and TR-06 (docs/standards/testing.md §10). Exits with 1 when a
check fails. `--report` also prints the per-story table and the S1 rules and transitions not in code yet.
"""
import glob
import io
import json
import os
import re
import sys

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
os.chdir(ROOT)

RULE = r"BR-[A-Z]+-\d{3}"
TRANSITION = r"T-[A-Z]+-\d{2}"
STORY = r"US-[A-Z]+-\d{3}"
ANY_ID = re.compile(rf"\b(?:{RULE}|{TRANSITION}|{STORY})\b")
# Generated or copied code carries no numbers of its own.
SKIPPED = ("/bin/", "/obj/", "/node_modules/", "src/web/src/api/", "src/web/src/test/contract/")


def read(path):
    return io.open(path, encoding="utf-8").read()


def files(patterns):
    found = set()
    for pattern in patterns:
        for path in glob.glob(pattern, recursive=True):
            normalized = path.replace("\\", "/")
            if not any(part in normalized for part in SKIPPED):
                found.add(normalized)
    return sorted(found)


# --- What the docs define --------------------------------------------------------------------------
rule_types = {}
for block in re.split(r"^#### ", read("docs/03-business-rules.md"), flags=re.M)[1:]:
    rule_id = block.split(" ")[0]
    kind = re.search(r"\*Tür:\* ([^·\n]+)", block)
    rule_types[rule_id] = kind.group(1).strip() if kind else ""
transitions = set(re.findall(rf"^\| ({TRANSITION}) \|", read("docs/04-state-machines.md"), re.M))
stories = {}
for path in glob.glob("docs/02-user-stories/0*.md"):
    for match in re.finditer(rf"^### ({STORY})[^\n]*\n(?:[^\n]*\n)*?Öncelik: ([^·\n]+)[^\n]*\nKurallar: ([^\n]*)", read(path), re.M):
        stories[match.group(1)] = (match.group(2).strip(), re.findall(RULE, match.group(3)))
defined = set(rule_types) | transitions | set(stories)

# --- Where the numbers appear ----------------------------------------------------------------------
test_files = files(["tests/**/*.cs", "tests/e2e/**/*.ts", "src/web/src/**/*.test.ts", "src/web/src/**/*.test.tsx"])
code_files = [
    path
    for path in files(["src/**/*.cs", "src/web/src/**/*.ts", "src/web/src/**/*.tsx"])
    if path not in test_files and ".stories." not in path
]

in_code = {}
for path in code_files:
    for found in ANY_ID.findall(read(path)):
        in_code.setdefault(found, set()).add(path)

tagged_rules, tagged_transitions, in_tests = set(), set(), {}
for path in test_files:
    text = read(path)
    for found in ANY_ID.findall(text):
        in_tests.setdefault(found, set()).add(path)
    if path.endswith(".cs"):
        # xUnit traits on a class or a method (naming §10).
        tagged_rules |= set(re.findall(rf'\[Trait\("Rule",\s*"({RULE})"\)\]', text))
        tagged_transitions |= set(re.findall(rf'\[Trait\("Transition",\s*"({TRANSITION})"\)\]', text))
    else:
        # Front-end and end-to-end tests name the number in the test's title.
        tagged_rules |= set(re.findall(RULE, text))
        tagged_transitions |= set(re.findall(TRANSITION, text))

errors_json = json.loads(read("src/web/src/locales/tr/errors.json"))
returned_rules = set()
for path in files(["src/**/*RuleCodes.cs"]):
    returned_rules |= set(re.findall(RULE, read(path)))

# --- Checks ----------------------------------------------------------------------------------------
failures = []


def check(code, title, problems):
    print(f"{code} {title}: {'ok' if not problems else f'{len(problems)} problem(s)'}")
    for problem in sorted(problems):
        print(f"  - {problem}")
    if problems:
        failures.append(code)


check(
    "TR-01",
    "numbers in code and tests are defined in the docs",
    [f"{found} ({', '.join(sorted(paths))})" for found, paths in {**in_tests, **in_code}.items() if found not in defined],
)
check(
    "TR-02",
    "rules in src/ have a test tagged with them",
    [f"{rule} ({', '.join(sorted(in_code[rule]))})" for rule in in_code if re.fullmatch(RULE, rule) and rule not in tagged_rules],
)
check(
    "TR-03",
    "transitions in src/ have a test tagged with them",
    [f"{transition}" for transition in in_code if re.fullmatch(TRANSITION, transition) and transition not in tagged_transitions],
)
check(
    "TR-04",
    "rule codes the API returns have a Turkish text in errors.json",
    [
        rule
        for rule in returned_rules
        if rule_types.get(rule, "").split()[0:1] in (["Kısıt"], ["Geçiş"], ["Yetki"]) and rule not in errors_json
    ],
)

# --- Reports ---------------------------------------------------------------------------------------
s1_stories = {story: rules for story, (priority, rules) in stories.items() if priority == "Must"}
s1_rules = sorted({rule for rules in s1_stories.values() for rule in rules})
implemented = {found for found in in_code}
print(
    f"TR-05 stories: {len(s1_stories)} S1 stories; "
    f"{sum(1 for rules in s1_stories.values() if rules and all(rule in implemented for rule in rules))} with every rule in code"
)
missing_rules = [rule for rule in s1_rules if rule not in implemented]
missing_transitions = sorted(transition for transition in transitions if transition not in implemented)
print(f"TR-06 not in code yet: {len(missing_rules)} S1 rules, {len(missing_transitions)} transitions")

if "--report" in sys.argv:
    print("\nTR-05 per S1 story (rules in code / tested / total):")
    for story, rules in sorted(s1_stories.items()):
        coded = [rule for rule in rules if rule in implemented]
        tested = [rule for rule in coded if rule in tagged_rules]
        print(f"  {story}: {len(coded)} / {len(tested)} / {len(rules)}")
    print("\nTR-06 S1 rules not in code:", ", ".join(missing_rules) or "—")
    print("TR-06 transitions not in code:", ", ".join(missing_transitions) or "—")

sys.exit(1 if failures else 0)
