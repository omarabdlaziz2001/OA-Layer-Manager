using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using System;

[assembly: ExtensionApplication(typeof(OA.LayerBatcher.RibbonLoader))]

namespace OA.LayerBatcher
{
    public class RibbonLoader : IExtensionApplication
    {
        // Diagnostic log to prove whether the plugin auto-loads at AutoCAD startup.
        // Written to %TEMP%\OA_LayerBatcher.log. Never throws.
        private static void Log(string message)
        {
            try
            {
                string path = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(), "OA_LayerBatcher.log");
                System.IO.File.AppendAllText(path,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}  {message}{Environment.NewLine}");
            }
            catch { }
        }

        public void Initialize()
        {
            Log($"Initialize() called. Product={Application.GetSystemVariable("PRODUCT")}, " +
                $"Ribbon={(ComponentManager.Ribbon == null ? "null" : "ready")}");

            if (ComponentManager.Ribbon != null)
                AddRibbon();
            else
                Application.Idle += OnApplicationIdle;
        }

        public void Terminate()
        {
            Log("Terminate() called.");
        }

        private void OnApplicationIdle(object sender, EventArgs e)
        {
            if (ComponentManager.Ribbon == null)
                return; // not ready yet, retry on next idle

            Application.Idle -= OnApplicationIdle;
            Log("Application.Idle: ribbon now ready, building ribbon.");
            AddRibbon();
        }

        private void AddRibbon()
        {
            try
            {
                RibbonControl ribbon = ComponentManager.Ribbon;
                if (ribbon != null)
                {
                    RibbonTab rtb = ribbon.FindTab("OA_TOOLS_TAB");
                    if (rtb != null)
                    {
                        Log("AddRibbon: tab already exists, nothing to do.");
                        return; // Already exists
                    }

                    rtb = new RibbonTab();
                    rtb.Title = "OA Tools";
                    rtb.Id = "OA_TOOLS_TAB";

                    ribbon.Tabs.Add(rtb);

                    RibbonPanelSource rps = new RibbonPanelSource();
                    rps.Title = "Layer Utilities";

                    RibbonPanel rp = new RibbonPanel();
                    rp.Source = rps;
                    rtb.Panels.Add(rp);

                    RibbonButton rb = new RibbonButton();
                    rb.Name = "Batch Layers";
                    rb.ShowText = true;
                    rb.Text = "Batch Layers";
                    rb.ShowImage = true;
                    
                    try
                    {
                        var uri = new Uri("pack://application:,,,/OA.LayerBatcher;component/Resources/icon.png");
                        rb.Image = new System.Windows.Media.Imaging.BitmapImage(uri);
                        rb.LargeImage = new System.Windows.Media.Imaging.BitmapImage(uri);
                        rb.Size = RibbonItemSize.Large;
                    }
                    catch { }

                    // The command parameter requires a trailing space to simulate pressing Enter
                    rb.CommandParameter = "OA_BATCHLAYERS ";
                    rb.CommandHandler = new RibbonCommandHandler();

                    rps.Items.Add(rb);

                    rtb.IsActive = true;
                    Log("AddRibbon: 'OA Tools' tab created successfully.");
                }
            }
            catch (System.Exception ex)
            {
                Log("AddRibbon ERROR: " + ex);
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\nError loading ribbon: {ex.Message}");
            }
        }
    }

    public class RibbonCommandHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            RibbonButton btn = parameter as RibbonButton;
            if (btn != null && btn.CommandParameter != null)
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.SendStringToExecute((string)btn.CommandParameter, true, false, true);
                }
            }
        }
    }
}
