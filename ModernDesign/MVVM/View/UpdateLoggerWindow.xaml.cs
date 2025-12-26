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
    public partial class UpdateLoggerWindow : Window
    {
        private readonly string _versionType; // "leuan" or "other"
        private CancellationTokenSource _cancellationTokenSource;
        private readonly HttpClient _httpClient = new HttpClient();
        private string _simsPath = "";
        private readonly string _tempFolder;

        // ✅ LISTA DE ARCHIVOS PARA "OTHER VERSIONS"
        private readonly List<UpdateFile> _otherVersionFiles = new List<UpdateFile>
        {
            new UpdateFile("Update Part 1", "https://www.mediafire.com/file_premium/hnillv3iy986uxn/Other_Files.zip/file"),
            new UpdateFile("Update Part 2", "https://www.mediafire.com/file_premium/m44n1u6c1d0s7un/Delta.zip/file"),
            new UpdateFile("Update Part 3", "https://www.mediafire.com/file_premium/617ntc9sfc5e6py/Data.zip/file"),
            new UpdateFile("Update Part 4", "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip")
        };

        // ✅ LISTA DE ARCHIVOS A ELIMINAR EN Game/Bin
        private readonly List<string> _filesToDelete = new List<string>
        {
            "anadius64.dll",
            "anadius64online.dll",
            "anadius.cfg",
            "check_version.dll",
            "TS4_DX9_x64.exe",
            "TS4_Launcher_x64.exe",
            "TS4_x64.exe",
            "TS4_x64_fpb.exe"
        };

        public UpdateLoggerWindow(string versionType)
        {
            InitializeComponent();
            _versionType = versionType;
            _tempFolder = Path.Combine(Path.GetTempPath(), "LeuansSims4Toolkit");

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
            Loaded += UpdateLoggerWindow_Loaded;
        }

        private void ApplyLanguage()
        {
            bool isSpanish = IsSpanishLanguage();

            if (isSpanish)
            {
                HeaderText.Text = "🔄 Actualizando el juego...";
                SubHeaderText.Text = "Por favor espera mientras se descarga la actualización";
                PathLabelText.Text = "Ubicación de The Sims 4";
                BrowseBtn.Content = "Buscar";
                CancelBtn.Content = "❌ Cancelar";
                StartBtn.Content = "▶️ Iniciar Actualización";
                SpeedLabel.Text = "Velocidad:";
                EtaLabel.Text = "ETA:";
            }
            else
            {
                HeaderText.Text = "🔄 Updating the game...";
                SubHeaderText.Text = "Please wait while the update is being downloaded";
                PathLabelText.Text = "The Sims 4 install location";
                BrowseBtn.Content = "Browse";
                CancelBtn.Content = "❌ Cancel";
                StartBtn.Content = "▶️ Start Update";
                SpeedLabel.Text = "Speed:";
                EtaLabel.Text = "ETA:";
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

        private async void UpdateLoggerWindow_Loaded(object sender, RoutedEventArgs e)
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
                if (Sims4PathFinder.FindSims4Path(out var potentialRoot))
                {
                    Dispatcher.Invoke(() => SetSimsPath(potentialRoot, true));
                }
                else
                {

                    Dispatcher.Invoke(() =>
                    {
                        StatusText.Text = isSpanish
                            ? "  (No encontrado - seleccionar manualmente)"
                            : "  (Not found - select manually)";
                        StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                                .ConvertFromString("#EF4444"));

                        AddLog(isSpanish
                            ? "⚠️ No se pudo detectar automáticamente la carpeta de The Sims 4."
                            : "⚠️ Could not auto-detect The Sims 4 folder.");
                        AddLog(isSpanish
                            ? "Por favor, selecciónala manualmente usando el botón 'Buscar'."
                            : "Please select it manually using the 'Browse' button.");
                    });
                }
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

            StartBtn.IsEnabled = false;
            BrowseBtn.IsEnabled = false;
            ProgressPanel.Visibility = Visibility.Visible;

            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                if (_versionType == "leuan")
                {
                    await StartLeuanDownloadAsync();
                }
                else // "other"
                {
                    await StartOtherVersionsDownloadAsync();
                }
            }
            catch (OperationCanceledException)
            {
                bool isSpanish = IsSpanishLanguage();
                AddLog(isSpanish ? "❌ Actualización cancelada por el usuario." : "❌ Update cancelled by user.");
            }
            catch (Exception ex)
            {
                bool isSpanish = IsSpanishLanguage();
                AddLog(isSpanish ? $"❌ Error: {ex.Message}" : $"❌ Error: {ex.Message}");
                MessageBox.Show(
                    isSpanish
                        ? $"Error durante la actualización:\n\n{ex.Message}"
                        : $"Error during update:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                StartBtn.IsEnabled = true;
                BrowseBtn.IsEnabled = true;
                ProgressPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async Task StartLeuanDownloadAsync()
        {
            bool isSpanish = IsSpanishLanguage();
            string downloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip";
            string tempZipPath = Path.Combine(_tempFolder, "update_leuan.zip");

            // ✅ 1. ELIMINAR ARCHIVOS OBSOLETOS PRIMERO
            await DeleteObsoleteFiles();

            // ✅ 2. DESCARGAR
            AddLog(isSpanish ? "\n📥 Iniciando descarga..." : "\n📥 Starting download...");
            AddLog($"URL: {downloadUrl}");

            await DownloadWithProgressAsync(downloadUrl, tempZipPath, "Leuan's Update", 1, 1);

            // ✅ 3. EXTRAER
            AddLog(isSpanish ? "📦 Extrayendo archivos..." : "📦 Extracting files...");
            ProgressText.Text = isSpanish ? "Extrayendo..." : "Extracting...";

            await Task.Run(() => ExtractZipWithOverwrite(tempZipPath, _simsPath));

            // ✅ 4. LIMPIAR ZIP
            if (File.Exists(tempZipPath))
                File.Delete(tempZipPath);

            AddLog(isSpanish ? "\n✅ Actualización completada exitosamente!" : "\n✅ Update completed successfully!");

            MessageBox.Show(
                isSpanish
                    ? "✅ El juego ha sido actualizado correctamente.\n\nYa puedes cerrar esta ventana y jugar."
                    : "✅ The game has been updated successfully.\n\nYou can now close this window and play.",
                isSpanish ? "Actualización Completada" : "Update Completed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task StartOtherVersionsDownloadAsync()
        {
            bool isSpanish = IsSpanishLanguage();
            int totalFiles = _otherVersionFiles.Count;

            // ✅ 1. ELIMINAR ARCHIVOS OBSOLETOS PRIMERO
            await DeleteObsoleteFiles();

            // ✅ 2. DESCARGAR Y EXTRAER CADA ARCHIVO
            AddLog(isSpanish
                ? $"\n📥 Iniciando descarga de {totalFiles} archivos..."
                : $"\n📥 Starting download of {totalFiles} files...");

            for (int i = 0; i < totalFiles; i++)
            {
                var file = _otherVersionFiles[i];
                int currentIndex = i + 1;

                AddLog($"\n[{currentIndex}/{totalFiles}] {file.Name}");
                AddLog($"URL: {file.Url}");

                string tempZipPath = Path.Combine(_tempFolder, $"update_part{currentIndex}.zip");

                // Descargar
                AddLog(isSpanish ? "📥 Descargando..." : "📥 Downloading...");
                await DownloadWithProgressAsync(file.Url, tempZipPath, file.Name, currentIndex, totalFiles);

                // Extraer
                AddLog(isSpanish ? "📦 Extrayendo..." : "📦 Extracting...");
                ProgressText.Text = isSpanish
                    ? $"Extrayendo {file.Name}... ({currentIndex}/{totalFiles})"
                    : $"Extracting {file.Name}... ({currentIndex}/{totalFiles})";

                await Task.Run(() => ExtractZipWithOverwrite(tempZipPath, _simsPath));

                // Eliminar ZIP
                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                    AddLog(isSpanish ? "🗑️ Archivo temporal eliminado." : "🗑️ Temporary file deleted.");
                }

                AddLog(isSpanish
                    ? $"✅ {file.Name} completado."
                    : $"✅ {file.Name} completed.");
            }

            AddLog(isSpanish
                ? "\n✅ ¡Todas las actualizaciones completadas exitosamente!"
                : "\n✅ All updates completed successfully!");

            // ✅ MOSTRAR POPUP ESPECIAL PARA "OTHER VERSIONS"
            ShowMigrationSuccessPopup();
        }

        // ✅ FUNCIÓN PARA ELIMINAR ARCHIVOS OBSOLETOS (SE EJECUTA ANTES DE DESCARGAR)
        private async Task DeleteObsoleteFiles()
        {
            bool isSpanish = IsSpanishLanguage();

            AddLog(isSpanish
                ? "🗑️ Eliminando archivos obsoletos..."
                : "🗑️ Deleting obsolete files...");

            string binPath = Path.Combine(_simsPath, "Game", "Bin");

            if (!Directory.Exists(binPath))
            {
                AddLog(isSpanish
                    ? "⚠️ No se encontró la carpeta Game/Bin."
                    : "⚠️ Game/Bin folder not found.");
                return;
            }

            int deletedCount = 0;
            int notFoundCount = 0;

            await Task.Run(() =>
            {
                foreach (var fileName in _filesToDelete)
                {
                    string filePath = Path.Combine(binPath, fileName);

                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            deletedCount++;
                        }
                        else
                        {
                            notFoundCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => AddLog($"  ❌ Error updating to a new version, ask the Chatbot for the Discord, and ask for help with this error: {fileName}: {ex.Message}"));
                    }
                }
            });

            AddLog(isSpanish
                ? $"✅ Limpieza completada: {deletedCount} archivos eliminados, {notFoundCount} no encontrados."
                : $"✅ Cleanup completed: {deletedCount} files deleted, {notFoundCount} not found.");
        }

        private void ShowMigrationSuccessPopup()
        {
            bool isSpanish = IsSpanishLanguage();

            string message = isSpanish
                ? "✅ Tu juego ha sido actualizado.\n\n" +
                  "De ahora en adelante, para mantenerte actualizado en el futuro, " +
                  "deberás elegir 'Leuan's Version'.\n\n" +
                  "Has migrado exitosamente desde versiones desactualizadas a la mejor " +
                  "y más recomendada versión.\n\n" +
                  "¡Felicidades! Tu juego ahora está usando la Leuan's Version!"
                : "✅ Your game has been updated.\n\n" +
                  "From now on to keep updated in the future you'll have to choose 'Leuan's Version'.\n\n" +
                  "You have migrated successfully from outdated versions to the best " +
                  "and most recommended version.\n\n" +
                  "Congrats! Your game is now using the Leuan's Version!";

            string title = isSpanish ? "🎉 Migración Exitosa" : "🎉 Migration Successful";

            MessageBox.Show(
                message,
                title,
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

    // ✅ CLASE AUXILIAR PARA ARCHIVOS DE ACTUALIZACIÓN
    public class UpdateFile
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public UpdateFile(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}