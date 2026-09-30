# Building Mario Kart Toolbox

If you just want to use the application, see [INSTALL.md](./INSTALL.md) instead.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Git](https://git-scm.com/)

The build runs `git` to embed the current commit in the version, so the source must be a git clone
(not a zip download) and `git` must be on your PATH.

## Getting the source

```
git clone https://github.com/HaroohiePals/MarioKartToolbox.git
cd MarioKartToolbox
```

## .NET CLI

This is the recommended way to build. The commands are the same on Windows, macOS and Linux.

### Build

```
dotnet build MarioKartToolbox.sln
```

### Run

Run the app:

```
dotnet run --project src/HaroohiePals.MarioKartToolbox/HaroohiePals.MarioKartToolbox.csproj
```

Run the command line tool:

```
dotnet run --project src/HaroohiePals.MarioKartToolbox.CommandLineInterface/HaroohiePals.MarioKartToolbox.CommandLineInterface.csproj -- --help
```

### Publish

To make a standalone copy that runs without the .NET SDK installed, publish for your target system.
The second command adds the command line tool to the same folder and can be skipped.

#### Windows (x64)

```
dotnet publish src/HaroohiePals.MarioKartToolbox/HaroohiePals.MarioKartToolbox.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
dotnet publish src/HaroohiePals.MarioKartToolbox.CommandLineInterface/HaroohiePals.MarioKartToolbox.CommandLineInterface.csproj -c Release -r win-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
```

Then start `Release\MarioKartToolbox.exe`.

#### macOS (Apple Silicon)

```
dotnet publish src/HaroohiePals.MarioKartToolbox/HaroohiePals.MarioKartToolbox.csproj -c Release -r osx-arm64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
dotnet publish src/HaroohiePals.MarioKartToolbox.CommandLineInterface/HaroohiePals.MarioKartToolbox.CommandLineInterface.csproj -c Release -r osx-arm64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
```

Then start `./Release/MarioKartToolbox`.

#### Linux (x64)

```
dotnet publish src/HaroohiePals.MarioKartToolbox/HaroohiePals.MarioKartToolbox.csproj -c Release -r linux-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
dotnet publish src/HaroohiePals.MarioKartToolbox.CommandLineInterface/HaroohiePals.MarioKartToolbox.CommandLineInterface.csproj -c Release -r linux-x64 --self-contained true -p:DebugType=None -p:DebugSymbols=false -o ./Release
```

Then start `./Release/MarioKartToolbox`.

Use `--self-contained false` for a smaller output that needs the [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) installed to run.

## Visual Studio and Rider

### Visual Studio (Windows only)

1. Install Visual Studio 2026 with the ".NET desktop development" workload.
2. Open `MarioKartToolbox.sln`.
3. In Solution Explorer, right click `HaroohiePals.MarioKartToolbox` and choose "Set as Startup Project".
4. Press F5 to build and run.

### Rider (Windows, macOS, Linux)

1. Open `MarioKartToolbox.sln`.
2. Select the `HaroohiePals.MarioKartToolbox` run configuration in the toolbar.
3. Click Run or Debug.

Use the Rider version that supports .NET 10.

## Troubleshooting

- **`dotnet` is not found or the SDK version is wrong**
  Install the .NET 10 SDK following the
  [Microsoft instructions for your system](https://learn.microsoft.com/en-us/dotnet/core/install/),
  then check with `dotnet --list-sdks`.

- **The build fails with an error from `git rev-parse`**
  The source is not a git clone, or `git` is not installed. Clone the repository with git as shown above.

### Linux

- **The build works but copy and paste between courses doesn't work**
  This is a missing system package, not a build problem.
  See [Linux troubleshooting in INSTALL.md](./INSTALL.md#linux-troubleshooting).
