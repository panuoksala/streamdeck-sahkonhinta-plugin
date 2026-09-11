# 02-final-validation: Validate the upgraded solution

Run final solution-level validation after the atomic project upgrade. This covers a clean restore/build of `StreamDeckSahkonhinta.sln`, execution of any discoverable automated tests, and a review for remaining package conflicts, vulnerabilities, or .NET 10 compatibility warnings.

The solution has one project and no project-to-project dependencies, so validation should focus on the final deployable plugin output, package compatibility, and ensuring the upgrade workflow artifacts accurately record what changed. If no automated tests exist, document that explicitly in the task progress details rather than creating a manual validation task.

**Done when**: the solution builds successfully for the upgraded target with 0 errors and 0 warnings, all available automated tests pass or the absence of tests is documented, and progress details record the final validation commands and results.

## Research Findings

### Projects Affected
- `StreamDeckSahkonhinta.csproj` — only project in the solution; already upgraded to `net10.0` by task `01-upgrade-streamdeck-plugin`.

### Validation Scope
- Build `StreamDeckSahkonhinta.sln` with restore enabled and no incremental compilation to validate the final state.
- Confirm package references resolve on `net10.0` and check for vulnerable/deprecated packages.
- Discover automated test projects; document absence if none are found.
- Confirm workflow progress artifacts exist for the completed upgrade task.

### Build Tool Decision
- Use `dotnet build` based on the cached decision in `scenario-instructions.md`: SDK-style project, modern .NET target only, and no legacy build requirements.

### Decomposition Decision
- Execute as an atomic validation task: one solution, one project, no test projects, and no internal decision points.
