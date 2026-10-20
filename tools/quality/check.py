"""Soul Circuit quality gates. Run from anywhere:

    uv run tools/quality/check.py            # static gates, a few seconds
    uv run tools/quality/check.py --unity    # + EditMode tests, coverage and CRAP (minutes)

Exit code is the number of failed gates. Thresholds, and why each differs from the
generic list it started from, are in tools/quality/README.md.
"""
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "Game" / "Tesseract"
CODE = [PROJECT / "Assets" / "Scripts", PROJECT / "Assets" / "Tests"]
UNITY = "C:/Program Files/Unity/Hub/Editor/6000.5.8f1/Editor/Unity.exe"

MAX_FILE_LINES = 300
MAX_CRAP = 10  # Core is fully covered, so CRAP equals cyclomatic complexity there
LIZARD = [
    "-C", "10",  # cyclomatic complexity per method
    "-L", "40",  # lines per method
    "-a", "5",  # parameters per method
    "-T", "cognitive_complexity=10",
    "-T", "halstead_difficulty=30",
    "-T", "max_nested_structures=3",
]

# (pattern, reason). Plain regexes over source with comments stripped: crude, but these
# APIs have no legitimate use here, so a false positive costs one rename.
BANNED = [
    (r"\bdynamic\b", "dynamic: unsupported by IL2CPP, and the C# 'any'"),
    (r"\basync\s+void\b", "async void: exceptions escape every handler"),
    (r"GameObject\.Find\b|FindObjectOfType\b|FindFirstObjectByType\b", "name/type search: pass a reference"),
    (r"Debug\.Log\(", "Debug.Log: info spam; use LogWarning/LogError for real conditions"),
    (r"^\s*public\s+(?!static|const|readonly|event|abstract|override|enum|class|struct|interface)"
     r"[\w<>\[\],\s]+\s+\w+\s*(=[^;>]*)?;", "public field on a component: use [SerializeField] private"),
    (r"^\s*//.*[;{}]\s*$", "commented-out code: git keeps history"),
]


def cs_files():
    return [p for d in CODE for p in d.rglob("*.cs")]


def strip_comments(text):
    # Keeps line structure so reported line numbers stay right.
    text = re.sub(r"/\*.*?\*/", lambda m: "\n" * m.group().count("\n"), text, flags=re.S)
    return re.sub(r"(?<!:)//(?!.*[;{}]\s*$).*", "", text)


def gate(name, failures):
    print(f"{'PASS' if not failures else 'FAIL'}  {name}")
    for f in failures:
        print(f"      {f}")
    return bool(failures)


def methods():
    # lizard -w prints only methods over a threshold, one per line.
    out = subprocess.run(["uvx", "lizard@1.24.1", "-l", "csharp", "-Ecognitive", "-Ehalstead",
                          "-ENS", "-w", *LIZARD, *map(str, CODE)], capture_output=True, text=True, encoding="utf-8", errors="replace")
    return [line for line in out.stdout.splitlines() if "warning:" in line]


def file_lengths():
    return [f"{p.relative_to(ROOT)}: {n} lines" for p in cs_files()
            if (n := len(p.read_text(encoding="utf-8").splitlines())) > MAX_FILE_LINES]


def banned():
    hits = []
    for p in cs_files():
        raw = p.read_text(encoding="utf-8").splitlines()
        clean = strip_comments("\n".join(raw)).splitlines()
        for pattern, reason in BANNED:
            # The commented-out-code rule must see comments, every other rule must not.
            lines = raw if reason.startswith("commented") else clean
            # Core holds plain data types; JsonUtility and Inspector structs need public fields.
            if "component" in reason and "Core" in p.parts:
                continue
            hits += [f"{p.relative_to(ROOT)}:{i}: {reason}"
                     for i, line in enumerate(lines, 1) if re.search(pattern, line)]
    return hits


def duplicates():
    # Any clone of 50+ tokens fails (--threshold 0): one extracted helper beats a second copy.
    out = subprocess.run(["bunx", "jscpd@5.4.1", "--format", "csharp", "--min-tokens", "50",
                          "--threshold", "0", "--reporters", "console", "--no-colors", *map(str, CODE)],
                         capture_output=True, text=True, encoding="utf-8", errors="replace", shell=sys.platform == "win32")
    return [] if out.returncode == 0 else re.findall(r"Clone found.*\n.*\n.*", out.stdout) or [out.stdout[-300:]]


def unity_tests():
    # Coverage is gated on SoulCircuit.Core only. The Game assembly is MonoBehaviour glue
    # that needs PlayMode scenes to execute; see README for why 100% there is rejected.
    cov = PROJECT / "Logs" / "coverage"
    log = PROJECT / "Logs" / "quality-tests.log"
    results = PROJECT / "Logs" / "quality-tests.xml"
    subprocess.run([UNITY, "-batchmode", "-runTests", "-testPlatform", "EditMode",
                    "-projectPath", str(PROJECT), "-testResults", str(results), "-logFile", str(log),
                    "-enableCodeCoverage", "-coverageResultsPath", str(cov),
                    "-coverageOptions", "generateAdditionalMetrics;assemblyFilters:+SoulCircuit.Core"])
    if not results.exists():
        return [f"no test results; see {log}"], []

    run = ET.parse(results).getroot()
    failures = [f"{run.get('failed')} of {run.get('total')} tests failed"] if run.get("failed") != "0" else []

    report = max(cov.rglob("TestCoverageResults_*.xml"), key=lambda p: p.stat().st_mtime)
    summary = ET.parse(report).getroot().find("Summary")
    covered, total = int(summary.get("visitedSequencePoints")), int(summary.get("numSequencePoints"))
    if covered < total:
        failures.append(f"Core line coverage {covered}/{total}, required 100%")
    crap = [f"{m.findtext('Name')}: CRAP {m.get('crapScore')}" for m in ET.parse(report).iter("Method")
            if float(m.get("crapScore", 0)) > MAX_CRAP]
    return failures, crap


def main():
    failed = sum([
        gate("method complexity, length, params, nesting, Halstead", methods()),
        gate(f"file length <= {MAX_FILE_LINES}", file_lengths()),
        gate("banned patterns", banned()),
        gate("duplicate code", duplicates()),
    ])
    if "--unity" in sys.argv:
        tests, crap = unity_tests()
        failed += gate("EditMode tests + Core coverage 100%", tests)
        failed += gate(f"CRAP <= {MAX_CRAP} (Core)", crap)
    sys.exit(failed)


if __name__ == "__main__":
    main()
