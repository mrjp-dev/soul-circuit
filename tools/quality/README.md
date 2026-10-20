# Quality gates

```bash
uv run tools/quality/check.py            # static gates, about 2 s; runs on every commit
uv run tools/quality/check.py --unity    # + EditMode tests, coverage, CRAP; about 1 min, Unity must be closed
```

The pre-commit hook runs the static gates. Enable it once per clone:

```bash
git config core.hooksPath .githooks
```

Compiler gates live in `csc.rsp` beside each game asmdef (`Assets/Scripts`, `Assets/Scripts/Core`,
`Assets/Tests/EditMode`), so Unity itself refuses to compile a violation. Vendored code has its own
`csc.rsp` files that only silence warnings and is never gated.

## Gates

Measured on the code at the time the gates were set (36 game files, 1,650 lines, 183 methods).

| Gate | Threshold | Measured | Proposed | Why it changed |
|---|---|---|---|---|
| Cyclomatic complexity | 10 per method | max 7 | < 22 | 22 is above NIST's ceiling of 15 and would never fire. Per method, because per file it punishes splitting code into more methods. |
| Cognitive complexity | 10 per method | max 7 | < 22 | Sonar's default is 15; this code sits at 7, so 10 keeps the headroom honest. |
| Halstead difficulty | 30 per method | max 18.6 | < 80 | No published threshold exists. 80 would never fire; 30 is a regression tripwire. |
| Method length | 40 lines | max 25 | (added) | |
| Parameters | 5 | max 4 | (added) | |
| Nesting depth | 3 | max 2 | (added) | |
| File length | 300 lines | max 242 | < 500 | 500 is loose for a codebase whose largest file is 242. |
| Line coverage | 100%, `SoulCircuit.Core` only | 93 of 93 | 100% | Core is plain C# and fully testable in EditMode. The Game assembly is MonoBehaviour glue that only runs inside scenes, where 100% means PlayMode tests for every menu and physics step: high cost, little signal. Logic that deserves a test belongs in Core. |
| CRAP | 10, Core only | max 5 | < 25 | At 100% coverage CRAP equals cyclomatic complexity, so it matches the complexity gate. On untested glue, CRAP = CC² + CC fails at CC 5, which would ban every non-trivial MonoBehaviour. |
| Surviving mutants | not gated | | 0 | Stryker cannot target Unity test assemblies; it needs a mirror .NET project per run. A trial on ActionForce gave one surviving mutant (a default string) on fully tested code, so "zero" is a hunt for equivalent mutants. Run on demand if a Core change feels under-tested. |
| Dead code | compiler errors | 0 | 0 | `-warnaserror` turns unused variables, unreachable code and obsolete APIs into build errors. Unity passes `/nowarn:0169 0649` itself, so unused private fields are not caught. |
| Duplicate code | no clone of 50+ tokens | 0 | 0 | jscpd. |
| `any` / `unknown` | `dynamic` banned | 0 | 0 | The C# equivalent. `object` stays allowed: Ink's API returns it. |

## Unity rules

Promoted from Info to errors in `csc.rsp` (Microsoft.Unity.Analyzers 1.27):

| Rule | What it stops |
|---|---|
| UNT0001 | empty Unity messages (`Update() {}` still costs a native call per frame) |
| UNT0002 | `tag ==` instead of `CompareTag` |
| UNT0005 | `Time.deltaTime` inside `FixedUpdate` |
| UNT0006 | misspelled Unity message signatures, which Unity silently never calls |
| UNT0008 | `?.` and `??` on Unity objects, which skip Unity's destroyed-object check |
| UNT0022 | setting position and rotation separately |
| UNT0026 | `GetComponent` allocation patterns |
| UNT0041 | string Animator parameters (hashed on every call) |

Text bans in `check.py`: `GameObject.Find` / `Find*OfType` (pass references), `Debug.Log` (use
`LogWarning` / `LogError` for real conditions), `async void`, public fields on components (use
`[SerializeField] private`), commented-out code.

## Not enforced here

Comment density (10 to 30% of code lines) is enforced per edit by the user-scope Claude hook, not
by this script.
