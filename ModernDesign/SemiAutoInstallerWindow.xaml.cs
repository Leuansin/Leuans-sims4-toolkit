using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernDesign.Core;
using ModernDesign.MVVM.View; // or whatever namespace DLCUnlockerWindow is in

namespace ModernDesign.MVVM.View
{
    public partial class SemiAutoInstallerWindow : Window
    {
        private string _downloadFolderPath = "";
        private string _simsPath = "";
        private readonly List<ArchiveFileInfo> _detectedArchives = new List<ArchiveFileInfo>();
        private readonly string _tempExtractionFolder;

        // 7-Zip path (only for r a r files)
        private const string SevenZipPath = @"C:\Program Files\7-Zip\7z.exe";

        public SemiAutoInstallerWindow()
        {
            InitializeComponent();

            // Temp folder for extraction
            _tempExtractionFolder = Path.Combine(Path.GetTempPath(), "LeuansSims4Toolkit_SemiAuto");

            if (!Directory.Exists(_tempExtractionFolder))
            {
                Directory.CreateDirectory(_tempExtractionFolder);
                try
                {
                    var di = new DirectoryInfo(_tempExtractionFolder);
                    di.Attributes |= FileAttributes.Hidden;
                }
                catch
                {
                    // If we can't mark it as hidden, it's not critical
                }
            }

            Loaded += async (s, e) =>
            {
                await AutoDetectSimsPath();
                ApplyLanguage();
            };
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

        private void ApplyLanguage()
        {
            bool isSpanish = IsSpanishLanguage();

            if (isSpanish)
            {
                Step1Title.Text = "Paso 1: Seleccionar Carpeta de Descargas";
                Step1Description.Text = "Selecciona la carpeta que contiene tus archivos DLC descargados (.zip)";
                DownloadPathText.Text = "No se ha seleccionado ninguna carpeta...";
                BrowseDownloadBtn.Content = "📁 Explorar";

                Step2Title.Text = "Paso 2: Seleccionar Ubicación de Sims 4";
                Step2Description.Text = "Selecciona la carpeta de instalación de The Sims 4";
                Sims4PathText.Text = "Selecciona la carpeta de instalación de The Sims 4...";
                BrowseSims4Btn.Content = "Explorar";
                AutoDetectBtn.Content = "🔍 Auto";

                DetectedFilesTitle.Text = "Archivos Detectados";
                ProgressTitle.Text = "Progreso de Instalación";
                LogTitle.Text = "Registro de Instalación";
                InstalledDLCsTitle.Text = "DLCs Instalados";

                InstallBtn.Content = "🚀  Iniciar Instalación";
            }
        }

        // === STEP 1: SELECT DOWNLOAD FOLDER ===

        private void BrowseDownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = IsSpanishLanguage()
                    ? "Selecciona la carpeta con los DLCs descargados"
                    : "Select the folder containing downloaded DLCs",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SetDownloadFolder(dialog.SelectedPath);
            }
        }

        private void SetDownloadFolder(string path)
        {
            _downloadFolderPath = path;
            DownloadPathText.Text = path;
            DownloadPathText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#F8FAFC"));

            // Scan for archives
            ScanForArchives();
            CheckIfReadyToInstall();
        }

        private void ScanForArchives()
        {
            _detectedArchives.Clear();
            DetectedFilesList.Children.Clear();

            if (string.IsNullOrEmpty(_downloadFolderPath) || !Directory.Exists(_downloadFolderPath))
                return;

            try
            {
                // Search for .zip and .rar files
                var zipFiles = Directory.GetFiles(_downloadFolderPath, "*.zip", SearchOption.AllDirectories);
                var rarFiles = Directory.GetFiles(_downloadFolderPath, "*.rar", SearchOption.AllDirectories);

                var allArchives = zipFiles.Concat(rarFiles).ToList();

                foreach (var archivePath in allArchives)
                {
                    var fileInfo = new FileInfo(archivePath);
                    var archiveInfo = new ArchiveFileInfo
                    {
                        FullPath = archivePath,
                        FileName = fileInfo.Name,
                        SizeInMB = fileInfo.Length / (1024.0 * 1024.0),
                        IsRar = archivePath.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)
                    };

                    _detectedArchives.Add(archiveInfo);

                    // Add to UI
                    var fileCard = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15FFFFFF")),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    var icon = new TextBlock
                    {
                        Text = archiveInfo.IsRar ? "📦" : "🗜️",
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0)
                    };

                    var nameText = new TextBlock
                    {
                        Text = archiveInfo.FileName,
                        Foreground = Brushes.White,
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 300,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    };

                    var sizeText = new TextBlock
                    {
                        Text = $"{archiveInfo.SizeInMB:F2} MB",
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                        FontSize = 11,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(10, 0, 0, 0)
                    };

                    stackPanel.Children.Add(icon);
                    stackPanel.Children.Add(nameText);
                    stackPanel.Children.Add(sizeText);

                    fileCard.Child = stackPanel;
                    DetectedFilesList.Children.Add(fileCard);
                }

                // Update count
                bool isSpanish = IsSpanishLanguage();
                DetectedCountText.Text = isSpanish
                    ? $" ({_detectedArchives.Count} archivos encontrados)"
                    : $" ({_detectedArchives.Count} archives found)";

                DetectedFilesPanel.Visibility = _detectedArchives.Count > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LogMessage($"Error scanning archives: {ex.Message}");
            }
        }

        // === STEP 2: SELECT SIMS 4 PATH ===

        private async Task AutoDetectSimsPath()
        {
            bool isSpanish = IsSpanishLanguage();
            Sims4StatusText.Text = isSpanish ? "  (Buscando...)" : "  (Searching...)";

            await Task.Run(() =>
            {

                if (Sims4PathFinder.FindSims4Path(out var rootPath))
                {
                    Dispatcher.Invoke(() => SetSimsPath(rootPath, true));
                }
                
                Dispatcher.Invoke(() =>
                {
                    Sims4StatusText.Text = isSpanish
                        ? "  (No encontrado - seleccionar manualmente)"
                        : "  (Not found - select manually)";
                    Sims4StatusText.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EF4444"));
                });
            });
        }

        private void SetSimsPath(string path, bool autoDetected = false)
        {
            _simsPath = path;
            Sims4PathText.Text = path;
            Sims4PathText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#F8FAFC"));

            bool isSpanish = IsSpanishLanguage();
            Sims4StatusText.Text = autoDetected
                ? (isSpanish ? "  (✓ Auto-detectado)" : "  (✓ Auto-detected)")
                : (isSpanish ? "  (✓ Seleccionado)" : "  (✓ Selected)");
            Sims4StatusText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#22C55E"));

            CheckIfReadyToInstall();
        }

        private void BrowseSims4Btn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = IsSpanishLanguage()
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
                    bool isSpanish = IsSpanishLanguage();
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

        private async void AutoDetectBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoDetectBtn.IsEnabled = false;
            await AutoDetectSimsPath();
            AutoDetectBtn.IsEnabled = true;
        }

        // === INSTALLATION LOGIC ===

        private void CheckIfReadyToInstall()
        {
            InstallBtn.IsEnabled = !string.IsNullOrEmpty(_downloadFolderPath) &&
                                   !string.IsNullOrEmpty(_simsPath) &&
                                   _detectedArchives.Count > 0;
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isSpanish = IsSpanishLanguage();

            // Disable controls
            InstallBtn.IsEnabled = false;
            BrowseDownloadBtn.IsEnabled = false;
            BrowseSims4Btn.IsEnabled = false;
            AutoDetectBtn.IsEnabled = false;

            // Show progress
            ProgressPanel.Visibility = Visibility.Visible;
            LogPanel.Visibility = Visibility.Visible;
            LogTextBox.Clear();

            int totalArchives = _detectedArchives.Count;
            int processedCount = 0;
            var errors = new List<string>();

            try
            {
                foreach (var archive in _detectedArchives)
                {
                    processedCount++;
                    UpdateProgress(archive.FileName, processedCount, totalArchives);

                    try
                    {
                        LogMessage(isSpanish
                            ? $"[{processedCount}/{totalArchives}] Procesando: {archive.FileName}"
                            : $"[{processedCount}/{totalArchives}] Processing: {archive.FileName}");

                        // Extract archive
                        await ExtractArchive(archive);

                        // Move extracted content to Sims folder
                        await MoveExtractedContent();

                        LogMessage(isSpanish
                            ? $"✓ Completado: {archive.FileName}"
                            : $"✓ Completed: {archive.FileName}");
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{archive.FileName}: {ex.Message}");
                        LogMessage($"✗ Error: {archive.FileName} - {ex.Message}");
                    }
                }

                // Show installed DLCs
                ShowInstalledDLCs();

                // Final message
                if (errors.Count == 0)
                {
                    MessageBox.Show(
                        isSpanish
                            ? $"¡Instalación completada exitosamente!\n\n" +
                              $"Se procesaron {totalArchives} archivo(s).\n\n" +
                              $"Por favor verifica tu instalación y procede a instalar el DLC Unlocker."
                            : $"Installation completed successfully!\n\n" +
                              $"Processed {totalArchives} file(s).\n\n" +
                              $"Please verify your installation and proceed to install the DLC Unlocker.",
                        isSpanish ? "Instalación Completada" : "Installation Completed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Ask about DLC Unlocker
                    var result = MessageBox.Show(
                        isSpanish
                            ? "¿Deseas instalar el EA DLC Unlocker ahora?\n\n" +
                              "Esto es necesario para que los DLCs funcionen correctamente."
                            : "Would you like to install the EA DLC Unlocker now?\n\n" +
                              "This is required for DLCs to work properly.",
                        isSpanish ? "DLC Unlocker" : "DLC Unlocker",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        OpenDLCUnlockerWindow();
                    }
                }
                else
                {
                    var errorText = string.Join(Environment.NewLine + " - ", errors);
                    MessageBox.Show(
                        isSpanish
                            ? $"Instalación completada con errores:\n\n - {errorText}"
                            : $"Installation completed with errors:\n\n - {errorText}",
                        isSpanish ? "Completado con errores" : "Completed with errors",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    isSpanish
                        ? $"Error crítico durante la instalación:\n\n{ex.Message}"
                        : $"Critical error during installation:\n\n{ex.Message}",
                    isSpanish ? "Error Crítico" : "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable controls
                InstallBtn.IsEnabled = true;
                BrowseDownloadBtn.IsEnabled = true;
                BrowseSims4Btn.IsEnabled = true;
                AutoDetectBtn.IsEnabled = true;
            }
        }

        private async Task ExtractArchive(ArchiveFileInfo archive)
        {
            bool isSpanish = IsSpanishLanguage();

            // Clean temp folder first
            if (Directory.Exists(_tempExtractionFolder))
            {
                Directory.Delete(_tempExtractionFolder, true);
            }
            Directory.CreateDirectory(_tempExtractionFolder);

            if (archive.IsRar)
            {
                // Use 7-Zip for .rar files
                if (!File.Exists(SevenZipPath))
                {
                    throw new FileNotFoundException(
                        isSpanish
                            ? "7-Zip no está instalado. Por favor instala 7-Zip para extraer archivos .rar"
                            : "7-Zip is not installed. Please install 7-Zip to extract .rar files");
                }

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = SevenZipPath,
                        Arguments = $"x \"{archive.FullPath}\" -o\"{_tempExtractionFolder}\" -y",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        if (process.ExitCode != 0)
                        {
                            throw new Exception(
                                isSpanish
                                    ? $"Error al extraer archivo RAR (código: {process.ExitCode})"
                                    : $"Failed to extract RAR file (code: {process.ExitCode})");
                        }
                    }
                });
            }
            else
            {
                // Use native .NET for .zip files
                await Task.Run(() =>
                {
                    using (ZipArchive zip = ZipFile.OpenRead(archive.FullPath))
                    {
                        foreach (ZipArchiveEntry entry in zip.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            string destinationPath = Path.Combine(_tempExtractionFolder, entry.FullName);
                            string directoryPath = Path.GetDirectoryName(destinationPath);

                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }

                            entry.ExtractToFile(destinationPath, overwrite: true);
                        }
                    }
                });
            }
        }

        private async Task MoveExtractedContent()
        {
            await Task.Run(() =>
            {
                // Find all folders in temp extraction folder
                var extractedFolders = Directory.GetDirectories(_tempExtractionFolder);

                foreach (var folder in extractedFolders)
                {
                    var folderName = Path.GetFileName(folder);
                    var destinationPath = Path.Combine(_simsPath, folderName);

                    // Move folder to Sims directory
                    if (Directory.Exists(destinationPath))
                    {
                        // Merge folders
                        CopyDirectory(folder, destinationPath, true);
                        Directory.Delete(folder, true);
                    }
                    else
                    {
                        Directory.Move(folder, destinationPath);
                    }
                }
            });
        }

        private void CopyDirectory(string sourceDir, string destDir, bool overwrite)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                return;

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, overwrite);
            }
        }

        private void ShowInstalledDLCs()
        {
            InstalledDLCsList.Children.Clear();

            var dlcList = UpdaterWindow.GetDLCList();
            int installedCount = 0;

            foreach (var dlc in dlcList)
            {
                if (IsDlcInstalled(dlc))
                {
                    installedCount++;

                    var dlcCard = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15FFFFFF")),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 4, 0, 4)
                    };

                    var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

                    var checkIcon = new TextBlock
                    {
                        Text = "✅",
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 10, 0)
                    };

                    var nameText = new TextBlock
                    {
                        Text = $"{dlc.Id} - {dlc.Name}",
                        Foreground = Brushes.White,
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(checkIcon);
                    stackPanel.Children.Add(nameText);

                    dlcCard.Child = stackPanel;
                    InstalledDLCsList.Children.Add(dlcCard);
                }
            }

            bool isSpanish = IsSpanishLanguage();
            InstalledDLCsCount.Text = isSpanish
                ? $" ({installedCount} instalados)"
                : $" ({installedCount} installed)";

            InstalledDLCsPanel.Visibility = installedCount > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private bool IsDlcInstalled(DLCInfo dlc)
        {
            if (string.IsNullOrEmpty(_simsPath) || dlc == null || string.IsNullOrWhiteSpace(dlc.Id))
                return false;

            try
            {
                string rootDlcFolder = Path.Combine(_simsPath, dlc.Id);
                bool rootExists = Directory.Exists(rootDlcFolder);

                string installerDlcFolder = Path.Combine(_simsPath, "__Installer", "DLC", dlc.Id);
                bool installerExists = Directory.Exists(installerDlcFolder);

                return rootExists && installerExists;
            }
            catch
            {
                return false;
            }
        }

        // === UI HELPERS ===

        private void UpdateProgress(string fileName, int current, int total)
        {
            Dispatcher.Invoke(() =>
            {
                bool isSpanish = IsSpanishLanguage();

                ProgressText.Text = isSpanish
                    ? $"Procesando {fileName}..."
                    : $"Processing {fileName}...";

                double percent = (current / (double)total) * 100;
                ProgressPercent.Text = $"{percent:F0}%";

                double totalWidth = ProgressPanel.ActualWidth > 0 ? ProgressPanel.ActualWidth : 400;
                ProgressBar.Width = (percent / 100.0) * totalWidth;

                CurrentFileText.Text = fileName;
                ProcessedCountText.Text = $"{current}/{total}";
            });
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}" + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private void OpenDLCUnlockerWindow()
        {
            try
            {
                var DLCUnlockerWindow = new DLCUnlockerWindow
                {
                    Owner = Application.Current.MainWindow
                };

                var fadeOut = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };

                fadeOut.Completed += (s, args) =>
                {
                    this.Close();

                    DLCUnlockerWindow.Opacity = 0;
                    DLCUnlockerWindow.Show();

                    var fadeIn = new DoubleAnimation
                    {
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(200)
                    };
                    DLCUnlockerWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
                };

                this.BeginAnimation(Window.OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                bool isSpanish = IsSpanishLanguage();
                MessageBox.Show(
                    isSpanish
                        ? $"Error al abrir el instalador del DLC Unlocker:\n{ex.Message}"
                        : $"Error opening DLC Unlocker installer:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // === NAVIGATION ===

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current?.MainWindow;

                // Si no hay ventana principal o somos nosotros mismos, cerrar directamente
                if (mainWindow == null || mainWindow == this || mainWindow.IsLoaded == false)
                {
                    var simpleFade = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };
                    simpleFade.Completed += (s, args) => { try { this.Close(); } catch { } };
                    this.BeginAnimation(Window.OpacityProperty, simpleFade);
                    return;
                }

                var fadeOut = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };

                fadeOut.Completed += (s, args) =>
                {
                    try
                    {
                        // Verificar nuevamente antes de manipular mainWindow
                        if (mainWindow != null && !mainWindow.IsLoaded)
                        {
                            this.Close();
                            return;
                        }

                        this.Hide();

                        if (mainWindow != null)
                        {
                            mainWindow.Opacity = 0;
                            mainWindow.Show();

                            var fadeIn = new DoubleAnimation { To = 1, Duration = TimeSpan.FromMilliseconds(200) };
                            fadeIn.Completed += (s2, args2) =>
                            {
                                try { this.Close(); } catch { }
                            };

                            try
                            {
                                mainWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
                            }
                            catch
                            {
                                this.Close();
                            }
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    catch
                    {
                        try { this.Close(); } catch { }
                    }
                };

                this.BeginAnimation(Window.OpacityProperty, fadeOut);
            }
            catch
            {
                try { this.Close(); } catch { }
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current?.MainWindow;

                // Si no hay ventana principal o somos nosotros mismos, cerrar directamente
                if (mainWindow == null || mainWindow == this || mainWindow.IsLoaded == false)
                {
                    var simpleFade = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };
                    simpleFade.Completed += (s, args) => { try { this.Close(); } catch { } };
                    this.BeginAnimation(Window.OpacityProperty, simpleFade);
                    return;
                }

                var fadeOut = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };

                fadeOut.Completed += (s, args) =>
                {
                    try
                    {
                        // Verificar nuevamente antes de manipular mainWindow
                        if (mainWindow != null && !mainWindow.IsLoaded)
                        {
                            this.Close();
                            return;
                        }

                        this.Hide();

                        if (mainWindow != null)
                        {
                            mainWindow.Opacity = 0;
                            mainWindow.Show();

                            var fadeIn = new DoubleAnimation { To = 1, Duration = TimeSpan.FromMilliseconds(200) };
                            fadeIn.Completed += (s2, args2) =>
                            {
                                try { this.Close(); } catch { }
                            };

                            try
                            {
                                mainWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
                            }
                            catch
                            {
                                this.Close();
                            }
                        }
                        else
                        {
                            this.Close();
                        }
                    }
                    catch
                    {
                        try { this.Close(); } catch { }
                    }
                };

                this.BeginAnimation(Window.OpacityProperty, fadeOut);
            }
            catch
            {
                try { this.Close(); } catch { }
            }
        }
    }

    // === HELPER CLASSES ===

    public class ArchiveFileInfo
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public double SizeInMB { get; set; }
        public bool IsRar { get; set; }
    }

}