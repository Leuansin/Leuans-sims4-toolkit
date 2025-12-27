using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ModernDesign.Core;

namespace ModernDesign.MVVM.View
{
    public partial class RepairLoggerWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly HttpClient _httpClient = new HttpClient();
        private string _simsPath = "";
        private readonly string _tempFolder;

        // ✅ LISTA DE COMPONENTES PARA REPARACIÓN
        private readonly Dictionary<string, RepairComponent> _repairComponents = new Dictionary<string, RepairComponent>
        {
            { "__Installer", new RepairComponent("__Installer", "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/BaseGame/__Installer.zip") },
            { "Data", new RepairComponent("Data", "https://www.mediafire.com/file_premium/617ntc9sfc5e6py/Data.zip/file") },
            { "Delta", new RepairComponent("Delta", "https://www.mediafire.com/file_premium/m44n1u6c1d0s7un/Delta.zip/file") },
            { "Game", new RepairComponent("Game", "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip") },
            { "Support", new RepairComponent("Support", "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/BaseGame/Support.zip") }
        };

        public RepairLoggerWindow()
        {
            InitializeComponent();
            _tempFolder = Path.Combine(Path.GetTempPath(), "LeuansSims4Toolkit_Repair");

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
                try
                {
                    var di = new DirectoryInfo(_tempFolder);
                    di.Attributes |= FileAttributes.Hidden;
                }
                catch { }
            }

            ApplyLanguage();
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
            Loaded += RepairLoggerWindow_Loaded;
        }

        private void ApplyLanguage()
        {
            bool isSpanish = IsSpanishLanguage();

            if (isSpanish)
            {
                HeaderText.Text = "🔧 Reparando el juego...";
                SubHeaderText.Text = "Selecciona los componentes que deseas reparar";
                PathLabelText.Text = "Ubicación de The Sims 4";
                BrowseBtn.Content = "Buscar";
                CancelBtn.Content = "❌ Cancelar";
                StartBtn.Content = "🔧 Iniciar Reparación";
                SpeedLabel.Text = "Velocidad:";
                EtaLabel.Text = "ETA:";
                ComponentsHeaderText.Text = "📦 Selecciona componentes a reparar:";
            }
            else
            {
                HeaderText.Text = "🔧 Repairing the game...";
                SubHeaderText.Text = "Select the components you want to repair";
                PathLabelText.Text = "The Sims 4 install location";
                BrowseBtn.Content = "Browse";
                CancelBtn.Content = "❌ Cancel";
                StartBtn.Content = "🔧 Start Repair";
                SpeedLabel.Text = "Speed:";
                EtaLabel.Text = "ETA:";
                ComponentsHeaderText.Text = "📦 Select components to repair:";
            }
        }

        private static bool IsSpanishLanguage()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string languagePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "language.ini");

                if (!File.Exists(languagePath))
                    return false;

                var lines = File.ReadAllLines(languagePath);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("Language") && trimmed.Contains("="))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length == 2)
                        {
                            return parts[1].Trim().ToLower().Contains("es");
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async void RepairLoggerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            bool isSpanish = IsSpanishLanguage();
            StatusText.Text = isSpanish ? "  (Buscando automáticamente...)" : "  (Searching automatically...)";

            await AutoDetectSimsPath();
        }

        private async Task AutoDetectSimsPath()
        {
            bool isSpanish = IsSpanishLanguage();

            await Task.Run(() =>
            {
                if (Sims4PathFinder.FindSims4Path(out var rootPath))
                {
                    Dispatcher.Invoke(() => SetSimsPath(rootPath, true));
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = isSpanish
                        ? "  (No encontrado - seleccionar manualmente)"
                        : "  (Not found - select manually)";
                    StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EF4444"));

                    AddLog(isSpanish
                        ? "⚠️ No se pudo detectar automáticamente la carpeta de The Sims 4."
                        : "⚠️ Could not auto-detect The Sims 4 folder.");
                    AddLog(isSpanish
                        ? "Por favor, selecciónala manualmente usando el botón 'Buscar'."
                        : "Please select it manually using the 'Browse' button.");
                });
            });
        }

        private void SetSimsPath(string path, bool autoDetected = false)
        {
            bool isSpanish = IsSpanishLanguage();
            _simsPath = path;
            PathTextBlock.Text = path;
            PathTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8FAFC"));

            StatusText.Text = autoDetected
                ? (isSpanish ? "  (✓ Auto-detectado)" : "  (✓ Auto-detected)")
                : (isSpanish ? "  (✓ Seleccionado)" : "  (✓ Selected)");
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#22C55E"));

            StartBtn.IsEnabled = true;

            AddLog(isSpanish
                ? $"✅ Carpeta de The Sims 4 detectada: {path}"
                : $"✅ The Sims 4 folder detected: {path}");
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isSpanish = IsSpanishLanguage();

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = isSpanish
                    ? "Selecciona la carpeta de instalación de The Sims 4"
                    : "Select The Sims 4 install folder",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var exePath = Path.Combine(dialog.SelectedPath, "Game", "Bin", "TS4_x64.exe");
                if (File.Exists(exePath) || Directory.Exists(Path.Combine(dialog.SelectedPath, "Data")))
                {
                    SetSimsPath(dialog.SelectedPath);
                }
                else
                {
                    MessageBox.Show(
                        isSpanish
                            ? "La carpeta seleccionada no parece ser una instalación válida de The Sims 4.\n\n" +
                              "Por favor selecciona la carpeta que contiene las subcarpetas 'Game' y 'Data'."
                            : "The selected folder does not look like a valid The Sims 4 installation.\n\n" +
                              "Please select the folder that contains the 'Game' and 'Data' subfolders.",
                        isSpanish ? "Ruta inválida" : "Invalid path",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_simsPath))
                return;

            // Verificar que al menos un componente esté seleccionado
            var selectedComponents = GetSelectedComponents();
            if (selectedComponents.Count == 0)
            {
                bool isSpanish = IsSpanishLanguage();
                MessageBox.Show(
                    isSpanish
                        ? "Por favor selecciona al menos un componente para reparar."
                        : "Please select at least one component to repair.",
                    isSpanish ? "Sin componentes seleccionados" : "No components selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            StartBtn.IsEnabled = false;
            BrowseBtn.IsEnabled = false;
            DataCheckBox.IsEnabled = false;
            DeltaCheckBox.IsEnabled = false;
            InstallerCheckBox.IsEnabled = false;
            SupportCheckBox.IsEnabled = false;
            GameCheckBox.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await StartRepairAsync(selectedComponents);
            }
            catch (OperationCanceledException)
            {
                bool isSpanish = IsSpanishLanguage();
                AddLog(isSpanish ? "❌ Reparación cancelada por el usuario." : "❌ Repair cancelled by user.");
            }
            catch (Exception ex)
            {
                bool isSpanish = IsSpanishLanguage();
                AddLog(isSpanish ? $"❌ Error: {ex.Message}" : $"❌ Error: {ex.Message}");
                MessageBox.Show(
                    isSpanish
                        ? $"Error durante la reparación:\n\n{ex.Message}"
                        : $"Error during repair:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                StartBtn.IsEnabled = true;
                BrowseBtn.IsEnabled = true;
                DataCheckBox.IsEnabled = true;
                DeltaCheckBox.IsEnabled = true;
                InstallerCheckBox.IsEnabled = true;
                SupportCheckBox.IsEnabled = true;
                GameCheckBox.IsEnabled = true;
                ProgressPanel.Visibility = Visibility.Collapsed;
            }
        }

        private List<RepairComponent> GetSelectedComponents()
        {
            var selected = new List<RepairComponent>();

            if (DataCheckBox.IsChecked == true)
                selected.Add(_repairComponents["Data"]);
            if (DeltaCheckBox.IsChecked == true)
                selected.Add(_repairComponents["Delta"]);
            if (InstallerCheckBox.IsChecked == true)
                selected.Add(_repairComponents["__Installer"]);
            if (SupportCheckBox.IsChecked == true)
                selected.Add(_repairComponents["Support"]);
            if (GameCheckBox.IsChecked == true)
                selected.Add(_repairComponents["Game"]);

            return selected;
        }

        private async Task StartRepairAsync(List<RepairComponent> components)
        {
            bool isSpanish = IsSpanishLanguage();
            int totalFiles = components.Count;

            AddLog(isSpanish
                ? $"\n🔧 Iniciando reparación de {totalFiles} componente(s)..."
                : $"\n🔧 Starting repair of {totalFiles} component(s)...");

            for (int i = 0; i < totalFiles; i++)
            {
                var component = components[i];
                int currentIndex = i + 1;

                AddLog($"\n[{currentIndex}/{totalFiles}] {component.Name}");
                AddLog($"URL: {component.Url}");

                string tempZipPath = Path.Combine(_tempFolder, $"repair_{component.Name}.zip");

                // Descargar
                AddLog(isSpanish ? "📥 Descargando..." : "📥 Downloading...");
                await DownloadWithProgressAsync(component.Url, tempZipPath, component.Name, currentIndex, totalFiles);

                // Extraer
                AddLog(isSpanish ? "📦 Extrayendo..." : "📦 Extracting...");
                ProgressText.Text = isSpanish
                    ? $"Extrayendo {component.Name}... ({currentIndex}/{totalFiles})"
                    : $"Extracting {component.Name}... ({currentIndex}/{totalFiles})";

                await Task.Run(() => ExtractZipWithOverwrite(tempZipPath, _simsPath));

                // Eliminar ZIP
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                    AddLog(isSpanish ? "🗑️ Archivo temporal eliminado." : "🗑️ Temporary file deleted.");
                }

                AddLog(isSpanish
                    ? $"✅ {component.Name} reparado exitosamente."
                    : $"✅ {component.Name} repaired successfully.");
            }

            AddLog(isSpanish
                ? "\n✅ ¡Reparación completada exitosamente!"
                : "\n✅ Repair completed successfully!");

            MessageBox.Show(
                isSpanish
                    ? "✅ Los componentes seleccionados han sido reparados correctamente.\n\nYa puedes cerrar esta ventana y jugar."
                    : "✅ The selected components have been repaired successfully.\n\nYou can now close this window and play.",
                isSpanish ? "Reparación Completada" : "Repair Completed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task DownloadWithProgressAsync(string url, string destinationPath, string fileName, int currentIndex, int totalCount)
        {
            using (var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[81920];
                long totalRead = 0;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var sw = Stopwatch.StartNew();
                    long lastBytesRead = 0;

                    int read;
                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read, _cancellationTokenSource.Token);
                        totalRead += read;

                        if (sw.ElapsedMilliseconds >= 500)
                        {
                            UpdateProgress(totalRead, totalBytes, totalRead - lastBytesRead, sw.Elapsed.TotalSeconds, fileName, currentIndex, totalCount);
                            lastBytesRead = totalRead;
                            sw.Restart();
                        }
                    }

                    UpdateProgress(totalBytes, totalBytes, 0, 0, fileName, currentIndex, totalCount);
                }
            }
        }

        private void UpdateProgress(long bytesRead, long totalBytes, long bytesSinceLast, double secondsElapsed, string fileName, int currentIndex, int totalCount)
        {
            Dispatcher.Invoke(() =>
            {
                bool isSpanish = IsSpanishLanguage();

                ProgressText.Text = isSpanish
                    ? $"Descargando {fileName}... ({currentIndex}/{totalCount})"
                    : $"Downloading {fileName}... ({currentIndex}/{totalCount})";

                if (totalBytes > 0)
                {
                    double percent = (bytesRead * 100.0) / totalBytes;
                    ProgressPercent.Text = $"{percent:F0}%";

                    double totalWidth = ProgressPanel.ActualWidth > 0 ? ProgressPanel.ActualWidth : 400;
                    ProgressBar.Width = (percent / 100.0) * totalWidth;

                    if (secondsElapsed > 0 && bytesSinceLast > 0)
                    {
                        double speedMBps = (bytesSinceLast / secondsElapsed) / (1024 * 1024);
                        SpeedText.Text = $"{speedMBps:F2} MB/s";

                        long remainingBytes = totalBytes - bytesRead;
                        if (speedMBps > 0)
                        {
                            double remainingSeconds = remainingBytes / (speedMBps * 1024 * 1024);
                            var eta = TimeSpan.FromSeconds(remainingSeconds);
                            EtaText.Text = $"{eta:mm\\:ss}";
                        }
                    }
                }
            });
        }

        private void ExtractZipWithOverwrite(string zipPath, string destinationPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string destinationFilePath = Path.Combine(destinationPath, entry.FullName);
                    string directoryPath = Path.GetDirectoryName(destinationFilePath);

                    if (!Directory.Exists(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    entry.ExtractToFile(destinationFilePath, overwrite: true);
                }
            }
        }

        private void AddLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogScroller.ScrollToEnd();
            });
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            this.Close();
        }
    }

    // ✅ CLASE AUXILIAR PARA COMPONENTES DE REPARACIÓN
    public class RepairComponent
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public RepairComponent(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}