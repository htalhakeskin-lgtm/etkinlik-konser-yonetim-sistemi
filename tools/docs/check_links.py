"""Check every relative markdown link (file + #anchor) in the repo docs using GitHub's slug rules."""
import io, os, re, unicodedata, sys
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))

def slug(text):
    # GitHub (html-pipeline): downcase, drop chars that are not letter/mark/number/connector/hyphen/space, spaces -> '-'
    text = re.sub(r"`", "", text)
    text = re.sub(r"\[([^\]]*)\]\([^)]*\)", r"\1", text)  # links in headings
    text = text.lower()
    out = []
    for ch in text:
        cat = unicodedata.category(ch)
        if ch in "- " or cat[0] in "LMN" or cat == "Pc":
            out.append(ch)
    return "".join(out).replace(" ", "-")

def anchors(path):
    res, seen = set(), {}
    in_code = False
    for line in io.open(path, encoding="utf-8"):
        if line.startswith("```"):
            in_code = not in_code
            continue
        if in_code:
            continue
        m = re.match(r"^(#{1,6})\s+(.*?)\s*$", line)
        if m:
            s = slug(m.group(2))
            n = seen.get(s, 0)
            res.add(s if n == 0 else f"{s}-{n}")
            seen[s] = n + 1
    return res

# Only files that git tracks or would track: ignored folders (node_modules, bin, tool state) are skipped.
import subprocess
listed = subprocess.run(
    ["git", "-C", ROOT, "ls-files", "--cached", "--others", "--exclude-standard", "--", "*.md"],
    capture_output=True, text=True, encoding="utf-8", check=True,
).stdout.splitlines()
md_files = [os.path.join(ROOT, p) for p in listed if os.path.exists(os.path.join(ROOT, p))]

cache, broken = {}, []
for f in md_files:
    text = io.open(f, encoding="utf-8").read()
    text = re.sub(r"```.*?```", "", text, flags=re.S)
    for m in re.finditer(r"\]\(([^)\s]+)\)", text):
        target = m.group(1)
        if re.match(r"^[a-z]+:", target):
            continue
        path, _, anchor = target.partition("#")
        full = os.path.normpath(os.path.join(os.path.dirname(f), path)) if path else f
        if not os.path.exists(full):
            broken.append((os.path.relpath(f, ROOT), target, "file missing"))
            continue
        if anchor and full.endswith(".md"):
            if full not in cache:
                cache[full] = anchors(full)
            if anchor not in cache[full]:
                broken.append((os.path.relpath(f, ROOT), target, "anchor missing"))
print(len(md_files), "files checked;", len(broken), "broken links")
exit_code = 1 if broken else 0
for b in broken:
    print(" ", b)
# headings containing uppercase dotted I (their slugs contain U+0307)
for f in md_files:
    for line in io.open(f, encoding="utf-8"):
        if re.match(r"^#{1,6}\s", line) and "İ" in line:
            print("HEADING-WITH-İ:", os.path.relpath(f, ROOT), line.strip())
sys.exit(exit_code)
