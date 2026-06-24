# Changelog

All notable changes to this project are documented here.
This project adheres to [Semantic Versioning](https://semver.org/).

## [1.1.0] - 2026-06-24
### Added
- **AutoCAD 2021–2024 support.** The plugin now ships two builds in one bundle — a
  .NET Framework 4.8 build for AutoCAD 2021–2024 and the .NET 8 build for 2025+ —
  and AutoCAD's autoloader picks the correct one per version.

### Changed
- The project multi-targets `net48` and `net8.0-windows`; each references the
  AutoCAD API assemblies from a matching installed version.
- `PackageContents.xml` declares one component per version range (R24.0–R24.3 → net48,
  R25.0+ → net8.0).
- Installer and `Install.bat` now recognize AutoCAD 2021–2026.

## [1.0.3] - 2026-06-12
### Fixed
- Plugin now auto-loads on AutoCAD startup. `PackageContents.xml` was restructured
  to match AutoCAD's autoloader requirements (`AutodeskProduct`/`ProductType`,
  `RuntimeRequirements` at the package level, `Platform="AutoCAD*"`, declared
  command), so the **OA Tools** ribbon tab appears without a manual `NETLOAD`.
- Linetype conversion now applies even when the chosen linetype is not yet present
  in the target drawing (it is loaded from `acad.lin` instead of being skipped).
- Batch processing no longer fails with a cryptic `eFilerError`. `CloseInput()` is
  called before `SaveAs`, and pre-flight checks report read-only / open-in-AutoCAD /
  missing files with actionable messages.

### Changed
- Build deploys a clean bundle containing only the plugin DLL (AutoCAD API
  assemblies are no longer copied), preventing load failures and stale binaries.
- Targets **AutoCAD 2025+** (`R25.0`, .NET 8).

### Added
- Startup diagnostic log at `%TEMP%\OA_LayerBatcher.log` for troubleshooting.
- Distribution tooling: GitHub Releases installer (`Install.bat`) and optional
  network-share auto-update scripts under `deploy/`.
