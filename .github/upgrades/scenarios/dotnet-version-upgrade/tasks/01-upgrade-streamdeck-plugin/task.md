# 01-upgrade-streamdeck-plugin: Upgrade StreamDeckSahkonhinta to .NET 10

Update the `StreamDeckSahkonhinta.csproj` project to target `net10.0`, align framework-related packages with the .NET 10-supported versions from the assessment, and remove package references whose functionality is provided by the framework when appropriate. The task should also verify .NET 10 SDK availability and check whether any `global.json` file constrains SDK resolution.

The assessment flags 6 recommended package upgrades, `System.Net.WebSockets` as included with framework reference, and 76 source-incompatible API findings concentrated in `System.Drawing`/GDI+ usage. Research should start in the project file and the 3 incident-bearing code files from `assessment.md`; fix API issues inline according to the confirmed option and keep Windows-specific drawing support available via the selected Windows compatibility approach.

**Done when**: `StreamDeckSahkonhinta.csproj` targets `net10.0`, package references are compatible with .NET 10, obsolete framework-provided package references are removed where safe, source/API compatibility issues are resolved inline, restore succeeds, and the project builds with 0 errors and 0 warnings.

## Research Findings

### Projects Affected
- `StreamDeckSahkonhinta.csproj` — single SDK-style executable project currently targeting `net6.0`; .NET 10 SDK validation succeeded and no `global.json` constrains SDK resolution.

### Files to Modify
- `StreamDeckSahkonhinta.csproj` — change `TargetFramework` to `net10.0`, update recommended package versions, remove `System.Net.WebSockets`, and keep Windows drawing compatibility available.
- `Program.cs` — remove the unused `System.Net.WebSockets` using after dropping the package reference.
- `Services\ElectricPriceService.cs` — update `HttpContent.ReadAsStringAsync()` usage for the .NET 10 behavioral-change finding by passing a cancellation token-compatible overload.
- `Services\IconRenderer.cs` and `SahkonhintaPriceAction.cs` — System.Drawing usage is isolated in the renderer; caller annotations may be needed if platform compatibility warnings surface.

### Packages to Update
| Package | Current | Target | Notes |
|---------|---------|--------|-------|
| Microsoft.Extensions.Hosting | 8.0.0 | 10.0.12 | Assessment recommended package upgrade. |
| Microsoft.Extensions.Configuration | 8.0.0 | 10.0.12 | Assessment recommended package upgrade. |
| Microsoft.Extensions.Configuration.Json | 8.0.0 | 10.0.12 | Assessment recommended package upgrade. |
| Microsoft.Extensions.Logging | 8.0.0 | 10.0.12 | Assessment recommended package upgrade. |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Assessment recommended package upgrade. |
| System.Drawing.Common | 8.0.0 | 10.0.12 | Assessment recommended package upgrade; used by `IconRenderer`. |
| System.Net.WebSockets | 4.3.0 | remove | Functionality included with framework reference. |
| Microsoft.Windows.Compatibility | not present | 10.0.12 | Added per confirmed Windows Compatibility Pack option; stable .NET 10 patch version chosen rather than preview 11 package. |

### API Changes / Migration Patterns
- `System.Drawing`/GDI+ findings are retained inline through `System.Drawing.Common` plus Windows compatibility support; build warnings will determine whether additional platform annotations are required.
- `HttpContent.ReadAsStringAsync()` behavioral change is addressed by using the overload with `CancellationToken.None`.

### Dependencies & Risks
- No project-to-project dependencies and no test projects were discovered for this solution.
- The project contains Windows-specific rendering but also has macOS runtime identifiers; the confirmed upgrade option prioritizes keeping Windows drawing support available rather than replacing the renderer with a cross-platform graphics stack.

### Decisions Made
- Execute as an atomic single-project task: the scope is known, all code changes are in one project, and no skill mandates decomposition.
- Use `dotnet build` for validation because the project is SDK-style, targets modern .NET only, and has no WPF/WinForms/XAML or legacy project features.
