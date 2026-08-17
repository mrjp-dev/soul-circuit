# Tesseract

Unity game project for Soul Circuit. The repository root is one level up at
`E:\Projects\Soul Circuit\`, which also holds the FMOD Studio project in `Audio/`.

## Environment

| Thing | Value |
|---|---|
| Unity | **6000.5.8f1** (Unity 6.5, a Supported release, not LTS) |
| Editor path | `C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe` |
| Render pipeline | URP **17.5.0**, Linear color space |
| Input | `activeInputHandler: 2`, so both the legacy Input Manager and the Input System are live |
| Scenes in build | 1 (`Assets/Scenes/Testing.unity`), of 32 on disk |

Unity 6.5 writes its editor log to `Logs/Editor.log` inside the project by default, not the
old per-user location.

## Vendored third-party (not UPM, so Unity will not update these)

| Asset | Version | Location |
|---|---|---|
| FMOD for Unity | **2.03.14** (`0x00020314`) | `Assets/Plugins/FMOD` |
| Ink for Unity | **1.3.0** | `Assets/Ink` |
| Abiogenesis3d UPixelator | shaderless pixel-art renderer | `Assets/Abiogenesis3d` |

Three vendor demo folders were removed during the Unity 6 migration because each broke the
build while being referenced by nothing: `UPixelator/Example`,
`TextMesh Pro/Examples & Extras`, and the FMOD `ResonanceAudio` addon. If you re-import a
vendor package, expect them back, and remove them again.

### FMOD

Banks live in `Assets/StreamingAssets` (`Master.bank`, `Master.strings.bank`) and are built
from `../../Audio/soul_circuit/soul_circuit.fspro`. `FMODStudioSettings.asset` holds that
path and is **not** shipped in the FMOD package, so preserve it across any FMOD upgrade.

Banks can be rebuilt headlessly with `fmodstudiocl.exe`, which is the CLI binary
(`fmodstudio.exe` is the GUI):

```bash
"C:/Program Files/FMOD SoundSystem/FMOD Studio 2.03.14/fmodstudiocl.exe" -build \
  "E:/Projects/Soul Circuit/Audio/soul_circuit/soul_circuit.fspro"
```

**One exception, and it is a hard one.** The CLI cannot migrate a project between FMOD major
versions. Loading a 2.02-era `.fspro` under 2.03 fails with:

> Project Load Error: Project is out of date and requires project migration. Please load the
> project using the Studio UI tool to migrate the project to the latest version

There is no `-migrate` flag; the full option list is `-build`, `-shared-audio-source-dir`,
`-banks`, `-platforms`, `-export-guids`, `-script`, `-diagnostic`, `-ignore-warnings`,
`-log-dir`, `-log-max-file-size`, `-working-dir`, `-help`. So the first open after a major
version change has to happen in the GUI, once. Every build after that can be headless.

FMOD requires the integration and the Studio that built the banks to agree on major.minor.

## Verifying changes

Always verify headlessly rather than assuming. The exit code alone is not trustworthy for
compile failures, so grep the log.

```bash
"C:/Program Files/Unity/Hub/Editor/6000.5.8f1/Editor/Unity.exe" -batchmode -quit -nographics \
  -projectPath "E:/Projects/Soul Circuit/Game/Tesseract" -logFile ./Logs/compile.log
```

Then check `Logs/compile.log` for `error CS`, `warning CS`, and `Aborting batchmode`.

Tests (there are currently **none**, so this exists for when they are added):

```bash
"C:/Program Files/Unity/Hub/Editor/6000.5.8f1/Editor/Unity.exe" -runTests -batchmode \
  -projectPath "E:/Projects/Soul Circuit/Game/Tesseract" \
  -testPlatform EditMode -testResults ./Logs/TestResults-EditMode.xml -logFile -
```

Unity holds a lock on the project, so only one editor process at a time. Kill any running
`Unity.exe` before starting a batchmode run.

### Importing a .unitypackage

`Unity.exe -importPackage` silently imports nothing when the project has compile errors, and
removing a vendored library guarantees compile errors. Reconstruct the package directly
instead. It is a gzipped tar of GUID-named folders, each with `pathname`, `asset`, and
`asset.meta`: write `asset` to `pathname`, and `asset.meta` to `pathname + ".meta"`. This
preserves every GUID, so settings assets and scene references survive.

## Code layout

Game code is small: **32 files, 3,153 LOC** under `Assets/Scripts`, split into `NPs/`,
`Player/`, and `Systems/`. Everything else under `Assets/` is vendored.

It has **no assembly definition**, so it all compiles into `Assembly-CSharp` and every edit
recompiles the lot. The 9 `.asmdef` files in the project all belong to third parties.

There are **no tests**.

Five managers use an ad-hoc static singleton with inconsistent casing, worth knowing before
you follow the pattern:

- `PlayerControl.Current`, `MenuManager.Current`
- `DataManager.current`, `DialogueManager.current`, `EventManager.current`

`PlayerControl.cs` is 751 LOC and about a quarter of all game code.

## Unity 6.5 gotchas already hit here

- `Object.GetInstanceID()` is obsolete **as an error**. Use `GetEntityId()`.
- `Rigidbody.velocity` is obsolete in favour of `linearVelocity` (warning, auto-upgradable).
- `FindObjectsSortMode` is obsolete. Use `FindObjectsByType<T>()` or
  `FindObjectsByType<T>(FindObjectsInactive)`.
- TextMeshPro is part of `com.unity.ugui` (2.5.0) and TMP mesh UV arrays are `Vector4[]`,
  not `Vector2[]`.
- The `vr` and `unityanalytics` modules are gone.
- URP Compatibility Mode was removed in 6.4, so custom render passes must use Render Graph.
  This project has zero `ScriptableRenderPass` and zero `ScriptableRendererFeature`
  subclasses, so that does not currently apply.

## Console state

A clean batchmode run on 6000.5.8f1 reports **0 errors and 0 warnings**.

One known message remains and is not worth fixing:

```
Assembly reference Packages/com.unity.cinemachine/Editor/Samples/ExposeHDRPInternals/
HDRP-Editor-ref.asmref has no target assembly definition
```

Cinemachine 3.1.7 ships an HDRP sample whose assembly reference cannot resolve in a URP
project. It lives in `Library/PackageCache`, so edits are wiped on reimport; silencing it
would mean embedding the whole package into `Packages/` and giving up UPM updates. The
occurrence count varies with how many compile passes a run does (10 in one run, 3 in
another), which is why it is not a useful signal to count.

Vendored **sample and demo folders** caused most of this migration's breakage. Four were
removed, all unreferenced by game content:

| Folder | Broke |
|---|---|
| `Abiogenesis3d/UPixelator/Example` | `using UnityEngine.XR.WSA`, VR module removed in 6.5 |
| `TextMesh Pro/Examples & Extras` | TMP UVs became `Vector4[]` in ugui 2.5.0 |
| `Plugins/FMOD/addons/ResonanceAudio` | abandoned upstream since Aug 2022 |
| `Ink/Demos` | `story.ink` compile timeout, 5 console messages |

Re-importing any of those vendor packages brings the folders back. Remove them again.

If `'UnityEditor.ProBuilder.ProBuilderEditor' is missing the class attribute
'ExtensionOfNativeClass'` reappears, it is a stale saved window layout holding a ProBuilder
5.2.2 editor window. Reset with `Window > Layouts > Default`. Cosmetic.

## Conventions

Match the file you are editing. Do not reformat untouched lines. Do not edit anything under
`Assets/Plugins/FMOD`, `Assets/Ink`, or `Assets/Abiogenesis3d`: those are vendored and any
change is lost on the next update.
