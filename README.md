# OA Layer Manager for AutoCAD

[![Latest release](https://img.shields.io/github/v/release/omarabdlaziz2001/OA-Layer-Manager?label=download)](https://github.com/omarabdlaziz2001/OA-Layer-Manager/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![AutoCAD](https://img.shields.io/badge/AutoCAD-2021%E2%80%932026-red)
![.NET](https://img.shields.io/badge/.NET-Framework%204.8%20%7C%208.0-512BD4)

A user-friendly AutoCAD plugin that **batch-edits layer properties — Color, Lineweight,
and Linetype — across many DWG files at once**, without opening each drawing.

> Point it at a folder of drawings, scan the layers, set the properties you want, and
> apply the changes to every file in one go.

---

## Features

- **Batch processing** — apply layer changes to hundreds of DWGs in one run.
- **Scan & map** — discovers every layer and linetype across the selected files.
- **Color / Lineweight / Linetype** — set any combination per layer; leave a field
  blank to keep it unchanged.
- **Auto-loads linetypes** from `acad.lin` when a drawing doesn't already contain them.
- **Clear results** — reports any files that couldn't be processed and why
  (read-only, open in AutoCAD, etc.).
- **Ribbon integration** — adds an **OA Tools** tab; also available via the
  `OA_BATCHLAYERS` command.

## Requirements

- **AutoCAD 2021–2026** (Windows 64-bit). The installer ships two builds and AutoCAD
  loads the right one automatically:
  - **2021–2024** → .NET Framework 4.8 build
  - **2025–2026** → .NET 8 build

## Installation

**Recommended — one-click installer:**

1. Download **`OALayerManager-Setup.exe`** from the
   [latest release](https://github.com/omarabdlaziz2001/OA-Layer-Manager/releases/latest).
2. Run it. (It installs per-user, so **no admin rights** are needed. If Windows
   SmartScreen warns about an unknown publisher, click **More info → Run anyway** —
   the installer is unsigned.)
3. Start AutoCAD and open the **OA Tools** ribbon tab.
   - If AutoCAD shows a security prompt about an unapproved application, choose
     **Always Load**.

**Alternative — ZIP:** download `OALayerManager-Installer.zip`, extract it, and
double-click `Install.bat`.

**Uninstall:** via *Settings → Apps* ("OA Layer Manager"), or delete the folder
`%APPDATA%\Autodesk\ApplicationPlugins\OALayerManager.bundle`.

## Usage

1. Click **Batch Layers** on the **OA Tools** tab (or type `OA_BATCHLAYERS`).
2. **Select Files** or **Select Folder** of DWGs.
3. Click **Scan** to list every layer/linetype found.
4. Set the new **Color / Lineweight / Linetype** per layer (blank = no change).
5. Click **Apply**. A summary reports successes and any skipped files.

## Building from source

The plugin references AutoCAD's managed API assemblies, which come from your local
AutoCAD installation (they are **not** redistributed here).

```sh
dotnet build "OA-Layer Manager/OA.LayerBatcher.csproj" -c Release
```

The build's PostBuild step assembles `OALayerManager.bundle` and deploys it to your
own `%APPDATA%\Autodesk\ApplicationPlugins` so you can test immediately. See
[`CLAUDE.md`](CLAUDE.md) for AutoCAD plugin conventions used in this project.

## Releasing a new version

1. Bump the version in [`OA-Layer Manager/PackageContents.xml`](OA-Layer%20Manager/PackageContents.xml)
   and `installer/OALayerManager.iss`, and add a [`CHANGELOG.md`](CHANGELOG.md) entry.
2. Build the installer (also builds the plugin in Release):
   ```sh
   powershell -ExecutionPolicy Bypass -File installer/build-installer.ps1
   ```
   Output: `installer/Output/OALayerManager-Setup-<version>.exe`.
   Requires [Inno Setup](https://jrsoftware.org/isinfo.php) (`winget install JRSoftware.InnoSetup`).
3. Create a GitHub release (tag e.g. `v1.0.4`) and attach the `.exe`.

## Office network deployment (optional)

For teams on a shared drive who want automatic updates, the [`deploy/`](deploy) folder
contains:

- **`Publish-ToShare.ps1`** — (maintainer) mirrors the built bundle to a network share.
- **`Setup-AutoUpdate.bat` / `.ps1`** — (each colleague, once) registers a per-logon
  task that syncs the bundle from the share into the local plugins folder.

This keeps everyone on the latest version without reinstalling. See comments at the top
of each script. (For most users, the GitHub release installer above is simpler.)

## Troubleshooting

- **The OA Tools tab doesn't appear:** confirm you're on AutoCAD **2021–2026**, then check
  the startup log at `%TEMP%\OA_LayerBatcher.log` — it records what happened during
  load. You can always launch the tool by typing `OA_BATCHLAYERS`.
- **A file is skipped during Apply:** the summary states the reason — usually the file
  is read-only or currently open in AutoCAD.

## License

Released under the [MIT License](LICENSE).
