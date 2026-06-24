using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace OA.LayerBatcher
{
    public class ScanResult
    {
        public IEnumerable<string> Layers { get; set; }
        public IEnumerable<string> Linetypes { get; set; }
        // Bug #5 fix: collect files that failed to scan instead of silently swallowing errors
        public List<string> FailedFiles { get; set; } = new List<string>();
    }

    public class DwgProcessor
    {
        // Bug #2 fix: returns a list of error messages for files that failed (instead of aborting on first error)
        // Bug #5 fix: process all files, collect failures, return them to caller
        public List<string> ProcessFiles(IEnumerable<string> dwgFiles, IEnumerable<LayerMapping> mappings,
            IProgress<(int current, int total, string file)> progress = null)
        {
            var files = new List<string>(dwgFiles);
            var failedFiles = new List<string>();
            int total = files.Count;

            for (int i = 0; i < total; i++)
            {
                // Report BEFORE so user sees "Processing X/Y: filename" while working on it
                progress?.Report((i, total, System.IO.Path.GetFileName(files[i])));
                try
                {
                    ProcessDwg(files[i], mappings);
                }
                catch (System.Exception ex)
                {
                    failedFiles.Add($"{System.IO.Path.GetFileName(files[i])}: {ex.Message}");
                }
            }

            // Report 100% only after all files are processed
            progress?.Report((total, total, "Complete"));
            return failedFiles;
        }

        public ScanResult ScanFilesForLayers(IEnumerable<string> dwgFiles,
            IProgress<(int current, int total, string file)> progress = null)
        {
            var layers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var linetypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failedFiles = new List<string>();
            var files = new List<string>(dwgFiles);
            int total = files.Count;
            int index = 0;

            foreach (string dwgFile in files)
            {
                progress?.Report((index, total, System.IO.Path.GetFileName(dwgFile)));
                index++;
                try
                {
                    using (Database db = new Database(false, true))
                    {
                        db.ReadDwgFile(dwgFile, FileOpenMode.OpenForReadAndReadShare, true, "");
                        db.CloseInput(true); // Safe: read-only, just releasing the file handle

                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                            foreach (ObjectId layerId in lt)
                            {
                                LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);
                                layers.Add(ltr.Name);
                            }

                            LinetypeTable ltt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
                            foreach (ObjectId linetypeId in ltt)
                            {
                                LinetypeTableRecord ltr = (LinetypeTableRecord)tr.GetObject(linetypeId, OpenMode.ForRead);
                                linetypes.Add(ltr.Name);
                            }

                            tr.Commit();
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Bug #5 fix: collect failed files with reason instead of silently ignoring them
                    failedFiles.Add($"{System.IO.Path.GetFileName(dwgFile)}: {ex.Message}");
                }
            }

            progress?.Report((total, total, "Complete"));
            return new ScanResult { Layers = layers, Linetypes = linetypes, FailedFiles = failedFiles };
        }

        private void ProcessDwg(string dwgFile, IEnumerable<LayerMapping> mappings)
        {
            // Pre-flight checks: turn AutoCAD's cryptic eFilerError into an actionable
            // message that tells the user exactly why this file could not be processed.
            if (!File.Exists(dwgFile))
                throw new InvalidOperationException("file not found (was it moved or deleted?)");

            if (File.GetAttributes(dwgFile).HasFlag(FileAttributes.ReadOnly))
                throw new InvalidOperationException("file is read-only — clear the read-only flag, then retry");

            // A .dwl/.dwl2 lock file next to the drawing means it is currently open in AutoCAD.
            if (File.Exists(Path.ChangeExtension(dwgFile, ".dwl")) ||
                File.Exists(Path.ChangeExtension(dwgFile, ".dwl2")))
                throw new InvalidOperationException("file is open in AutoCAD — close the drawing, then retry");

            using (Database db = new Database(false, true))
            {
                db.ReadDwgFile(dwgFile, FileOpenMode.OpenForReadAndWriteNoShare, true, "");
                // Force a full load into memory AND release the file handle. Without this the
                // database keeps the source file open, so SaveAs to the same path fails with
                // eFilerError. (The prior "Bug #8" change wrongly removed this and caused that error.)
                db.CloseInput(true);

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    LinetypeTable ltt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);

                    foreach (var mapping in mappings)
                    {
                        if (string.IsNullOrWhiteSpace(mapping.LayerName))
                            continue;

                        // If nothing is to be changed, skip
                        if (string.IsNullOrWhiteSpace(mapping.ColorName) &&
                            string.IsNullOrWhiteSpace(mapping.LineWeightName) &&
                            string.IsNullOrWhiteSpace(mapping.LinestyleName))
                        {
                            continue;
                        }

                        if (lt.Has(mapping.LayerName))
                        {
                            LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(lt[mapping.LayerName], OpenMode.ForWrite);

                            // Process Color
                            if (!string.IsNullOrWhiteSpace(mapping.ColorName))
                            {
                                var match = Regex.Match(mapping.ColorName, @"^(\d+)");
                                if (match.Success && short.TryParse(match.Groups[1].Value, out short colorIndex))
                                {
                                    ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex);
                                }
                            }

                            // Process LineWeight
                            if (!string.IsNullOrWhiteSpace(mapping.LineWeightName))
                            {
                                ltr.LineWeight = ParseLineWeight(mapping.LineWeightName);
                            }

                            // Process Linetype
                            if (!string.IsNullOrWhiteSpace(mapping.LinestyleName))
                            {
                                // The linetype dropdown is the UNION of linetypes across all scanned
                                // files, so a chosen linetype may not exist in THIS drawing yet. Load
                                // it from acad.lin before assigning instead of silently skipping
                                // (previous bug: linetypes never converted on files that lacked them).
                                if (!ltt.Has(mapping.LinestyleName))
                                {
                                    try { db.LoadLineTypeFile(mapping.LinestyleName, "acad.lin"); }
                                    catch { /* not defined in acad.lin — leave layer unchanged */ }
                                }

                                if (ltt.Has(mapping.LinestyleName))
                                    ltr.LinetypeObjectId = ltt[mapping.LinestyleName];
                            }
                        }
                    }

                    tr.Commit();
                }

                db.SaveAs(dwgFile, DwgVersion.Current);
            }
        }

        private LineWeight ParseLineWeight(string lwName)
        {
            if (lwName == "Default") return LineWeight.ByLineWeightDefault;

            var match = Regex.Match(lwName, @"^(\d+\.\d+)");
            if (match.Success)
            {
                string numStr = match.Groups[1].Value.Replace(".", "");
                if (Enum.TryParse<LineWeight>("LineWeight" + numStr, true, out var lwEnum))
                {
                    return lwEnum;
                }
            }
            return LineWeight.ByLineWeightDefault;
        }
    }
}