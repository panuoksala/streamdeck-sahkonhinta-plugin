# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0
- **Commit Strategy**: Single Commit at End

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Compatibility
- Unsupported API Handling: Fix Inline
- Windows Native APIs: Windows Compatibility Pack

## Strategy
**Selected**: All-At-Once
**Rationale**: Single SDK-style project targeting modern .NET (`net6.0`), no project dependency graph, and no incompatible packages.

### Execution Constraints
- Single atomic upgrade: update the target framework, package references, and code compatibility issues together.
- Validate with restore and build after the atomic upgrade is applied.
- Fix API issues inline; do not create deferred stubs unless a blocking issue requires user direction.
- Keep Windows-specific drawing support available through the selected Windows compatibility approach.

## Build Tool Decisions
- **StreamDeckSahkonhinta.csproj**: dotnet build (SDK-style project targeting modern .NET only; no WPF/WinForms/XAML or legacy project features).

## Decisions
- User confirmed the generated upgrade options and target framework net10.0.
