# ROM Project — Quick Reference

A **ROM Project** is an unpacked, editable version of a Mario Kart DS ROM. Instead of editing a packed `.nds` file directly, Mario Kart Toolbox extracts it into a folder you can edit with any tool — swap textures, edit courses, tweak overlays — and then rebuilds a fresh `.nds` when you're done.

## Folder layout

After creating a project you'll get a directory like this:

```
MyProject/
├── MyProject.json      # the project file (see below)
├── header.bin          # ROM header
├── banner.bin          # ROM banner (icon + title)
├── rsasig.bin          # only present if the source ROM was signed
├── arm9.bin            # ARM9 main binary
├── arm9ovt.bin         # ARM9 overlay table
├── arm7.bin            # ARM7 main binary
├── arm7ovt.bin         # ARM7 overlay table
├── overlay9/
│   └── overlay9_<id>.bin
├── overlay7/
│   └── overlay7_<id>.bin
└── root/               # the in-ROM filesystem
    ├── Course/
    │   ├── cross_course_arc/   # a .carc unpacked into a folder
    │   └── ...
    ├── MainRace/
    └── ...
```

Anything ending in `_arc` is an unpacked Nitro archive (`.carc`). You can edit the files inside it freely; when you rebuild, the folder is repacked back into a `.carc` automatically. If you don't want archives unpacked, pass `--no-unpack` when creating the project.

## Creating a project

From the CLI:

```
mktb-cli create <input.nds> <name> [-o <outputPath>] [-n]
```

- `<input.nds>` — path to the source ROM.
- `<name>` — name of the project (also used for the `.json` file).
- `-o`, `--output` — where to create the project folder. Defaults to `./<name>`. Must be empty.
- `-n`, `--no-unpack` — keep `.carc` archives packed instead of expanding them.

You can also create a project from the GUI via **File → New ROM Project**.

## Rebuilding a ROM

Once you've made your edits, rebuild the project back into a `.nds` file:

```
mktb-cli build <path/to/MyProject.json> -o out.nds
```

The rebuilder reads the project JSON, walks the working directory, repacks any `_arc` folders, and writes a fresh ROM.

You can also build a ROM from the GUI via **File → Export → Nintendo DS ROM** after opening the project.

## The project JSON

The `.json` file at the root of the project describes how to put the ROM back together. You normally don't need to touch it — except for `IgnoreFilePatterns`, which lets you exclude files from the rebuild (handy for README files, backups, screenshots, editor scratch files, etc. that you want to keep in the project folder but don't belong in the final ROM).

Example:

```json
{
  "Name": "RomProject",
  "Version": 1,
  "RomInfo": {
    "FsRootPath": "root/",
    "HeaderPath": "header.bin",
    "BannerPath": "banner.bin",
    "RsaSignaturePath": "rsasig.bin",
    "Arm9Path": "arm9.bin",
    "Arm9OvtPath": "arm9ovt.bin",
    "Arm9OverlaysPaths": [
      "overlay9/overlay9_0.bin",
      "overlay9/overlay9_1.bin",
      "overlay9/overlay9_2.bin",
      "overlay9/overlay9_3.bin"
    ],
    "Arm7Path": "arm7.bin",
    "Arm7OvtPath": "arm7ovt.bin",
    "Arm7OverlaysPaths": []
  },
  "IgnoreFilePatterns": [
    "*.bak",
    "*.md",
    "notes/**",
    "root/Course/**/*.blend",
    ".DS_Store"
  ]
}
```

### Field reference

| Field | What it's for |
| --- | --- |
| `Name` | Project name shown in the editor. |
| `Version` | Project format version. Leave as-is unless you know what you're doing. |
| `IgnoreFilePatterns` | Glob patterns for files that should be excluded from the rebuilt ROM. |
| `RomInfo.FsRootPath` | Folder (relative to the project) that becomes the ROM filesystem. |
| `RomInfo.HeaderPath` / `BannerPath` / `RsaSignaturePath` | Paths to the extracted header, banner, and optional RSA signature. |
| `RomInfo.Arm9Path` / `Arm9OvtPath` / `Arm9OverlaysPaths` | ARM9 binary, overlay table, and overlay files. |
| `RomInfo.Arm7Path` / `Arm7OvtPath` / `Arm7OverlaysPaths` | ARM7 equivalents. |

### Ignore patterns

`IgnoreFilePatterns` uses standard glob syntax and is matched against paths relative to the project root:

- `*.bak` — any `.bak` file anywhere.
- `notes/**` — everything under a `notes/` folder.
- `root/Course/**/*.blend` — Blender source files kept alongside exported course data.
- `.DS_Store` — macOS metadata files.

Ignored files stay on disk in your project folder; they just won't be included when the ROM is rebuilt.

## Tips

- Don't rename or move the top-level files (`header.bin`, `arm9.bin`, etc.) unless you update the paths in the JSON to match.
- `_arc` folders are repacked automatically — you never need to touch the original `.carc` file.
