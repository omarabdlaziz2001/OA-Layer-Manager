using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using System;

[assembly: CommandClass(typeof(OA.LayerBatcher.Commands))]

namespace OA.LayerBatcher
{
    public class Commands
    {
        [CommandMethod("OA_BATCHLAYERS")]
        public void ShowBatchLayersWindow()
        {
            try
            {
                MainWindow win = new MainWindow();
                Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(win);
            }
            catch (System.Exception ex)
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage("\nError: " + ex.Message);
                }
            }
        }
    }
}