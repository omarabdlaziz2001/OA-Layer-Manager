using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

namespace OA.LayerBatcher
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<LayerMapping> Mappings { get; set; }
        public ObservableCollection<string> AvailableColors { get; set; }
        public ObservableCollection<string> AvailableLineWeights { get; set; }
        public ObservableCollection<string> AvailableLinetypes { get; set; }
        private List<string> _selectedFiles;
        private ICollectionView _mappingsView;

        public MainWindow()
        {
            InitializeComponent();
            Mappings = new ObservableCollection<LayerMapping>();
            AvailableColors = new ObservableCollection<string>();
            AvailableLineWeights = new ObservableCollection<string>();
            AvailableLinetypes = new ObservableCollection<string>();
            _mappingsView = CollectionViewSource.GetDefaultView(Mappings);
            _mappingsView.Filter = LayerFilter;
            MappingGrid.ItemsSource = _mappingsView;
        }

        private bool LayerFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(SearchBox?.Text)) return true;
            return obj is LayerMapping m && m.LayerName != null &&
                   m.LayerName.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            ClearSearchButton.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Collapsed : Visibility.Visible;
            _mappingsView?.Refresh();
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SearchBox.Focus();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Bug #9 fix: recurse into subfolders with SearchOption.AllDirectories
                    _selectedFiles = new List<string>(
                        Directory.GetFiles(dialog.SelectedPath, "*.dwg", SearchOption.AllDirectories));
                    FolderPathText.Text = $"📁 Folder: {_selectedFiles.Count} .dwg file(s) — hover to see list";
                    FolderPathText.ToolTip = BuildFileTooltip(_selectedFiles);
                }
            }
        }

        private void SelectFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "AutoCAD Drawing (*.dwg)|*.dwg";

            if (dialog.ShowDialog() == true)
            {
                _selectedFiles = new List<string>(dialog.FileNames);
                FolderPathText.Text = $"📄 {_selectedFiles.Count} file(s) selected — hover to see list";
                FolderPathText.ToolTip = BuildFileTooltip(_selectedFiles);
            }
        }

        private string BuildFileTooltip(List<string> files)
        {
            if (files == null || files.Count == 0) return null;
            var lines = files.Select((f, i) => $"{i + 1}. {System.IO.Path.GetFileName(f)}");
            return string.Join("\n", lines);
        }

        private void SetBusy(bool busy)
        {
            ProgressPanel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
            ApplyButton.IsEnabled = !busy;
            ScanButton.IsEnabled = !busy;
            if (!busy)
            {
                ProgressBar.Value = 0;
                ProgressStatusText.Text = "";
            }
        }

        private void ReportProgress((int completed, int total, string file) p)
        {
            double pct = p.total > 0 ? (double)p.completed / p.total * 100 : 0;
            ProgressBar.Value = pct;

            // Bug #4 fix: completed is 0-based file index, so +1 gives correct 1-based display
            if (p.completed < p.total)
                ProgressStatusText.Text = $"Working on {p.completed + 1}/{p.total}: {p.file}";
            else
                ProgressStatusText.Text = "Complete!";
        }

        private async void Scan_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFiles == null || _selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select files or a folder first.");
                return;
            }

            SetBusy(true);
            ProgressStatusText.Text = "Scanning files...";

            ScanResult scanResult;
            try
            {
                var progress = new Progress<(int, int, string)>(ReportProgress);
                var processor = new DwgProcessor();
                var files = new List<string>(_selectedFiles);
                scanResult = await Task.Run(() => processor.ScanFilesForLayers(files, progress));

                Mappings.Clear();
                foreach (var layer in scanResult.Layers)
                {
                    Mappings.Add(new LayerMapping { LayerName = layer, LinestyleName = "" });
                }

                AvailableColors.Clear();
                AvailableColors.Add(""); // blank = no change
                AvailableColors.Add("1 - Red");
                AvailableColors.Add("2 - Yellow");
                AvailableColors.Add("3 - Green");
                AvailableColors.Add("4 - Cyan");
                AvailableColors.Add("5 - Blue");
                AvailableColors.Add("6 - Magenta");
                AvailableColors.Add("7 - White/Black");

                AvailableLineWeights.Clear();
                AvailableLineWeights.Add(""); // blank = no change
                AvailableLineWeights.Add("Default");
                AvailableLineWeights.Add("0.00 mm");
                AvailableLineWeights.Add("0.05 mm");
                AvailableLineWeights.Add("0.09 mm");
                AvailableLineWeights.Add("0.13 mm");
                AvailableLineWeights.Add("0.15 mm");
                AvailableLineWeights.Add("0.18 mm");
                AvailableLineWeights.Add("0.20 mm");
                AvailableLineWeights.Add("0.25 mm");
                AvailableLineWeights.Add("0.30 mm");
                AvailableLineWeights.Add("0.35 mm");
                AvailableLineWeights.Add("0.40 mm");
                AvailableLineWeights.Add("0.50 mm");
                AvailableLineWeights.Add("0.53 mm");
                AvailableLineWeights.Add("0.60 mm");
                AvailableLineWeights.Add("0.70 mm");
                AvailableLineWeights.Add("0.80 mm");
                AvailableLineWeights.Add("0.90 mm");
                AvailableLineWeights.Add("1.00 mm");
                AvailableLineWeights.Add("1.06 mm");
                AvailableLineWeights.Add("1.20 mm");
                AvailableLineWeights.Add("1.40 mm");
                AvailableLineWeights.Add("1.58 mm");
                AvailableLineWeights.Add("2.00 mm");
                AvailableLineWeights.Add("2.11 mm");

                AvailableLinetypes.Clear();
                AvailableLinetypes.Add(""); // blank = no change
                foreach (var ltype in scanResult.Linetypes)
                    AvailableLinetypes.Add(ltype);

                // Bug #7 fix: reference columns by name instead of fragile numeric index
                ColorColumn.ItemsSource = AvailableColors;
                WeightColumn.ItemsSource = AvailableLineWeights;
                LinetypeColumn.ItemsSource = AvailableLinetypes;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error scanning files: " + ex.Message);
                SetBusy(false);
                return;
            }

            SetBusy(false);

            // Bug #5 fix: surface any files that failed to scan
            string msg = $"Scan complete! Found {Mappings.Count} unique layers and {scanResult.Linetypes.Count()} unique linetypes.";
            if (scanResult.FailedFiles.Count > 0)
                msg += $"\n\n⚠ {scanResult.FailedFiles.Count} file(s) could not be read:\n" + string.Join("\n", scanResult.FailedFiles);

            System.Windows.MessageBox.Show(msg);
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFiles == null || _selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select files or a folder first.");
                return;
            }

            SetBusy(true);
            ProgressStatusText.Text = "Applying changes...";

            List<string> failedFiles;
            try
            {
                var progress = new Progress<(int, int, string)>(ReportProgress);
                var processor = new DwgProcessor();
                var files = new List<string>(_selectedFiles);
                // Bug #3 fix: ToList() snapshot here is correct — captures current user selections
                // (now reliable because LayerMapping implements INotifyPropertyChanged)
                var mappings = Mappings.ToList();
                failedFiles = await Task.Run(() => processor.ProcessFiles(files, mappings, progress));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unexpected error: " + ex.Message);
                SetBusy(false);
                return;
            }

            SetBusy(false);

            // Bug #5 fix: report any files that failed to process
            if (failedFiles.Count > 0)
                System.Windows.MessageBox.Show($"Processing complete with {failedFiles.Count} error(s):\n\n" + string.Join("\n", failedFiles));
            else
                System.Windows.MessageBox.Show("Processing complete! All files updated successfully.");
        }
    }

    // Bug #1 fix: LayerMapping now implements INotifyPropertyChanged so DataGrid ComboBox
    // selections are preserved correctly (no silent data loss on grid refresh during search).
    public class LayerMapping : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _layerName;
        public string LayerName
        {
            get => _layerName;
            set { _layerName = value; OnPropertyChanged(nameof(LayerName)); }
        }

        private string _colorName;
        public string ColorName
        {
            get => _colorName;
            set { _colorName = value; OnPropertyChanged(nameof(ColorName)); }
        }

        private string _lineWeightName;
        public string LineWeightName
        {
            get => _lineWeightName;
            set { _lineWeightName = value; OnPropertyChanged(nameof(LineWeightName)); }
        }

        private string _linestyleName;
        public string LinestyleName
        {
            get => _linestyleName;
            set { _linestyleName = value; OnPropertyChanged(nameof(LinestyleName)); }
        }
    }
}