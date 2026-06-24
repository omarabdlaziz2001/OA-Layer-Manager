# AutoCAD .NET Plugin Development Guidelines

## Build & Environment Quirks
* **CRITICAL:** All references to AutoCAD API assemblies (specifically `AcDbMgd.dll`, `AcMgd.dll`, and `AcCoreMgd.dll`) MUST have their **Copy Local property set to False** [3, 4]. Having copies of these in the build output folder causes fatal loading errors [3].
* Target **.NET 8.0** for plugins designed for AutoCAD 2025 and 2026 [4, 5]. 
* When configuring the compiler's output path logic, avoid nesting the plugin DLL inside a "net8.0-windows" subfolder to simplify AutoCAD's Autoloader system [6].

## UI & WPF Integration
* To show WPF modal dialogs inside AutoCAD, ALWAYS use `Application.ShowModalWindow(window)` instead of the standard WPF `ShowDialog()` to ensure AutoCAD handles input focus correctly [7, 8].
* For modeless palettes, implement a `PaletteSet` and wrap your WPF `UserControl` inside an `ElementHost` [9].

## AutoCAD API Best Practices
* Always wrap database operations (reading or modifying drawing entities) within a `Transaction` and ensure the transaction is committed using `tr.Commit()` [10, 11].
* Define custom commands using the `[CommandMethod("COMMANDNAME")]` attribute on public static methods [4, 12].
* Use `Application.DocumentManager.MdiActiveDocument.Editor` to prompt users for input, selections, or to write messages to the command line [13, 14]. 
* For event handlers, ensure they are registered during plugin initialization and correctly unregistered during cleanup to prevent memory leaks [15].

## Deployment & Auto-Loading Rules
* **Autoloader (Preferred):** Deploy the plugin by placing the `.dll` and a properly formatted `PackageContents.xml` file inside a `.bundle` folder in the `%ProgramData%\Autodesk\ApplicationPlugins` directory [16, 17]. 
* **LISP Scripts:** When writing AutoLISP scripts (e.g., `acad.lsp` or `acaddoc.lsp`) to bridge the `NETLOAD` command, ALWAYS use forward slashes (`/`) or double backslashes (`\\`) in file paths. Single backslashes act as escape characters and will cause the load script to fail [18-20].