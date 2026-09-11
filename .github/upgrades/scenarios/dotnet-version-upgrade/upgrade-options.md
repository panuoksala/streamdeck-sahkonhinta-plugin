# Upgrade Options — StreamDeckSahkonhinta

Assessment: 1 SDK-style project targeting net6.0, proposed net10.0; 6 package upgrades recommended, 76 source-incompatible System.Drawing API findings, and 1 behavioral change.

## Strategy

### Upgrade Strategy
A single modern .NET project with no dependency graph is best handled as one atomic upgrade.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects simultaneously in a single atomic pass. |
| Top-Down | Upgrade entry-point applications first and consolidate shared libraries later; useful for larger modern .NET solutions. |

## Compatibility

### Unsupported API Handling
The assessment found 76 source-incompatible API findings and 1 behavioral change for the selected target framework.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve every API change in the same task, including complex ones, leaving no deferred stubs. |
| Defer Complex Changes | Apply simple replacements inline and create stub-resolution subtasks for complex changes. |

### Windows Native APIs
The assessment found extensive System.Drawing/GDI+ usage, which is Windows-specific.

| Value | Description |
|-------|-------------|
| **Windows Compatibility Pack** (selected) | Add Microsoft.Windows.Compatibility so Windows APIs remain available; the app remains Windows-only until APIs are replaced. |
| No Compatibility Pack | Surface Windows API build errors immediately and replace them with cross-platform alternatives. |
