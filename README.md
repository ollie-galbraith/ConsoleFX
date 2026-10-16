# ConsoleFX

A PowerShell module for building interactive terminal UIs. It provides bordered text display, interactive grid menus, layered/stacked menus, and styled text input — all driven by a C# backend that handles ANSI escape sequences, color models, and console resize detection.

---

## Requirements

| Requirement | Value |
|---|---|
| PowerShell Desktop (.NET 4.8) | Windows PowerShell 5.1 |
| PowerShell Core (.NET 10) | PowerShell 7 running on .NET 10 |

The C# assembly must be built before the module can be imported. The project targets .NET Framework 4.8 for Windows PowerShell 5.1 and .NET 10 for PowerShell Core. Both targets are compiled to `lib/net48/` and `lib/net10.0/`.

Build both assemblies on Windows or Linux with the .NET 10 SDK:

```sh
dotnet build src/ConsoleFX/ConsoleFX.csproj --configuration Release
```

The .NET Framework 4.8 target is cross-compiled on Linux using reference assemblies; its resulting `lib/net48/ConsoleFX.dll` runs under Windows PowerShell 5.1 on Windows.

---

## Installation

```powershell
Import-Module ./ConsoleFX.psd1
```

The module manifest (`ConsoleFX.psd1`) loads the appropriate DLL for the running PowerShell edition automatically.

---

## Color Specification

All color parameters accept any of the following formats:

| Format | Example |
|---|---|
| Named color (`ConsoleFX.Color` enum) | `'Cyan'`, `'Magenta'`, `'White'` |
| 256-color index (integer 0–255) | `196` |
| RGB array (three integers 0–255) | `@(255, 128, 0)` |
| Color model object | `[ConsoleFX.ColorModel.RGB]`, `[ConsoleFX.ColorModel.Hex]`, etc. |

Tab completion is registered for all color parameters.

---

## Commands

### `Add-Border`

Wraps one or more lines of text in a styled Unicode border. Supports pipeline input, multi-line content, titles, full-width mode, text formatting (bold, italic, etc.), and custom colors.

```powershell
Add-Border [-Object] <string[]>
           [-Title <string>]
           [-Type <string>]        # Curved | Single | Double | Dotted  (default: Curved)
           [-TextColor <color>]
           [-TextFormatting <FormatType[]>]
           [-BorderColor <color>]
           [-TitleColor <color>]
           [-TitleFormatting <FormatType[]>]
           [-FullWidth]
           [-Padding <int>]        # default: 1
           [-NoWrap]
           [-XPosition <int>]
           [-YPosition <int>]
```

**Examples**

```powershell
# Simple bordered message
"Hello, World!" | Add-Border -Title "Greeting" -BorderColor Yellow

# Output:
# ╭─ Greeting ────╮
# │ Hello, World! │
# ╰───────────────╯

# Double border
Add-Border "Different border types" -Title 'Example' -Type Double

# Output:
# ╔═ Example ══════════════╗
# ║ Different border types ║
# ╚════════════════════════╝

# Full-width, multi-line pipeline
@"
Server : WEB01
Status : Online
"@ | Add-Border -Title "Status" -FullWidth -TextColor Cyan -BorderColor Blue
```

---

### `Select-GridMenu`

Displays an interactive grid menu. The user navigates with arrow keys and confirms with Enter. Escape cancels and returns an empty string.

```powershell
Select-GridMenu -Items <string[]>
                [-Columns <int>]          # default: 3
                [-Border]
                [-Title <string>]         # requires -Border
                [-BorderType <string>]    # requires -Border  (default: Curved)
                [-TextColor <color>]      # default: White
                [-SelectionColor <color>] # default: Magenta
                [-BorderColor <color>]    # requires -Border  (default: Blue)
                [-TitleColor <color>]     # requires -Border  (default: White)
                [-FullWidth <bool>]       # default: $true
                [-StartRow <int>]         # 0-based row to render at  (default: 0)
                [-BorderInset <int>]      # horizontal inset from each edge when FullWidth
                [-IsDimmed]               # render faint (for use as a background layer)
                [-UnderlayGridMenu <GridMenu>]  # dimmed menu to render behind this one
                [-PassThru]              # return { Result; Menu } instead of just the string
```

Returns the selected item as a `string`, or `string.Empty` if the user pressed Escape.

When `-PassThru` is specified, returns a `PSCustomObject` with two properties:

| Property | Type | Description |
|---|---|---|
| `Result` | `string` | The selected item (or empty if cancelled) |
| `Menu` | `ConsoleFX.GridMenu` | A dimmed copy of this menu, ready to pass as `-UnderlayGridMenu` to the next menu |

**Examples**

```powershell
# Basic menu
$choice = Select-GridMenu -Items @('Option A', 'Option B', 'Option C') -Border -Title "Pick one"

# Multi-column with custom colors
$choice = Select-GridMenu -Items $servers -Columns 4 -Border -Title "Servers" `
    -SelectionColor Red -BorderColor @(0, 120, 212) -TitleColor Cyan

# Layered menus (show previous menu dimmed in background)
$step1 = Select-GridMenu -Items @('Alpha', 'Beta', 'Gamma') -Border -Title "Step 1" -PassThru
$step2 = Select-GridMenu -Items @('X', 'Y', 'Z') -Border -Title "Step 2" `
    -StartRow 5 -BorderInset 4 -UnderlayGridMenu $step1.Menu
```

---

### `Show-LayeredGridMenu`

Orchestrates a sequence of menus declared as a list of definition objects. Each previously visited menu is redrawn dimmed as a background layer while the next menu runs on top. Pressing Escape on any menu navigates back one step.

```powershell
Show-LayeredGridMenu -MenuItems <Object[]>
```

Each item in `MenuItems` is a hashtable (or `PSCustomObject`) with the following keys:

| Key | Type | Default | Description |
|---|---|---|---|
| `Index` | `int` | list order | Explicit sort order for the menu |
| `Title` | `string` | — | Display title in the border |
| `Items` | `string[]` | `@()` | The menu choices |
| `Columns` | `int` | `3` | Number of columns |
| `Padding` | `int` | `1` | Inner padding |
| `FullWidth` | `bool` | `$true` | Expand to console width |
| `Border` | `bool` | `$true` | Draw a border |
| `BorderType` | `string` | `Curved` | Border style |
| `StartRow` | `int` | `0` | Render offset from the top |
| `BorderInset` | `int` | `0` | Horizontal inset from each edge |
| `TextColor` | color | `White` | Item text color |
| `SelectionColor` | color | `Magenta` | Highlighted item color |
| `BorderColor` | color | `Blue` | Border color |
| `TitleColor` | color | `Blue` | Title color |
| `NextBySelection` | `hashtable` | — | Map `selectedItem → Index/Title` for branching |
| `NextIndex` | `int` | — | Always advance to this menu index after selection |
| `NextTitle` | `string` | — | Always advance to this menu title after selection |

Returns a hashtable:

```powershell
@{
    MenuOrder               = @(...)   # Ordered list of menu titles
    SelectionsByOriginalOrder = @{...} # Selections keyed by original list position
    OrderedSelections       = @(...)   # Selections in render order
}
```

**Examples**

```powershell
# Linear two-step wizard
$result = Show-LayeredGridMenu -MenuItems @(
    @{
        Index       = 0
        Title       = "Choose Environment"
        Items       = @('Dev', 'Test', 'Prod')
        StartRow    = 0
        BorderInset = 0
        Border      = $true
    },
    @{
        Index       = 1
        Title       = "Choose Action"
        Items       = @('Deploy', 'Rollback', 'Status')
        StartRow    = 5
        BorderInset = 4
        Border      = $true
    }
)

Write-Host "Env: $($result.OrderedSelections[0])  Action: $($result.OrderedSelections[1])"

# Conditional branching — different menu 2 based on menu 1 selection
$result = Show-LayeredGridMenu -MenuItems @(
    @{
        Index          = 0
        Title          = "OS"
        Items          = @('Linux', 'Windows')
        NextBySelection = @{
            'Linux'   = 1
            'Windows' = 2
        }
    },
    @{
        Index = 1
        Title = "Linux Action"
        Items = @('SSH', 'VSCode')
    },
    @{
        Index = 2
        Title = "Windows Action"
        Items = @('WinRM', 'RDP')
    }
)
```

---

### `Read-TextInput`

Displays a styled text input field. The user types freely; Enter confirms, Escape cancels.

```powershell
Read-TextInput -Prompt <string>
               [-InitialValue <string>]
               [-VariableName <string>]   # store result in caller's scope
               [-PassThru]               # return value even when -VariableName is set
               [-Border]
               [-Title <string>]          # requires -Border
               [-BorderType <string>]     # requires -Border  (default: Curved)
               [-BorderColor <color>]     # requires -Border  (default: Blue)
               [-TitleColor <color>]      # requires -Border  (default: White)
               [-FullWidth <bool>]        # default: $true
               [-Padding <int>]           # default: 1
               [-StartRow <int>]          # 0-based render row  (default: 0)
               [-BorderInset <int>]       # horizontal inset from each edge when FullWidth
               [-PromptInline]            # show prompt and input on the same line
               [-AsSecureString]          # mask input with * and return SecureString
               [-PromptColor <color>]     # default: Cyan
               [-TextColor <color>]       # default: White
```

Returns:
- A `string` containing the typed value (empty string if cancelled), unless `-AsSecureString` is specified.
- A `SecureString` when `-AsSecureString` is used. The typed characters are masked as `*` on screen and never exist as a plain string in managed memory.

When `-VariableName` is provided the result is written into a variable in the **caller's** scope. Use `-PassThru` alongside `-VariableName` to also receive the return value.

**Examples**

```powershell
# Simple prompt
$name = Read-TextInput -Prompt "Enter your name" -Border -PromptInline

# Password field — masked, returns SecureString
$pass = Read-TextInput -Prompt "Password" -Border -PromptInline -AsSecureString

# Pre-populated inline prompt at a specific row
$result = Read-TextInput -Prompt "Hostname" -InitialValue "server01" `
    -Border -PromptInline -FullWidth -StartRow 5

# Store into caller variable without pipeline output
Read-TextInput -Prompt "Username" -Border -VariableName 'EnteredUser'
```

---

## Border Styles

Available border styles (pass as `-Type` / `-BorderType`):

| Style | Sample characters |
|---|---|
| `Curved` | `╭ ╮ ╰ ╯ ─ │` |
| `Single` | `┌ ┐ └ ┘ ─ │` |
| `Double` | `╔ ╗ ╚ ╝ ═ ║` |
| `Dotted` | `⣿ ⣾ …` |

---

## Color Model Objects

For precise colors beyond named values, the C# assembly exposes several color model types in the `ConsoleFX.ColorModel` namespace:

- `RGB` — `@{ Red = 255; Green = 128; Blue = 0 }`
- `Hex` — six-character hex string
- `HSV` — hue / saturation / value
- `HSL` — hue / saturation / lightness
- `CMYK` — cyan / magenta / yellow / key

These are accepted anywhere a color parameter is used and are automatically converted to the terminal's RGB ANSI code via `Resolve-ColorCode`.

---

## Architecture

```
ConsoleFX/
├── ConsoleFX.psm1           # Module loader — detects PSEdition, loads DLL, dot-sources scripts
├── ConsoleFX.psd1           # Module manifest
│
├── Public/
│   ├── Add-Border.ps1       # Bordered text renderer
│   ├── Select-GridMenu.ps1  # Single interactive grid menu
│   ├── Show-LayeredGridMenu.ps1  # Multi-step stacked menu orchestrator
│   └── Read-TextInput.ps1   # Interactive text/password input field
│
├── Private/
│   ├── Add-DynamicParameter.ps1  # Helper to register dynamic parameters
│   ├── ConvertTo-Case.ps1        # Pascal ↔ snake_case conversion
│   ├── Resolve-ColorCode.ps1     # Normalises any color format to an ANSI escape string
│   └── Split-LongString.ps1      # Word-wraps text to fit within a border
│
├── lib/
│   ├── net48/ConsoleFX.dll   # .NET Framework 4.8 build
│   ├── net10.0/ConsoleFX.dll # .NET 10 build
│   ├── borders.json          # Border character sets (embedded resource)
│   └── spinners.json         # Spinner frame sets
│
└── src/ConsoleFX/
    ├── BorderManager.cs      # Loads/resolves border styles from JSON
    ├── CharMatrix.cs         # 2D character buffer — renders ANSI output with cursor prefix per row
    ├── ColorModels.cs        # RGB / Hex / HSV / HSL / CMYK types + conversion
    ├── ColorValidator.cs     # PS attribute to validate color parameters
    ├── ConsoleResizeWatcher.cs  # Native interop to detect terminal resize events
    ├── ControlCodes.cs       # ANSI static classes: Cursor, Erase, Format, Screen, Foreground, Background
    ├── GridConverter.cs      # Fits items into a column layout based on console width
    ├── GridManager.cs        # Grid coordinate helpers
    ├── GridMenu.cs           # Interactive grid menu engine (arrow keys, selection, underlay rendering)
    ├── StyleBuilder.cs       # Composable ANSI style sequences
    └── TextInput.cs          # Interactive text input engine (masking, resize, cursor tracking)
```
