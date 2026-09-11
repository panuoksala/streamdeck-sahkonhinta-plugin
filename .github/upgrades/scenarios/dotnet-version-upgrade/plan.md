# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade `StreamDeckSahkonhinta.csproj` from `net6.0` to `net10.0`.
**Scope**: Single SDK-style executable project with 620 lines of code, 14 NuGet packages, no project dependencies, and extensive `System.Drawing` usage identified by the assessment.

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 1 project, all on modern .NET (`net6.0`), SDK-style, no dependency graph, and no incompatible packages.

## Tasks

### 01-upgrade-streamdeck-plugin: Upgrade StreamDeckSahkonhinta to .NET 10

Update the `StreamDeckSahkonhinta.csproj` project to target `net10.0`, align framework-related packages with the .NET 10-supported versions from the assessment, and remove package references whose functionality is provided by the framework when appropriate. The task should also verify .NET 10 SDK availability and check whether any `global.json` file constrains SDK resolution.

The assessment flags 6 recommended package upgrades, `System.Net.WebSockets` as included with framework reference, and 76 source-incompatible API findings concentrated in `System.Drawing`/GDI+ usage. Research should start in the project file and the 3 incident-bearing code files from `assessment.md`; fix API issues inline according to the confirmed option and keep Windows-specific drawing support available via the selected Windows compatibility approach.

**Done when**: `StreamDeckSahkonhinta.csproj` targets `net10.0`, package references are compatible with .NET 10, obsolete framework-provided package references are removed where safe, source/API compatibility issues are resolved inline, restore succeeds, and the project builds with 0 errors and 0 warnings.

---

### 02-final-validation: Validate the upgraded solution

Run final solution-level validation after the atomic project upgrade. This covers a clean restore/build of `StreamDeckSahkonhinta.sln`, execution of any discoverable automated tests, and a review for remaining package conflicts, vulnerabilities, or .NET 10 compatibility warnings.

The solution has one project and no project-to-project dependencies, so validation should focus on the final deployable plugin output, package compatibility, and ensuring the upgrade workflow artifacts accurately record what changed. If no automated tests exist, document that explicitly in the task progress details rather than creating a manual validation task.

**Done when**: the solution builds successfully for the upgraded target with 0 errors and 0 warnings, all available automated tests pass or the absence of tests is documented, and progress details record the final validation commands and results.
