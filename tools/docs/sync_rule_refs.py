"""Validate cross-references between docs and regenerate each story's 'Kurallar:' line."""
import io, re, glob, os, sys
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
os.chdir(os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "docs")))

br = io.open("03-business-rules.md", encoding="utf-8").read()
rules = re.findall(r"^#### (BR-[A-Z]+-\d{3})", br, re.M)
assert len(rules) == len(set(rules)), "duplicate rule id"

stories = {}
for f in glob.glob("02-user-stories/0*.md"):
    for us in re.findall(r"^### (US-[A-Z]+-\d{3})", io.open(f, encoding="utf-8").read(), re.M):
        assert us not in stories, "duplicate story " + us
        stories[us] = f

refs_br, refs_us = set(), set()
for f in glob.glob("**/*.md", recursive=True):
    t = io.open(f, encoding="utf-8").read()
    refs_br |= set(re.findall(r"BR-[A-Z]+-\d{3}", t))
    refs_us |= set(re.findall(r"US-[A-Z]+-\d{3}", t))
print("rules:", len(rules), "| stories:", len(stories))
print("undefined BR refs:", sorted(refs_br - set(rules)))
print("undefined US refs:", sorted(refs_us - set(stories)))

rev = {}
for b in re.split(r"^#### ", br, flags=re.M)[1:]:
    rid = b.split(" ")[0]
    m = re.search(r"\*Hikayeler:\* ([^·\n]+)", b)
    assert m, rid
    for us in re.findall(r"US-[A-Z]+-\d{3}", m.group(1)):
        rev.setdefault(us, set()).add(rid)
print("stories without rules:", sorted(set(stories) - set(rev)))

ORDER = ["SYS", "PTY", "VEN", "EVT", "RDR", "EQP", "WHS", "MRP"]
key = lambda r: (ORDER.index(r.split("-")[1]), int(r.split("-")[2]))
for f in set(stories.values()):
    s = io.open(f, encoding="utf-8").read()
    s = re.sub(r"\nKurallar: [^\n]*", "", s)
    def ins(m):
        r = sorted(rev.get(m.group(1), []), key=key)
        return m.group(0) + "\nKurallar: " + (", ".join(r) if r else "—")
    s = re.sub(r"^### (US-[A-Z]+-\d{3})[^\n]*\n[^\n]*\n\nÖncelik: [^\n]*", ins, s, flags=re.M)
    io.open(f, "w", encoding="utf-8", newline="\n").write(s)
