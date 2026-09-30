# Installing Mario Kart Toolbox

Download the package for your system from the [Releases](https://github.com/HaroohiePals/MarioKartToolbox/releases) page.
The packages include everything needed to run, you don't need to install .NET.

If there is no package for your system, or you want the latest changes, see [BUILDING.md](./BUILDING.md).

| System | Package |
|---|---|
| Windows (x64) | `MarioKartToolbox-<version>-win-x64.zip` |
| macOS (Apple Silicon) | `MarioKartToolbox-<version>-osx-arm64.dmg` |
| Linux (x64) | `MarioKartToolbox-<version>-linux-x64.tar.gz` |

A graphics card with OpenGL 4.0 support is required.

## Windows

1. Extract the zip to a folder of your choice.
2. Run `MarioKartToolbox.exe`.

If Windows SmartScreen blocks the app, click "More info" and then "Run anyway".

On Windows on ARM, use the x64 package. It runs through the built-in x64 emulation.

The command line tool is `MarioKartToolboxCli.exe`, in the same folder.

## macOS

1. Open the dmg.
2. Drag "Mario Kart Toolbox" into the Applications folder.
3. Open the app.

macOS blocks apps from unknown developers, so it will refuse to open the app the first time.
Go to System Settings > Privacy & Security and click "Open Anyway".

The command line tool is inside the app bundle. Run it from a terminal with:

```
"/Applications/Mario Kart Toolbox.app/Contents/MacOS/MarioKartToolboxCli" --help
```

## Linux

1. Extract the archive:
   ```
   tar -xzf MarioKartToolbox-<version>-linux-x64.tar.gz
   ```
2. Run it:
   ```
   cd MarioKartToolbox-<version>-linux-x64
   ./MarioKartToolbox
   ```

The command line tool is `MarioKartToolboxCli`, in the same folder.

### Linux troubleshooting

Most desktop distributions already have everything installed. If something doesn't work, check the list below.
Package names are for Debian/Ubuntu, other distributions use similar names.

- **Copy and paste between courses doesn't work**
  The clipboard needs `xsel`. Install it with `sudo apt install xsel`.

## Settings

Settings, window layout (imgui.ini) and themes are stored in:

- Windows: `%LOCALAPPDATA%\MarioKartToolbox`
- macOS: `~/Library/Application Support/MarioKartToolbox`
- Linux: `~/.local/share/MarioKartToolbox`

Delete this folder to reset the app to its defaults.
