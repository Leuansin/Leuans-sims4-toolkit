using Microsoft.Win32;
using ModernDesign.MVVM.View; // or whatever namespace DLCUnlockerWindow is in
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ModernDesign.Core;

namespace ModernDesign.MVVM.View
{
    public partial class UpdaterWindow : Window
    {
        private List<CheckBox> _allCheckBoxes = new List<CheckBox>();

        private string _simsPath = "";
        private readonly List<DLCInfo> _dlcList;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _tempFolder;
        private readonly LowSpeedDetector _lowSpeedDetector = new LowSpeedDetector();

        private static bool _isDownloading = false;
        private static CancellationTokenSource _downloadCancellation = null;
        private static Action<DownloadProgressInfo> _progressCallback = null;

        // URL of your own unlocker package (.zip with setup.bat/exe + g_The Sims 4.ini, etc.)
        private const string UnlockerPackageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Unlocker.zip";

        public UpdaterWindow()
        {
            InitializeComponent();


            // PROTECCIÓN GLOBAL contra crashes
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            };

            Dispatcher.UnhandledException += (s, e) =>
            {
                Debug.WriteLine($"Dispatcher exception: {e.Exception.Message}");
                e.Handled = true;

                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            };

            // Detectar si hay descargas en progreso al abrir la ventana
            Loaded += async (s, e) =>
            {

                await AutoDetectSimsPath();
                UpdateUnlockerStatus();

                // ✅ DETECTAR SI HAY DESCARGA EN PROGRESO
                if (_isDownloading)
                {
                    bool isSpanish = IsSpanishLanguage();

                    LogTextBox.AppendText(isSpanish
                        ? "⚠️ Descarga en progreso detectada.\n\n" +
                          "Reconectando al progreso de descarga...\n\n"
                        : "⚠️ Download in progress detected.\n\n" +
                          "Reconnecting to download progress...\n\n");

                    // Mostrar panel de progreso
                    ProgressPanel.Visibility = Visibility.Visible;

                    // Reconectar el callback de progreso
                    _progressCallback = UpdateUiProgress;

                    // Deshabilitar controles
                    DownloadBtn.IsEnabled = false;
                    SelectAllBtn.IsEnabled = false;
                    DeselectAllBtn.IsEnabled = false;
                    foreach (CheckBox cb in DLCList.Children) cb.IsEnabled = false;
                }
                else
                {
                    // VERIFICAR Y LIMPIAR ARCHIVOS HUÉRFANOS (OBLIGATORIO)
                    await CheckAndCleanOrphanedFilesOnStartup();
                }
            };

            // ✅ CHECK IF DLC IMAGES ARE DISABLED AND ASK USER
            if (!ShouldLoadDLCImages())
            {
                bool isSpanish = IsSpanishLanguage();

                string message = isSpanish
                    ? "Las imágenes de DLC están actualmente desactivadas.\n\n" +
                      "¿Deseas activarlas?\n\n" +
                      "⚠️ Advertencia: Activar las imágenes puede consumir hasta 1GB de RAM."
                    : "DLC images are currently disabled.\n\n" +
                      "Would you like to enable them?\n\n" +
                      "⚠️ Warning: Enabling images may consume up to 1GB of RAM.";

                string title = isSpanish
                    ? "¿Activar Imágenes de DLC?"
                    : "Enable DLC Images?";

                var result = MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SetLoadDLCImages(true);

                    // ✅ INFORM USER THAT IMAGES WILL ALWAYS LOAD NOW
                    string infoMessage = isSpanish
                        ? "Las imágenes de DLC se cargarán siempre de ahora en adelante.\n\n" +
                          "Si deseas desactivar esta opción, puedes ir a \"Settings\" y desactivarla desde ahí."
                        : "DLC images will now always load from now on.\n\n" +
                          "If you want to disable this option, you can go to \"Settings\" and turn it off there.";

                    string infoTitle = isSpanish
                        ? "Imágenes Activadas"
                        : "Images Enabled";

                    MessageBox.Show(
                        infoMessage,
                        infoTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }

            _dlcList = GetDLCList();
            PopulateDLCList();

            // Temp folder for DLC downloads and unlocker package
            _tempFolder = Path.Combine(Path.GetTempPath(), "LeuansSims4Toolkit");

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
                try
                {
                    var di = new DirectoryInfo(_tempFolder);
                    di.Attributes |= FileAttributes.Hidden;
                }
                catch
                {
                    // If we can't mark it as hidden, it's not critical
                }
            }

            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;

        }

        private async Task CheckAndCleanOrphanedFilesOnStartup()
        {
            try
            {
                if (!Directory.Exists(_tempFolder))
                    return;

                // ✅ Buscar TODOS los archivos
                var allFiles = Directory.GetFiles(_tempFolder);

                // ✅ FILTRAR: Solo archivos que NO terminen en .zip (case-insensitive)
                var orphanedFiles = allFiles
                    .Where(f => !Path.GetFileName(f).EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (orphanedFiles.Count == 0)
                    return; // No hay archivos huérfanos, todo bien

                // ✅ DEBUG: Ver qué archivos se detectaron
                Debug.WriteLine($"🔍 Orphaned files detected: {orphanedFiles.Count}");
                foreach (var file in orphanedFiles)
                {
                    Debug.WriteLine($"  - {Path.GetFileName(file)} (Extension: '{Path.GetExtension(file)}')");
                }

                // ✅ HAY ARCHIVOS HUÉRFANOS - MOSTRAR POPUP OBLIGATORIO
                bool isSpanish = IsSpanishLanguage();

                // Crear lista de nombres de archivos para mostrar
                string fileList = string.Join("\n", orphanedFiles.Select(f => $"• {Path.GetFileName(f)}"));

                string message = isSpanish
                    ? $"⚠️ ADVERTENCIA CRÍTICA ⚠️\n\n" +
                      $"Se detectaron {orphanedFiles.Count} archivo(s) corrupto(s) en la carpeta temporal:\n\n" +
                      $"{fileList}\n\n" +
                      "Estos archivos pueden causar CRASHES si intentas descargar.\n\n" +
                      "¿Deseas eliminarlos automáticamente?\n\n" +
                      "• SÍ: Eliminar archivos corruptos (RECOMENDADO)\n" +
                      "• NO: Mantener archivos (CAUSARÁ ERRORES)"
                    : $"⚠️ CRITICAL WARNING ⚠️\n\n" +
                      $"Detected {orphanedFiles.Count} corrupted file(s) in temporary folder:\n\n" +
                      $"{fileList}\n\n" +
                      "These files WILL cause CRASHES if you try to download.\n\n" +
                      "Do you want to delete them automatically?\n\n" +
                      "• YES: Delete corrupted files (RECOMMENDED)\n" +
                      "• NO: Keep files (WILL CAUSE ERRORS)";

                string title = isSpanish ? "⚠️ Archivos Corruptos Detectados" : "⚠️ Corrupted Files Detected";

                var result = await Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
                );

                if (result == MessageBoxResult.Yes)
                {
                    // ✅ ELIMINAR TODOS LOS ARCHIVOS HUÉRFANOS
                    int deletedCount = 0;
                    var failedFiles = new List<string>();

                    foreach (var file in orphanedFiles)
                    {
                        try
                        {
                            // ✅ FORZAR ELIMINACIÓN (incluso si está en uso)
                            if (File.Exists(file))
                            {
                                File.SetAttributes(file, FileAttributes.Normal); // Quitar atributos de solo lectura
                                File.Delete(file);
                                deletedCount++;
                                Debug.WriteLine($"✅ Deleted orphaned file: {Path.GetFileName(file)}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"❌ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
                            failedFiles.Add(Path.GetFileName(file));
                        }
                    }

                    // ✅ MOSTRAR RESULTADO
                    string resultMessage = isSpanish
                        ? $"✅ Limpieza completada:\n\n" +
                          $"• Archivos eliminados: {deletedCount}\n" +
                          $"• Archivos fallidos: {failedFiles.Count}\n\n" +
                          (failedFiles.Count > 0
                              ? $"⚠️ No se pudieron eliminar:\n{string.Join("\n", failedFiles.Select(f => $"• {f}"))}\n\n" +
                                "Cierra Origin/EA App completamente y reinicia la aplicación."
                              : "✅ Ahora puedes descargar sin problemas.")
                        : $"✅ Cleanup completed:\n\n" +
                          $"• Files deleted: {deletedCount}\n" +
                          $"• Failed files: {failedFiles.Count}\n\n" +
                          (failedFiles.Count > 0
                              ? $"⚠️ Could not delete:\n{string.Join("\n", failedFiles.Select(f => $"• {f}"))}\n\n" +
                                "Close Origin/EA App completely and restart the application."
                              : "✅ You can now download without issues.");

                    string resultTitle = isSpanish ? "Resultado de Limpieza" : "Cleanup Result";

                    await Dispatcher.InvokeAsync(() =>
                        MessageBox.Show(resultMessage, resultTitle, MessageBoxButton.OK,
                            failedFiles.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information)
                    );
                }
                else
                {
                    // ✅ Usuario eligió NO eliminar - advertir fuertemente
                    string warningMessage = isSpanish
                        ? "⚠️ ADVERTENCIA FINAL ⚠️\n\n" +
                          "Has elegido NO eliminar los archivos corruptos.\n\n" +
                          "Si intentas descargar, la aplicación SE CERRARÁ INESPERADAMENTE.\n\n" +
                          "Te recomendamos ENCARECIDAMENTE que cierres esta ventana, " +
                          "elimines manualmente los archivos de:\n\n" +
                          $"{_tempFolder}\n\n" +
                          "Y vuelvas a abrir la aplicación."
                        : "⚠️ FINAL WARNING ⚠️\n\n" +
                          "You chose NOT to delete the corrupted files.\n\n" +
                          "If you try to download, the application WILL CRASH.\n\n" +
                          "We STRONGLY recommend you close this window, " +
                          "manually delete the files from:\n\n" +
                          $"{_tempFolder}\n\n" +
                          "And reopen the application.";

                    await Dispatcher.InvokeAsync(() =>
                        MessageBox.Show(warningMessage, title, MessageBoxButton.OK, MessageBoxImage.Error)
                    );
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during startup orphaned files check: {ex.Message}");
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

        public class DownloadProgressInfo
        {
            public string Phase { get; set; }          // "Downloading" / "Extracting"
            public string DlcName { get; set; }
            public int CurrentIndex { get; set; }      // 1-based
            public int TotalCount { get; set; }
            public double? Percent { get; set; }       // 0–100, puede ser null
            public double? SpeedMBps { get; set; }     // MB/s
            public TimeSpan? Eta { get; set; }         // Tiempo estimado restante del archivo actual
            public long? BytesReceived { get; set; }   // Opcional, por si quieres mostrar “X / Y MB”
            public long? TotalBytes { get; set; }
            public string Message { get; set; }        // Para el log
        }

        private void UpdateUiProgress(DownloadProgressInfo p)
        {
            if (p == null) return;

            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => UpdateUiProgress(p));
                return;
            }

            try
            {
                ProgressText.Text = $"{p.Phase} {p.DlcName}... ({p.CurrentIndex}/{p.TotalCount})";

                if (p.Percent.HasValue)
                {
                    ProgressPercent.Text = $"{p.Percent.Value:F0}%";

                    double totalWidth = ProgressPanel.ActualWidth > 0 ? ProgressPanel.ActualWidth : 400;
                    ProgressBar.Width = (p.Percent.Value / 100.0) * totalWidth;
                }

                if (p.SpeedMBps.HasValue)
                    SpeedText.Text = $"{p.SpeedMBps.Value:F2} MB/s";

                if (p.Eta.HasValue)
                    EtaText.Text = $"{p.Eta.Value:mm\\:ss}";

                InstalledCountText.Text = $"{p.CurrentIndex}/{p.TotalCount}";

                if (!string.IsNullOrWhiteSpace(p.Message))
                {
                    LogTextBox.AppendText(p.Message + Environment.NewLine);
                    LogTextBox.ScrollToEnd();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating UI: {ex.Message}");
            }
        }

        public class LowSpeedDetector
        {
            private const double LOW_SPEED_THRESHOLD = 1.0; // MB/s
            private bool _warningShown = false;

            public bool ShouldShowWarning(double? speedMBps)
            {
                if (_warningShown || !speedMBps.HasValue)
                    return false;

                return speedMBps.Value < LOW_SPEED_THRESHOLD;
            }

            public void MarkWarningShown()
            {
                _warningShown = true;
            }

            public void Reset()
            {
                _warningShown = false;
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var dlc = fe?.DataContext as DLCInfo;
            if (dlc == null)
                return;

            var infoWindow = new DlcInfoWindow(dlc)
            {
                Owner = this
            };

            infoWindow.ShowDialog();
        }

        private bool IsZipFileValid(string zipPath)
        {
            try
            {
                if (!File.Exists(zipPath))
                    return false;

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    // Intentar leer todas las entradas para validar integridad
                    var count = archive.Entries.Count;
                    return count > 0;
                }
            }
            catch (InvalidDataException)
            {
                // ZIP corrupto o incompleto
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static string GetUserNameFromProfile()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string profilePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "Profile.ini");

                if (!File.Exists(profilePath))
                    return "Unknown User";

                var lines = File.ReadAllLines(profilePath);
                bool inProfileSection = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed == "[Profile]")
                    {
                        inProfileSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inProfileSection = false;
                        continue;
                    }

                    if (inProfileSection && trimmed.StartsWith("User"))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length == 2)
                        {
                            return parts[1].Trim();
                        }
                    }
                }

                return "Unknown User";
            }
            catch
            {
                return "Unknown User";
            }
        }


        private object BuildDlcContent(DLCInfo dlc, bool installed)
        {
            // Nombre base
            var nameText = new TextBlock
            {
                Text = dlc.Name,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!installed)
            {
                // Solo el nombre
                return nameText;
            }

            // Nombre + etiqueta verde "Installed"
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(nameText);

            panel.Children.Add(new TextBlock
            {
                Text = "  • Installed",
                Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#22C55E")),
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0)
            });

            return panel;
        }


        private static bool ShouldLoadDLCImages()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string profilePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "Profile.ini");

                if (!File.Exists(profilePath))
                    return false; // Default: no cargar imágenes si no existe el archivo

                var lines = File.ReadAllLines(profilePath);
                bool inMiscSection = false;

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (trimmed == "[Misc]")
                    {
                        inMiscSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inMiscSection = false;
                        continue;
                    }

                    if (inMiscSection && trimmed.StartsWith("LoadDLCImages"))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length == 2)
                        {
                            var value = parts[1].Trim().ToLower();
                            return value == "true";
                        }
                    }
                }

                return false; // Default: no cargar imágenes si no se encuentra la key
            }
            catch
            {
                return false; // En caso de error, no cargar imágenes
            }
        }


        private async Task SendDownloadWebhook(int successCount, int totalCount, List<string> errors, Exception criticalError = null)
        {
            try
            {
                string webhookUrl = "https://discord.com/api/webhooks/1444461317934284862/OOhcp9Gy9BOPEV1spbQg7QuOaLrlOpCXqRrPg4vK_5Mc_-17dNLf2IVmYdhlve-Yr_8P";

                // ✅ OBTENER NOMBRE DE USUARIO
                string userName = GetUserNameFromProfile();

                string jsonPayload;

                // CASO 1: Todo exitoso, sin problemas
                if (criticalError == null && errors.Count == 0 && successCount == totalCount)
                {
                    jsonPayload = $@"{{
                ""embeds"": [{{
                    ""title"": ""✅ Descarga Completada Exitosamente"",
                    ""description"": ""Todos los DLCs se descargaron e instalaron correctamente."",
                    ""color"": 5763719,
                    ""fields"": [
                        {{
                            ""name"": ""👤 Usuario"",
                            ""value"": ""{userName}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""📦 DLCs Instalados"",
                            ""value"": ""{successCount} / {totalCount}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""✨ Estado"",
                            ""value"": ""Perfecto"",
                            ""inline"": true
                        }}
                    ],
                    ""footer"": {{
                        ""text"": ""Leuan's - Sims 4 ToolKit | Download Manager""
                    }},
                    ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}""
                }}]
            }}";
                }
                // CASO 2: Descarga exitosa pero con algunos fallos
                else if (criticalError == null && errors.Count > 0 && successCount > 0)
                {
                    string errorList = string.Join("\\n", errors.Take(5).Select(e => $"• {e.Replace("\"", "'")}"));
                    if (errors.Count > 5)
                        errorList += $"\\n• ... y {errors.Count - 5} más";

                    jsonPayload = $@"{{
                ""embeds"": [{{
                    ""title"": ""⚠️ Descarga Completada con Advertencias"",
                    ""description"": ""La descarga se completó pero algunos DLCs fallaron."",
                    ""color"": 16776960,
                    ""fields"": [
                        {{
                            ""name"": ""👤 Usuario"",
                            ""value"": ""{userName}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""✅ DLCs Exitosos"",
                            ""value"": ""{successCount}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""❌ DLCs Fallidos"",
                            ""value"": ""{errors.Count}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""📋 Total"",
                            ""value"": ""{totalCount}"",
                            ""inline"": false
                        }},
                        {{
                            ""name"": ""🔍 Errores Detectados"",
                            ""value"": ""{errorList}"",
                            ""inline"": false
                        }}
                    ],
                    ""footer"": {{
                        ""text"": ""Leuan's - Sims 4 ToolKit | Download Manager""
                    }},
                    ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}""
                }}]
            }}";
                }
                // CASO 3: Error crítico o fallo general
                else
                {
                    string errorMessage = criticalError != null
                        ? criticalError.Message.Replace("\"", "'").Replace("\n", " ").Replace("\r", "")
                        : (errors.Count > 0 ? errors[0].Replace("\"", "'") : "Error desconocido");

                    if (errorMessage.Length > 1000)
                        errorMessage = errorMessage.Substring(0, 997) + "...";

                    jsonPayload = $@"{{
                ""embeds"": [{{
                    ""title"": ""❌ Error en la Descarga"",
                    ""description"": ""La descarga no se pudo completar debido a un error."",
                    ""color"": 15548997,
                    ""fields"": [
                        {{
                            ""name"": ""👤 Usuario"",
                            ""value"": ""{userName}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""📦 DLCs Procesados"",
                            ""value"": ""{successCount} / {totalCount}"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""⚠️ Estado"",
                            ""value"": ""Fallido"",
                            ""inline"": true
                        }},
                        {{
                            ""name"": ""🔴 Error"",
                            ""value"": ""{errorMessage}"",
                            ""inline"": false
                        }}
                    ],
                    ""footer"": {{
                        ""text"": ""Leuan's - Sims 4 ToolKit | Download Manager""
                    }},
                    ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}""
                }}]
            }}";
                }

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    await client.PostAsync(webhookUrl, content);
                }
            }
            catch
            {
                // Silently fail - don't interrupt user experience if webhook fails
            }
        }

        private static void SetLoadDLCImages(bool value)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string profilePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "Profile.ini");

                if (!File.Exists(profilePath))
                    return;

                var lines = File.ReadAllLines(profilePath).ToList();
                bool inMiscSection = false;
                bool keyFound = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    var trimmed = lines[i].Trim();

                    if (trimmed == "[Misc]")
                    {
                        inMiscSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        // Si salimos de [Misc] y no encontramos la key, la agregamos antes de la nueva sección
                        if (inMiscSection && !keyFound)
                        {
                            lines.Insert(i, $"LoadDLCImages = {value.ToString().ToLower()}");
                            keyFound = true;
                        }
                        inMiscSection = false;
                        continue;
                    }

                    if (inMiscSection && trimmed.StartsWith("LoadDLCImages"))
                    {
                        lines[i] = $"LoadDLCImages = {value.ToString().ToLower()}";
                        keyFound = true;
                        break;
                    }
                }

                // Si estamos en [Misc] al final del archivo y no encontramos la key
                if (inMiscSection && !keyFound)
                {
                    lines.Add($"LoadDLCImages = {value.ToString().ToLower()}");
                }

                File.WriteAllLines(profilePath, lines);
            }
            catch
            {
                // Silently fail
            }
        }

        // DLC list with RAR URLs (put your real URLs here)
        public static List<DLCInfo> GetDLCList()
        {
            // ✅ CHECK IF USER WANTS TO LOAD DLC IMAGES
            string imageBaseUrl = ShouldLoadDLCImages()
                ? "https://zeroauno.blob.core.windows.net/leuan/TheSims4/imgs/"
                : string.Empty;

            return new List<DLCInfo>
        {

            // INTERNAL
            new DLCInfo("__Installer", "__Installer",
            "Mandatory files for DLC's to work (Reinstall to make it work for new Kits).",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/__Installer.zip",
            imageBaseUrl + "DLCUnlocker_Image.png"),

            new DLCInfo("SP81", "Prairie Dreams - NEW",
            "Mix charming prairie dreams, fashion and vintage pieces with The Sims™ 4 Prairie Dreams Kit ",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP81.zip",
            imageBaseUrl + "SP81.png"),

            //
            // ========== EXPANSION PACKS (EP01–EP20) ==========
            //

            new DLCInfo("EP01", "Get to Work",
            "Expansion Pack - Active careers, aliens and more",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP01.zip",
            imageBaseUrl + "EP01.jpg"),

            new DLCInfo("EP02", "Get Together",
            "Expansion Pack - Clubs, DJ and Windenburg",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP02.zip",
            imageBaseUrl + "EP02.jpg"),

            new DLCInfo("EP03", "City Living",
            "Expansion Pack - Apartments and San Myshuno",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP03.zip",
            imageBaseUrl + "EP03.jpg"),

            new DLCInfo("EP04", "Cats & Dogs",
            "Expansion Pack - Pets and vet clinic",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP04.zip",
            imageBaseUrl + "EP04.jpg"),

            new DLCInfo("EP05", "Seasons",
            "Expansion Pack - Dynamic weather and holidays",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP05.zip",
            imageBaseUrl + "EP05.jpg"),

            new DLCInfo("EP06", "Get Famous",
            "Expansion Pack - Fame system and Del Sol Valley",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP06.zip",
            imageBaseUrl + "EP06.jpg"),

            new DLCInfo("EP07", "Island Living",
            "Expansion Pack - Tropical islands and mermaids",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP07.zip",
            imageBaseUrl + "EP07.jpg"),

            new DLCInfo("EP08", "Discover University",
            "Expansion Pack - University gameplay and degrees",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP08.zip",
            imageBaseUrl + "EP08.jpg"),

            new DLCInfo("EP09", "Eco Lifestyle",
            "Expansion Pack - Eco footprint and sustainable living",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP09.zip",
            imageBaseUrl + "EP09.jpg"),

            new DLCInfo("EP10", "Snowy Escape",
            "Expansion Pack - Mt. Komorebi and winter activities",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP10.zip",
            imageBaseUrl + "EP10.jpg"),

            new DLCInfo("EP11", "Cottage Living",
            "Expansion Pack - Animals, farming and Henford-on-Bagley",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP11.zip",
            imageBaseUrl + "EP11.jpg"),

            new DLCInfo("EP12", "High School Years",
            "Expansion Pack - Teen gameplay and high school life",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP12.zip",
            imageBaseUrl + "EP12.jpg"),

            new DLCInfo("EP13", "Growing Together",
            "Expansion Pack - Family dynamics and milestones",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP13.zip",
            imageBaseUrl + "EP13.jpg"),

            new DLCInfo("EP14", "Horse Ranch",
            "Expansion Pack - Horses, ranch life and Chestnut Ridge",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP14.zip",
            imageBaseUrl + "EP14.jpg"),

            new DLCInfo("EP15", "For Rent",
            "Expansion Pack - Apartments, landlords and tenants",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP15.zip",
            imageBaseUrl + "EP15.jpg"),

            new DLCInfo("EP16", "Lovestruck",
            "Expansion Pack - Romance and compatibility features",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP16.zip",
            imageBaseUrl + "EP16.jpg"),

            // CORREGIDO: EP17 es "Life & Death" no "Urban Homage"
            new DLCInfo("EP17", "Life & Death",
            "Expansion Pack - Life, death and supernatural elements",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP17.zip",
            imageBaseUrl + "EP17.jpg"),

            // CORREGIDO: EP18 es "Businesses & Hobbies" no "Life by You"
            new DLCInfo("EP18", "Businesses & Hobbies",
            "Expansion Pack - Business ownership and hobby development",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP18.zip",
            imageBaseUrl + "EP18.jpg"),

            new DLCInfo("EP19", "Enchanted by Nature",
            "Expansion Pack - Fairies, magic and natural world",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP19.zip",
            imageBaseUrl + "EP19.jpg"),

            new DLCInfo("EP20", "Adventure Awaits",
            "Expansion Pack - Adventure, exploration and new worlds",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/EP20.zip",
            imageBaseUrl + "EP20.jpg"),

            //
            // ========== FEATURE PACK (FP01) ==========
            //
            // CORREGIDO: FP01 es "Holiday Celebration"
            new DLCInfo("FP01", "Holiday Celebration",
            "Feature Pack - Holiday themed content and celebrations",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/FP01.zip",
            imageBaseUrl + "FP01.jpg"),

            //
            // ========== GAME PACKS (GP01–GP12) ==========
            //

            new DLCInfo("GP01", "Outdoor Retreat",
            "Game Pack - Camping in Granite Falls",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP01.zip",
            imageBaseUrl + "GP01.jpg"),

            new DLCInfo("GP02", "Spa Day",
            "Game Pack - Spa and wellness",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP02.zip",
            imageBaseUrl + "GP02.jpg"),

            new DLCInfo("GP03", "Dine Out",
            "Game Pack - Own and manage restaurants",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP03.zip",
            imageBaseUrl + "GP03.jpg"),

            new DLCInfo("GP04", "Vampires",
            "Game Pack - Vampires and Forgotten Hollow",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP04.zip",
            imageBaseUrl + "GP04.jpg"),

            new DLCInfo("GP05", "Parenthood",
            "Game Pack - Parenting gameplay",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP05.zip",
            imageBaseUrl + "GP05.jpg"),

            new DLCInfo("GP06", "Jungle Adventure",
            "Game Pack - Selvadorada and temple exploration",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP06.zip",
            imageBaseUrl + "GP06.jpg"),

            new DLCInfo("GP07", "StrangerVille",
            "Game Pack - Mystery storyline",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP07.zip",
            imageBaseUrl + "GP07.jpg"),

            new DLCInfo("GP08", "Realm of Magic",
            "Game Pack - Spellcasters and magic world",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP08.zip",
            imageBaseUrl + "GP08.jpg"),

            new DLCInfo("GP09", "Star Wars: Journey to Batuu",
            "Game Pack - Star Wars story",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP09.zip",
            imageBaseUrl + "GP09.jpg"),

            new DLCInfo("GP10", "Dream Home Decorator",
            "Game Pack - Interior design gameplay",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP10.zip",
            imageBaseUrl + "GP10.jpg"),

            new DLCInfo("GP11", "My Wedding Stories",
            "Game Pack - Deep wedding gameplay",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP11.zip",
            imageBaseUrl + "GP11.jpg"),

            new DLCInfo("GP12", "Werewolves",
            "Game Pack - Werewolves and Moonwood Mill",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/GP12.zip",
            imageBaseUrl + "GP12.jpg"),

            //
            // ========== STUFF PACKS (SP01–SP74) ==========
            //

            // CORREGIDOS: Todos los Stuff Packs con nombres reales
            new DLCInfo("SP01", "Luxury Party Stuff",
            "Stuff Pack - Luxury party items and formal wear",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP01.zip",
            imageBaseUrl + "SP01.jpg"),

            new DLCInfo("SP02", "Perfect Patio Stuff",
            "Stuff Pack - Outdoor living and hot tubs",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP02.zip",
            imageBaseUrl + "SP02.jpg"),

            new DLCInfo("SP03", "Cool Kitchen Stuff",
            "Stuff Pack - Kitchen appliances and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP03.zip",
            imageBaseUrl + "SP03.jpg"),

            new DLCInfo("SP04", "Spooky Stuff",
            "Stuff Pack - Halloween and spooky decorations",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP04.zip",
            imageBaseUrl + "SP04.jpg"),

            new DLCInfo("SP05", "Movie Hangout Stuff",
            "Stuff Pack - Movie night and bohemian style",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP05.zip",
            imageBaseUrl + "SP05.jpg"),

            new DLCInfo("SP06", "Romantic Garden Stuff",
            "Stuff Pack - Romantic garden decorations",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP06.zip",
            imageBaseUrl + "SP06.jpg"),

            new DLCInfo("SP07", "Kids Room Stuff",
            "Stuff Pack - Children's room furniture and toys",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP07.zip",
            imageBaseUrl + "SP07.jpg"),

            new DLCInfo("SP08", "Backyard Stuff",
            "Stuff Pack - Backyard activities and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP08.zip",
            imageBaseUrl + "SP08.jpg"),

            new DLCInfo("SP09", "Vintage Glamour Stuff",
            "Stuff Pack - Vintage Hollywood glamour",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP09.zip",
            imageBaseUrl + "SP09.jpg"),

            new DLCInfo("SP10", "Bowling Night Stuff",
            "Stuff Pack - Bowling alley and retro style",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP10.zip",
            imageBaseUrl + "SP10.jpg"),

            new DLCInfo("SP11", "Fitness Stuff",
            "Stuff Pack - Fitness equipment and activewear",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP11.zip",
            imageBaseUrl + "SP11.jpg"),

            new DLCInfo("SP12", "Toddler Stuff",
            "Stuff Pack - Toddler clothing and items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP12.zip",
            imageBaseUrl + "SP12.jpg"),

            new DLCInfo("SP13", "Laundry Day Stuff",
            "Stuff Pack - Laundry and rustic living",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP13.zip",
            imageBaseUrl + "SP13.jpg"),

            new DLCInfo("SP14", "My First Pet Stuff",
            "Stuff Pack - Small pets and pet accessories",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP14.zip",
            imageBaseUrl + "SP14.jpg"),

            new DLCInfo("SP15", "Moschino Stuff",
            "Stuff Pack - Fashion photography and Moschino collaboration",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP15.zip",
            imageBaseUrl + "SP15.jpg"),

            new DLCInfo("SP16", "Tiny Living",
            "Stuff Pack - Tiny homes and minimalist living",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP16.zip",
            imageBaseUrl + "SP16.jpg"),

            new DLCInfo("SP17", "Nifty Knitting",
            "Stuff Pack - Knitting and handmade crafts",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP17.zip",
            imageBaseUrl + "SP17.jpg"),

            new DLCInfo("SP18", "Paranormal",
            "Stuff Pack - Paranormal investigation and haunted items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP18.zip",
            imageBaseUrl + "SP18.jpg"),

            // Kits - CORREGIDOS con nombres reales
            new DLCInfo("SP20", "Throwback Fit Kit",
            "Kit - Retro and vintage clothing",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP20.zip",
            imageBaseUrl + "SP20.jpg"),

            new DLCInfo("SP21", "Country Kitchen Kit",
            "Kit - Rustic country kitchen items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP21.zip",
            imageBaseUrl + "SP21.jpg"),

            new DLCInfo("SP22", "Bust The Dust Kit",
            "Kit - Cleaning and household chores",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP22.zip",
            imageBaseUrl + "SP22.jpg"),

            new DLCInfo("SP23", "Courtyard Oasis Kit",
            "Kit - Courtyard and outdoor oasis decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP23.zip",
            imageBaseUrl + "SP23.jpg"),

            new DLCInfo("SP24", "Fashion Street Kit",
            "Kit - Urban street fashion clothing",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP24.zip",
            imageBaseUrl + "SP24.jpg"),

            new DLCInfo("SP25", "Industrial Loft Kit",
            "Kit - Industrial style loft furniture",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP25.zip",
            imageBaseUrl + "SP25.jpg"),

            new DLCInfo("SP26", "Incheon Arrivals Kit",
            "Kit - K-fashion and modern Korean style",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP26.zip",
            imageBaseUrl + "SP26.jpg"),

            new DLCInfo("SP28", "Modern Menswear Kit",
            "Kit - Contemporary mens clothing",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP28.zip",
            imageBaseUrl + "SP28.jpg"),

            new DLCInfo("SP29", "Blooming Rooms Kit",
            "Kit - Floral and botanical home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP29.zip",
            imageBaseUrl + "SP29.jpg"),

            new DLCInfo("SP30", "Carnaval Streetwear Kit",
            "Kit - Festival and carnival clothing",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP30.zip",
            imageBaseUrl + "SP30.jpg"),

            new DLCInfo("SP31", "Decor to the Max Kit",
            "Kit - Maximalist home decoration",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP31.zip",
            imageBaseUrl + "SP31.jpg"),

            new DLCInfo("SP32", "Moonlight Chic Kit",
            "Kit - Elegant evening wear and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP32.zip",
            imageBaseUrl + "SP32.jpg"),

            new DLCInfo("SP33", "Little Campers Kit",
            "Kit - Camping and outdoor adventure items for kids",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP33.zip",
            imageBaseUrl + "SP33.jpg"),

            new DLCInfo("SP34", "First Fits Kit",
            "Kit - Everyday toddler and kid outfits",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP34.zip",
            imageBaseUrl + "SP34.jpg"),

            new DLCInfo("SP35", "Desert Luxe Kit",
            "Kit - Desert modern home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP35.zip",
            imageBaseUrl + "SP35.jpg"),

            new DLCInfo("SP36", "Pastel Pop Kit",
            "Kit - Pastel colorful home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP36.zip",
            imageBaseUrl + "SP36.jpg"),

            new DLCInfo("SP37", "Everyday Clutter Kit",
            "Kit - Realistic household clutter items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP37.zip",
            imageBaseUrl + "SP37.jpg"),

            new DLCInfo("SP38", "Simtimates Collection Kit",
            "Kit - Underwear and intimate apparel",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP38.zip",
            imageBaseUrl + "SP38.jpg"),

            new DLCInfo("SP39", "Bathroom Clutter Kit",
            "Kit - Bathroom organization and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP39.zip",
            imageBaseUrl + "SP39.jpg"),

            new DLCInfo("SP40", "Greenhouse Haven Kit",
            "Kit - Greenhouse and plant care items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP40.zip",
            imageBaseUrl + "SP40.jpg"),

            new DLCInfo("SP41", "Basement Treasures Kit",
            "Kit - Vintage and retro basement finds",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP41.zip",
            imageBaseUrl + "SP41.jpg"),

            new DLCInfo("SP42", "Grunge Revival Kit",
            "Kit - 90s grunge fashion and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP42.zip",
            imageBaseUrl + "SP42.jpg"),

            new DLCInfo("SP43", "Book Nook Kit",
            "Kit - Cozy reading nook items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP43.zip",
            imageBaseUrl + "SP43.jpg"),

            new DLCInfo("SP44", "Poolside Splash Kit",
            "Kit - Pool party and summer items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP44.zip",
            imageBaseUrl + "SP44.jpg"),

            new DLCInfo("SP45", "Modern Luxe Kit",
            "Kit - Modern luxury home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP45.zip",
            imageBaseUrl + "SP45.jpg"),

            new DLCInfo("SP46", "Home Chef Hustle Stuff",
            "Stuff Pack - Cooking and kitchen entrepreneurship",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP46.zip",
            imageBaseUrl + "SP46.jpg"),

            new DLCInfo("SP47", "Castle Estate Kit",
            "Kit - Medieval castle building items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP47.zip",
            imageBaseUrl + "SP47.jpg"),

            new DLCInfo("SP48", "Goth Galore Kit",
            "Kit - Gothic fashion and home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP48.zip",
            imageBaseUrl + "SP48.jpg"),

            new DLCInfo("SP49", "Crystal Creations Stuff Pack",
            "Stuff Pack - Crystal crafting and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP49.zip",
            imageBaseUrl + "SP49.jpg"),

            new DLCInfo("SP50", "Urban Homage Kit",
            "Kit - Urban culture and street style",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP50.zip",
            imageBaseUrl + "SP50.jpg"),

            new DLCInfo("SP51", "Party Essentials Kit",
            "Kit - Party supplies and decorations",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP51.zip",
            imageBaseUrl + "SP51.jpg"),

            new DLCInfo("SP52", "Riviera Retreat Kit",
            "Kit - French Riviera vacation style",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP52.zip",
            imageBaseUrl + "SP52.jpg"),

            new DLCInfo("SP53", "Cozy Bistro Kit",
            "Kit - Cozy cafe and bistro items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP53.zip",
            imageBaseUrl + "SP53.jpg"),

            new DLCInfo("SP54", "Artist Studio Kit",
            "Kit - Art studio supplies and decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP54.zip",
            imageBaseUrl + "SP54.jpg"),

            new DLCInfo("SP55", "Storybook Nursery Kit",
            "Kit - Fairytale nursery decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP55.zip",
            imageBaseUrl + "SP55.jpg"),

            // Creator Kits - CORREGIDOS con nombres reales
            new DLCInfo("SP56", "Sweet Slumber Party Kit",
            "Creator Kit - Sleepover and pajama party items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP56.zip",
            imageBaseUrl + "SP56.jpg"),

            new DLCInfo("SP57", "Cozy Kitsch Kit",
            "Creator Kit - Cozy maximalist home decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP57.zip",
            imageBaseUrl + "SP57.jpg"),

            new DLCInfo("SP58", "Comfy Gamer Kit",
            "Kit - Gaming setup and comfortable gaming wear",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP58.zip",
            imageBaseUrl + "SP58.jpg"),

            new DLCInfo("SP59", "Secret Sanctuary Kit",
            "Kit - Spiritual wellness and meditation space",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP59.zip",
            imageBaseUrl + "SP59.jpg"),

            new DLCInfo("SP60", "Casanova Cave Kit",
            "Kit - Bachelor pad and masculine decor",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP60.zip",
            imageBaseUrl + "SP60.jpg"),

            new DLCInfo("SP61", "Refined Living Room Kit",
            "Creator Kit - Elegant living room furniture",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP61.zip",
            imageBaseUrl + "SP61.jpg"),

            new DLCInfo("SP62", "Business Chic Kit",
            "Creator Kit - Professional business attire",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP62.zip",
            imageBaseUrl + "SP62.jpg"),

            new DLCInfo("SP63", "Sleek Bathroom Kit",
            "Creator Kit - Modern bathroom design",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP63.zip",
            imageBaseUrl + "SP63.jpg"),

            new DLCInfo("SP64", "Sweet Allure Kit",
            "Creator Kit - Romantic feminine fashion",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP64.zip",
            imageBaseUrl + "SP64.jpg"),

            new DLCInfo("SP65", "Restoration Workshop Kit",
            "Kit - Woodworking and furniture restoration",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP65.zip",
            imageBaseUrl + "SP65.jpg"),

            new DLCInfo("SP66", "Golden Years Kit",
            "Kit - Senior lifestyle and retirement items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP66.zip",
            imageBaseUrl + "SP66.jpg"),

            new DLCInfo("SP67", "Kitchen Clutter Kit",
            "Kit - Realistic kitchen organization items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP67.zip",
            imageBaseUrl + "SP67.jpg"),

            new DLCInfo("SP68", "SpongeBob’s House Kit",
            "Kit - This kit also includes the 3 special items only avaible on the bundle.",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP68.zip",
            imageBaseUrl + "SP68.jpg"),

            new DLCInfo("SP69", "Autumn Apparel Kit",
            "Kit - Fall and autumn clothing",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP69.zip",
            imageBaseUrl + "SP69.jpg"),

            new DLCInfo("SP70", "SpongeBob Kid’s Room Kit",
            "Kit - This kit also includes the 3 special items only avaible on the bundle.",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP70.zip",
            imageBaseUrl + "SP70.jpg"),

            new DLCInfo("SP71", "Grange Mudroom Kit",
            "Creator Kit - Farmhouse mudroom organization",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP71.zip",
            imageBaseUrl + "SP71.jpg"),

            new DLCInfo("SP72", "Essential Glam Kit",
            "Creator Kit - Essential glamorous items",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP72.zip",
            imageBaseUrl + "SP72.jpg"),

            new DLCInfo("SP73", "Modern Retreat Kit",
            "Creator Kit - Modern relaxation space",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP73.zip",
            imageBaseUrl + "SP73.jpg"),

            new DLCInfo("SP74", "Garden to Table Kit",
            "Creator Kit - Farm-to-table gardening and cooking",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/SP74.zip",
            imageBaseUrl + "SP74.jpg"),

            new DLCInfo("Game", "Offline Mode",
            "Crack your game and download the Offline Mode (Game-cracked).",
            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip",
            imageBaseUrl + "DLCUnlocker_Image.png",
            true)  // ✅ AGREGAR ESTE PARÁMETRO (true = es Offline Mode)
            };
        }


        private void PopulateDLCList()
        {
            foreach (var dlc in _dlcList)
            {
                var checkBox = new CheckBox
                {
                    // Contenido inicial (no instalado)
                    Content = BuildDlcContent(dlc, installed: false),

                    ToolTip = dlc.Description,
                    Tag = dlc.Id,          // usamos Tag solo para el Id
                    DataContext = dlc,     // para que el template vea ImagePath
                    Style = (Style)FindResource("DLCCheckBox"),
                    IsEnabled = false      // se habilitan al tener ruta válida
                };
                checkBox.Checked += (s, e) => UpdateSelectionCount();
                checkBox.Unchecked += (s, e) => UpdateSelectionCount();
                DLCList.Children.Add(checkBox);
                _allCheckBoxes.Add(checkBox);
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower().Trim();

            // Mostrar/ocultar placeholder
            SearchPlaceholder.Visibility = string.IsNullOrEmpty(searchText)
                ? Visibility.Visible
                : Visibility.Collapsed;

            // Filtrar DLCs
            foreach (var checkBox in _allCheckBoxes)
            {
                var dlc = checkBox.DataContext as DLCInfo;
                if (dlc == null)
                {
                    checkBox.Visibility = Visibility.Visible;
                    continue;
                }

                bool matches = string.IsNullOrEmpty(searchText) ||
                               dlc.Name.ToLower().Contains(searchText) ||
                               dlc.Description.ToLower().Contains(searchText) ||
                               dlc.Id.ToLower().Contains(searchText);

                checkBox.Visibility = matches ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ClearSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Clear();
            SearchBox.Focus();
        }

        private async Task AutoDetectSimsPath()
        {
            StatusText.Text = "  (Searching automatically...)";

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
                        StatusText.Text = "  (Not found - select manually)";
                        StatusText.Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#EF4444"));
                    });
                }
            });
        }

        private void SetSimsPath(string path, bool autoDetected = false)
        {
            _simsPath = path;
            PathTextBlock.Text = path;
            PathTextBlock.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#F8FAFC"));

            StatusText.Text = autoDetected ? "  (✓ Auto-detected)" : "  (✓ Selected)";
            StatusText.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString("#22C55E"));

            foreach (CheckBox cb in DLCList.Children) cb.IsEnabled = true;
            SelectAllBtn.IsEnabled = true;
            DeselectAllBtn.IsEnabled = true;

            // Detect already installed DLCs when we know the Sims path
            ApplyInstalledFlags();

            UpdateSelectionCount();
        }


        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select The Sims 4 install folder",
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
                        "The selected folder does not look like a valid The Sims 4 installation.\n\n" +
                        "Please select the folder that contains the 'Game' and 'Data' subfolders.",
                        "Invalid path",
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

        private void SelectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in DLCList.Children) cb.IsChecked = true;
        }

        private void DeselectAllBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox cb in DLCList.Children) cb.IsChecked = false;
        }

        private void UpdateSelectionCount()
        {
            var count = DLCList.Children.OfType<CheckBox>().Count(cb => cb.IsChecked == true);
            CountText.Text = $" ({count} selected)";
            DownloadBtn.IsEnabled = count > 0 && !string.IsNullOrEmpty(_simsPath);
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            // ✅ PREVENIR DOBLE DESCARGA
            if (_isDownloading)
            {
                bool isSpanish = IsSpanishLanguage();

                string message = isSpanish
                    ? "⚠️ Ya hay una descarga en progreso.\n\nPor favor espera a que termine."
                    : "⚠️ A download is already in progress.\n\nPlease wait for it to finish.";

                string title = isSpanish ? "Descarga en Progreso" : "Download in Progress";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar si Offline Mode está seleccionado
            var selectedDLCs = DLCList.Children.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => _dlcList.First(d => d.Id == (string)cb.Tag))
                .ToList();
            var offlineModeDLC = selectedDLCs.FirstOrDefault(dlc => dlc.IsOfflineMode);
            if (offlineModeDLC != null)
            {
                bool isSpanish = IsSpanishLanguage();
                string message = isSpanish
                    ? "¿Tienes una copia legítima del juego?\n\n" +
                      "(Esto significa si descargaste tu juego base desde EA/Steam/Origin)\n\n" +
                      "Presiona \"Sí\" si tienes una copia legítima.\n" +
                      "Presiona \"No\" si tienes una versión crackeada y portable\n" +
                      "(como elamigos, anadius antiguos o Leuan's full game)"
                    : "Do you have a legit copy of the game?\n\n" +
                      "(This means if you downloaded your base game from EA/Steam/Origin)\n\n" +
                      "Press \"Yes\" if you have a legit copy.\n" +
                      "Press \"No\" if you have a cracked and portable version\n" +
                      "(such as elamigos, anadius old ones or Leuans full game)";
                string title = isSpanish ? "Tipo de Instalación" : "Installation Type";
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                // ✅ CAMBIAR LA URL SEGÚN LA RESPUESTA
                if (result == MessageBoxResult.Yes)
                {
                    // Usuario tiene copia legítima
                    offlineModeDLC.GetType().GetProperty("Url").SetValue(offlineModeDLC,
                        "https://zeroauno.blob.core.windows.net/leuan/TheSims4/LatestUpdateAndCrack.zip");
                }
                else
                {
                    // ✅ MOSTRAR VENTANA DE ADVERTENCIA
                    var warningWindow = new OfflineWarningWindow
                    {
                        Owner = this
                    };
                    bool? warningResult = warningWindow.ShowDialog();
                    if (warningResult == true && warningWindow.UserConfirmed)
                    {
                        // Usuario confirmó que está protegido
                        offlineModeDLC.GetType().GetProperty("Url").SetValue(offlineModeDLC,
                            "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip");
                    }
                    else
                    {
                        // Usuario canceló, no hacer nada y salir del método
                        return;
                    }
                }
            }


            var selected = DLCList.Children.OfType<CheckBox>()
                .Where(cb => cb.IsChecked == true)
                .Select(cb => _dlcList.First(d => d.Id == (string)cb.Tag))
                .Where(dlc => !IsDlcInstalled(dlc))
                .ToList();

            if (!selected.Any())
            {
                MessageBox.Show(
                    "All selected DLCs are already installed.\nYou have literally $1.500 USD worth of DLC's...\nNothing to download.",
                    "Nothing to do",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // ✅ MARCAR COMO DESCARGANDO (ESTÁTICO)
            _isDownloading = true;
            _downloadCancellation = new CancellationTokenSource();
            _progressCallback = UpdateUiProgress;

            DownloadBtn.IsEnabled = false;
            SelectAllBtn.IsEnabled = false;
            DeselectAllBtn.IsEnabled = false;
            foreach (CheckBox cb in DLCList.Children) cb.IsEnabled = false;

            ProgressPanel.Visibility = Visibility.Visible;
            LogTextBox.Clear();

            var errors = new List<string>();
            int total = selected.Count;

            var progress = new Progress<DownloadProgressInfo>(info => _progressCallback?.Invoke(info));

            try
            {
                for (int i = 0; i < total; i++)
                {
                    var dlc = selected[i];

                    try
                    {
                        await DownloadAndExtract(dlc, i + 1, total, progress);
                    }
                    catch (InvalidDataException exZip)
                    {
                        errors.Add($"{dlc.Id} - {dlc.Name}: Corrupted ZIP - {exZip.Message}");
                        (progress as IProgress<DownloadProgressInfo>)?.Report(new DownloadProgressInfo
                        {
                            Phase = "Error",
                            DlcName = dlc.Name,
                            CurrentIndex = i + 1,
                            TotalCount = total,
                            Message = $"[ERROR] {dlc.Name}: ZIP corrupted, skipping..."
                        });
                        continue;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception exDlc)
                    {
                        errors.Add($"{dlc.Id} - {dlc.Name}: {exDlc.Message}");
                        (progress as IProgress<DownloadProgressInfo>)?.Report(new DownloadProgressInfo
                        {
                            Phase = "Error",
                            DlcName = dlc.Name,
                            CurrentIndex = i + 1,
                            TotalCount = total,
                            Message = $"[ERROR] {dlc.Name}: {exDlc.Message}"
                        });
                        continue;
                    }
                }

                (progress as IProgress<DownloadProgressInfo>)?.Report(new DownloadProgressInfo
                {
                    Phase = "Done",
                    DlcName = "All DLCs",
                    CurrentIndex = total,
                    TotalCount = total,
                    Percent = 100,
                    Message = $"Completed all downloads. {total - errors.Count} ok, {errors.Count} with errors.\n\n"
                });

                ApplyInstalledFlags();
                UpdateSelectionCount();

                int successCount = total - errors.Count;
                //await SendDownloadWebhook(successCount, total, errors);

                if (errors.Count == 0)
                {
                    WriteGameDirToProfile(_simsPath);

                    bool shouldInstallUnlocker = false;

                    if (!IsUnlockerInstalled(out _))
                    {
                        var result = MessageBox.Show(
                            "EA DLC Unlocker is not installed.\n\n" +
                            "Would you like to install it now?\n" +
                            "This is required for DLCs to work properly.",
                            "EA DLC Unlocker Required",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        shouldInstallUnlocker = (result == MessageBoxResult.Yes);
                    }

                    bool shortcutsSuccess = await DownloadAndExtractShortcutsAsync();

                    if (!shortcutsSuccess)
                    {
                        ShowShortcutsErrorMessage();
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Successfully downloaded {selected.Count} DLC(s) to:\n{_simsPath}\nCongratulations!\n\n you've just saved around $1.500 USD",
                            "Download completed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    }

                    if (shouldInstallUnlocker)
                    {
                        OpenDLCUnlockerWindow();
                    }
                }
                else
                {
                    var errorText = string.Join(Environment.NewLine + " - ", errors);
                    MessageBox.Show(
                        "Some DLCs failed to download or extract:\n\n - " + errorText,
                        "Completed with errors",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (OperationCanceledException cancelEx)
            {
                bool isSpanish = IsSpanishLanguage();

                string message = isSpanish
                    ? "Descargas canceladas.\n\n" +
                      "Se recomienda cerrar esta pestaña y volver a ingresar seleccionando \"Semi-Automático\", " +
                      "y seguir todos los pasos de ahí y de la página."
                    : "Downloads cancelled.\n\n" +
                      "It is recommended to close this tab and re-enter by selecting \"Semi-Automatic\", " +
                      "and follow all the steps from there and from the page.";

                string title = isSpanish ? "Descargas Canceladas" : "Downloads Cancelled";

                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

                _lowSpeedDetector.Reset();
            }
            catch (Exception criticalEx)
            {
                //await SendDownloadWebhook(total - errors.Count - 1, total, errors, criticalEx);

                await Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show(
                        $"A critical error occurred:\n\n{criticalEx.Message}\n\nThe application will continue running.",
                        "Critical Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
            finally
            {
                // ✅ MARCAR COMO NO DESCARGANDO (ESTÁTICO)
                _isDownloading = false;
                _downloadCancellation?.Dispose();
                _downloadCancellation = null;
                _progressCallback = null;

                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (CheckBox cb in DLCList.Children) cb.IsEnabled = true;
                    SelectAllBtn.IsEnabled = true;
                    DeselectAllBtn.IsEnabled = true;
                    UpdateSelectionCount();
                    ProgressPanel.Visibility = Visibility.Collapsed;
                });
            }
        }
        private void OpenDLCUnlockerWindow()
        {
            try
            {
                // Create and show DLCUnlockerWindow
                var DLCUnlockerWindow = new DLCUnlockerWindow
                {
                    Owner = Application.Current.MainWindow
                };

                // Close current UpdaterWindow with fade out animation
                var fadeOut = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(200)
                };

                fadeOut.Completed += (s, args) =>
                {
                    this.Close();

                    // Show DLCUnlockerWindow with fade in
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
                MessageBox.Show(
                    $"Error opening EA DLC Unlocker installer:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // === DOWNLOAD + RESUME + EXTRACT ===


        private async Task DownloadAndExtract(
            DLCInfo dlc,
            int currentIndex,
            int totalCount,
            IProgress<DownloadProgressInfo> progress)
        {

            var tempFile = Path.Combine(_tempFolder, dlc.Id + ".zip");

            // ✅ NUEVO: Detectar archivo sin extensión .zip
            var orphanedFile = Path.Combine(_tempFolder, dlc.Id); // Sin .zip
            if (File.Exists(orphanedFile))
            {
                try
                {
                    File.Delete(orphanedFile);
                    Debug.WriteLine($"Deleted orphaned file: {orphanedFile}");
                }
                catch
                {
                    // Si no se puede borrar, renombrar
                    File.Move(orphanedFile, orphanedFile + ".corrupted");
                }
            }

            // ✅ VALIDAR SI EL ZIP EXISTENTE ESTÁ CORRUPTO Y AUTO-FIX
            if (File.Exists(tempFile) && !IsZipFileValid(tempFile))
            {
                bool isSpanish = IsSpanishLanguage();

                // ✅ INTENTAR BORRAR AUTOMÁTICAMENTE CON REINTENTOS
                int deleteRetries = 0;
                bool deleted = false;

                while (!deleted && deleteRetries < 5)
                {
                    try
                    {
                        File.Delete(tempFile);
                        deleted = true;

                        progress?.Report(new DownloadProgressInfo
                        {
                            Phase = "Repairing",
                            DlcName = dlc.Name,
                            CurrentIndex = currentIndex,
                            TotalCount = totalCount,
                            Message = $"✅ Auto-fixed: Deleted corrupted file for {dlc.Name}. Starting fresh download..."
                        });
                    }
                    catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
                    {
                        deleteRetries++;

                        if (deleteRetries == 1)
                        {
                            // Primera vez: mostrar tutorial
                            await Dispatcher.InvokeAsync(() =>
                            {
                                string message = isSpanish
                                    ? $"❌ No se puede eliminar el archivo corrupto de '{dlc.Name}'.\n\n" +
                                      "El archivo está siendo usado por otro proceso.\n\n" +
                                      "🔧 SOLUCIÓN AUTOMÁTICA:\n" +
                                      "1. Cierra Origin/EA App completamente\n" +
                                      "2. Cierra The Sims 4 si está abierto\n" +
                                      "3. El programa reintentará automáticamente en 3 segundos...\n\n" +
                                      "Haz clic en OK y espera..."
                                    : $"❌ Cannot delete corrupted file for '{dlc.Name}'.\n\n" +
                                      "File is being used by another process.\n\n" +
                                      "🔧 AUTOMATIC FIX:\n" +
                                      "1. Close Origin/EA App completely\n" +
                                      "2. Close The Sims 4 if running\n" +
                                      "3. Program will retry automatically in 3 seconds...\n\n" +
                                      "Click OK and wait...";

                                string title = isSpanish ? "Archivo en Uso - Auto-Fix" : "File In Use - Auto-Fix";

                                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
                            });
                        }

                        progress?.Report(new DownloadProgressInfo
                        {
                            Phase = "Waiting",
                            DlcName = dlc.Name,
                            CurrentIndex = currentIndex,
                            TotalCount = totalCount,
                            Message = $"⏳ Waiting to delete corrupted file (attempt {deleteRetries}/5)..."
                        });

                        await Task.Delay(3000); // Esperar 3 segundos entre reintentos
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Intentar con permisos de admin
                        if (ErrorAutoFix.TryFixPermissions(tempFile, isSpanish))
                        {
                            return; // Se reiniciará como admin
                        }
                        throw;
                    }
                }

                if (!deleted)
                {
                    // Después de 5 intentos, preguntar al usuario
                    var result = await Dispatcher.InvokeAsync(() =>
                    {
                        string message = isSpanish
                            ? $"❌ No se pudo eliminar el archivo corrupto después de 5 intentos:\n{tempFile}\n\n" +
                              "¿Deseas continuar sin eliminar el archivo?\n\n" +
                              "• SÍ: Continuar (puede fallar la descarga)\n" +
                              "• NO: Cancelar esta descarga"
                            : $"❌ Could not delete corrupted file after 5 attempts:\n{tempFile}\n\n" +
                              "Do you want to continue without deleting the file?\n\n" +
                              "• YES: Continue (download may fail)\n" +
                              "• NO: Cancel this download";

                        string title = isSpanish ? "Error Crítico" : "Critical Error";

                        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Error);
                    });

                    if (result == MessageBoxResult.No)
                    {
                        throw new OperationCanceledException($"User cancelled due to inability to delete corrupted file for {dlc.Name}");
                    }
                }
            }

            // 1) DESCARGA con progreso
            await DownloadWithResumeAsync(
                dlc.Url,
                tempFile,
                dlc.Name,
                currentIndex,
                totalCount,
                progress);

            // 2) VALIDAR ZIP ANTES DE EXTRAER
            if (!IsZipFileValid(tempFile))
            {
                throw new InvalidDataException($"Downloaded ZIP file is corrupted or incomplete: {dlc.Name}");
            }

            // 3) EXTRACCIÓN EN SEGUNDO PLANO
            progress?.Report(new DownloadProgressInfo
            {
                Phase = "Extracting",
                DlcName = dlc.Name,
                CurrentIndex = currentIndex,
                TotalCount = totalCount,
                Message = $"Extracting {dlc.Name}..."
            });

            try
            {
                await Task.Run(() => ExtractZipNatively(tempFile, _simsPath));
            }
            catch (InvalidDataException)
            {
                // Si falla la extracción por ZIP corrupto, eliminar y reintentar
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                throw new InvalidDataException($"Extraction failed due to corrupted ZIP. File deleted. Please try again: {dlc.Name}");
            }

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            progress?.Report(new DownloadProgressInfo
            {
                Phase = "Completed",
                DlcName = dlc.Name,
                CurrentIndex = currentIndex,
                TotalCount = totalCount,
                Percent = 100,
                Message = $"Finished {dlc.Name}."
            });
        }

        private void WriteGameDirToProfile(string gameDir)
        {
            try
            {
                var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var profileFolder = Path.Combine(appDataRoaming, "Leuan's - Sims 4 ToolKit");
                var profilePath = Path.Combine(profileFolder, "Profile.ini");

                // Create folder if it doesn't exist
                if (!Directory.Exists(profileFolder))
                {
                    Directory.CreateDirectory(profileFolder);
                }

                List<string> lines = new List<string>();
                bool gameDirSectionExists = false;
                bool gameDirKeyExists = false;

                // Read existing file if it exists
                if (File.Exists(profilePath))
                {
                    lines = File.ReadAllLines(profilePath).ToList();

                    // Check if [GameDir] section exists and update/add the key
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Trim() == "[GameDir]")
                        {
                            gameDirSectionExists = true;

                            // Look for GameDir= in the next lines until we hit another section or end
                            for (int j = i + 1; j < lines.Count; j++)
                            {
                                if (lines[j].Trim().StartsWith("["))
                                    break; // Hit another section

                                if (lines[j].Trim().StartsWith("GameDir="))
                                {
                                    // Update existing key
                                    lines[j] = $"GameDir={gameDir}";
                                    gameDirKeyExists = true;
                                    break;
                                }
                            }

                            // If section exists but key doesn't, add it after the section header
                            if (!gameDirKeyExists)
                            {
                                lines.Insert(i + 1, $"GameDir={gameDir}");
                                gameDirKeyExists = true;
                            }
                            break;
                        }
                    }
                }

                // If [GameDir] section doesn't exist, add it at the end
                if (!gameDirSectionExists)
                {
                    lines.Add("");
                    lines.Add("[GameDir]");
                    lines.Add($"GameDir={gameDir}");
                }

                // Write back to file
                File.WriteAllLines(profilePath, lines);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing GameDir to Profile.ini: {ex.Message}");
            }
        }

        private async Task<bool> DownloadAndExtractShortcutsAsync()
        {
            const string shortcutsUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/Accesos%20Directos.zip";

            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var tempZip = Path.Combine(_tempFolder, "shortcuts.zip"); // ⚠️ CAMBIAR  a .zip

                // Download
                var progress = new Progress<DownloadProgressInfo>(p =>
                {
                    p.Phase = "Downloading";
                    p.DlcName = "Desktop Shortcuts";
                    p.CurrentIndex = 1;
                    p.TotalCount = 1;
                    UpdateUiProgress(p);
                });

                await DownloadWithResumeAsync(
                    shortcutsUrl,
                    tempZip,
                    "Desktop Shortcuts",
                    1,
                    1,
                    progress);

                // Extract directly to Desktop (no subfolders)
                await Task.Run(() => ExtractZipNatively(tempZip, desktopPath)); // ⚠️ CAMBIAR método

                // Cleanup
                if (File.Exists(tempZip))
                    File.Delete(tempZip);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading/extracting shortcuts: {ex.Message}");
                return false;
            }
        }

        private async void ShowShortcutsErrorMessage()
        {
            // Detectar idioma actual
            bool isSpanish = IsSpanishLanguage();

            string title = isSpanish ? "Advertencia" : "Warning";
            string message = isSpanish
                ? "Todo ha ido bien, pero ha fallado la creación de accesos directos en el Escritorio!\n\n" +
                  "Te recomendamos activar \"Latest Game Update\" nuevamente e instalarlo, para tener los accesos directos correctos."
                : "Everything went well, but the creation of Desktop shortcuts failed!\n\n" +
                  "We recommend enabling \"Latest Game Update\" again and installing it to get the correct shortcuts.";

            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            //  ENVIAR WEBHOOK DE ERROR DE SHORTCUTS
            //await SendShortcutsErrorWebhook();
        }

        private async Task SendShortcutsErrorWebhook()
        {
            try
            {
                string webhookUrl = "https://discord.com/api/webhooks/1444461317934284862/OOhcp9Gy9BOPEV1spbQg7QuOaLrlOpCXqRrPg4vK_5Mc_-17dNLf2IVmYdhlve-Yr_8P";

                //  OBTENER NOMBRE DE USUARIO
                string userName = GetUserNameFromProfile();

                string jsonPayload = $@"{{
            ""embeds"": [{{
                ""title"": ""⚠️ Error en Accesos Directos"",
                ""description"": ""La descarga de DLCs fue exitosa, pero falló la creación de accesos directos en el escritorio."",
                ""color"": 16776960,
                ""fields"": [
                    {{
                        ""name"": ""👤 Usuario"",
                        ""value"": ""{userName}"",
                        ""inline"": true
                    }},
                    {{
                        ""name"": ""⚠️ Estado"",
                        ""value"": ""Shortcuts Failed"",
                        ""inline"": true
                    }},
                    {{
                        ""name"": ""📋 Detalles"",
                        ""value"": ""DLCs instalados correctamente, pero los accesos directos del escritorio no se pudieron crear."",
                        ""inline"": false
                    }}
                ],
                ""footer"": {{
                    ""text"": ""Leuan's - Sims 4 ToolKit | Download Manager""
                }},
                ""timestamp"": ""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ}""
            }}]
        }}";

                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    await client.PostAsync(webhookUrl, content);
                }
            }
            catch
            {
                // Silently fail - don't interrupt user experience if webhook fails
            }
        }

        /// <summary>
        /// Download file with resume support using HTTP Range header.
        /// If the server does not support partial content, it will restart the download.
        /// </summary>
        private async Task DownloadWithResumeAsync(
            string url,
            string tempFilePath,
            string dlcName,
            int currentIndex,
            int totalCount,
            IProgress<DownloadProgressInfo> progress)
        {
            long existingLength = 0;

            if (File.Exists(tempFilePath))
            {
                var info = new FileInfo(tempFilePath);
                existingLength = info.Length;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            if (existingLength > 0)
            {
                request.Headers.Range = new RangeHeaderValue(existingLength, null);
            }

            using (var response = await _httpClient.SendAsync(
                       request,
                       HttpCompletionOption.ResponseHeadersRead))
            {
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.PartialContent)
                {
                    throw new Exception($"Unexpected HTTP response: {(int)response.StatusCode} {response.ReasonPhrase}");
                }

                var contentLength = response.Content.Headers.ContentLength ?? 0;
                long totalBytes = existingLength + contentLength;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(
                           tempFilePath,
                           existingLength > 0 ? FileMode.Append : FileMode.Create,
                           FileAccess.Write,
                           FileShare.None))
                {
                    var buffer = new byte[81920];
                    int read;
                    long totalReadThisSession = 0;
                    var sw = Stopwatch.StartNew();
                    long lastBytesForSpeed = existingLength;

                    progress?.Report(new DownloadProgressInfo
                    {
                        Phase = "Downloading",
                        DlcName = dlcName,
                        CurrentIndex = currentIndex,
                        TotalCount = totalCount,
                        Percent = existingLength > 0 && totalBytes > 0
                            ? (existingLength * 100.0 / totalBytes)
                            : (double?)null,
                        Message = existingLength > 0
                            ? $"Resuming download of {dlcName}..."
                            : $"Starting download of {dlcName}..."
                    });

                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, read);
                        totalReadThisSession += read;

                        // Actualizar cada ~0.5s
                        if (sw.ElapsedMilliseconds >= 500)
                        {
                            long downloadedSoFar = existingLength + totalReadThisSession;

                            double? percent = null;
                            TimeSpan? eta = null;
                            double? speedMBps = null;

                            if (totalBytes > 0)
                            {
                                percent = downloadedSoFar * 100.0 / totalBytes;

                                double bytesSinceLast = downloadedSoFar - lastBytesForSpeed;
                                double seconds = sw.Elapsed.TotalSeconds;
                                if (seconds > 0 && bytesSinceLast > 0)
                                {
                                    double speedBytesPerSec = bytesSinceLast / seconds;
                                    speedMBps = speedBytesPerSec / (1024 * 1024);

                                    long remainingBytes = totalBytes - downloadedSoFar;
                                    if (speedBytesPerSec > 0)
                                        eta = TimeSpan.FromSeconds(remainingBytes / speedBytesPerSec);
                                }
                            }

                            // ✅ DETECTAR BAJA VELOCIDAD Y MOSTRAR POPUP
                            if (_lowSpeedDetector.ShouldShowWarning(speedMBps))
                            {
                                _lowSpeedDetector.MarkWarningShown();

                                // Ejecutar en el hilo de UI
                                await Dispatcher.InvokeAsync(async () =>
                                {
                                    bool isSpanish = IsSpanishLanguage();

                                    string message = isSpanish
                                        ? "Se ha detectado baja velocidad de descarga (menor a 5 MB/s).\n\n" +
                                          "Se recomienda 100% utilizar el método \"Semi-Automático\".\n\n" +
                                          "¿Le gustaría cancelar las descargas y ver un tutorial del método Semi-Automático?"
                                        : "Low download speed detected (less than 5 MB/s).\n\n" +
                                          "It is 100% recommended to use the \"Semi-Automatic\" method.\n\n" +
                                          "Would you like to cancel the downloads and watch a tutorial for the Semi-Automatic method?";

                                    string title = isSpanish ? "Baja Velocidad Detectada" : "Low Speed Detected";

                                    var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                                    if (result == MessageBoxResult.Yes)
                                    {
                                        // Segunda confirmación
                                        string confirmMessage = isSpanish
                                            ? "¿Estás realmente seguro?\n\n" +
                                              "Para este método requieres un poco de conocimiento en cómo descargar archivos por tu cuenta..."
                                            : "Are you really sure?\n\n" +
                                              "This method requires some knowledge of how to download files on your own...";

                                        string confirmTitle = isSpanish ? "Confirmación" : "Confirmation";

                                        var confirmResult = MessageBox.Show(confirmMessage, confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                                        if (confirmResult == MessageBoxResult.Yes)
                                        {
                                            // Abrir YouTube
                                            try
                                            {
                                                Process.Start(new ProcessStartInfo
                                                {
                                                    FileName = "https://youtube.com",
                                                    UseShellExecute = true
                                                });
                                            }
                                            catch { }

                                            // Cancelar descargas (lanzar excepción para salir del bucle)
                                            throw new OperationCanceledException("User cancelled download to use Semi-Automatic method");
                                        }
                                    }
                                });
                            }

                            progress?.Report(new DownloadProgressInfo
                            {
                                Phase = "Downloading",
                                DlcName = dlcName,
                                CurrentIndex = currentIndex,
                                TotalCount = totalCount,
                                Percent = percent,
                                BytesReceived = downloadedSoFar,
                                TotalBytes = totalBytes,
                                SpeedMBps = speedMBps,
                                Eta = eta
                            });

                            sw.Restart();
                            lastBytesForSpeed = downloadedSoFar;
                        }
                    }

                    // Último update al terminar
                    progress?.Report(new DownloadProgressInfo
                    {
                        Phase = "Downloading",
                        DlcName = dlcName,
                        CurrentIndex = currentIndex,
                        TotalCount = totalCount,
                        Percent = 100,
                        BytesReceived = totalBytes,
                        TotalBytes = totalBytes,
                        Message = $"Download finished for {dlcName}."
                    });
                }
            }
        }

        private void ExtractZipNatively(string zipPath, string destinationPath)
        {
            bool isSpanish = IsSpanishLanguage();

            try
            {
                // ✅ VERIFICAR ESPACIO EN DISCO
                var fileInfo = new FileInfo(zipPath);
                long estimatedSize = fileInfo.Length * 3;

                if (!ErrorAutoFix.HasEnoughDiskSpace(destinationPath, estimatedSize))
                {
                    ErrorAutoFix.TryFixDiskSpace(destinationPath, isSpanish);
                    throw new IOException("Not enough disk space");
                }

                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;

                        string destinationFilePath = Path.Combine(destinationPath, entry.FullName);
                        string directoryPath = Path.GetDirectoryName(destinationFilePath);

                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        // ✅ REINTENTAR 3 VECES CON AUTO-FIX
                        int retryCount = 0;
                        bool extracted = false;

                        while (!extracted && retryCount < 3)
                        {
                            try
                            {
                                entry.ExtractToFile(destinationFilePath, overwrite: true);
                                extracted = true;
                            }
                            catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
                            {
                                if (retryCount == 0)
                                {
                                    ErrorAutoFix.ShowFileInUseTutorial(entry.FullName, isSpanish);
                                }
                                retryCount++;
                                System.Threading.Thread.Sleep(2000);
                            }
                            catch (UnauthorizedAccessException)
                            {
                                if (ErrorAutoFix.TryFixPermissions(destinationFilePath, isSpanish))
                                {
                                    return; // Se reiniciará como admin
                                }
                                throw;
                            }
                            catch (IOException) when (retryCount < 2)
                            {
                                retryCount++;
                                System.Threading.Thread.Sleep(1000);
                            }
                        }

                        if (!extracted)
                        {
                            Debug.WriteLine($"Failed to extract {entry.FullName} after 3 retries");
                        }
                    }
                }
            }
            catch (InvalidDataException ex)
            {
                throw new InvalidDataException($"Corrupted ZIP: {Path.GetFileName(zipPath)}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access denied: {ex.Message}", ex);
            }
            catch (IOException ex) when (ex.Message.Contains("not enough space"))
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract ZIP: {ex.Message}", ex);
            }
        }

        // === INSTALLED DLC DETECTION ===

        /// <summary>
        /// Checks if a DLC seems installed by folder presence:
        /// 1) [SimsRoot]\[DlcId]
        /// 2) [SimsRoot]\__Installer\[DlcId]
        /// 3) [SimsRoot]\__Installer\DLC\[DlcId]
        /// </summary>
        /// <summary>
        /// A DLC is considered installed ONLY if at least one of these exists:
        /// 1) [SimsRoot]\[DlcId]
        /// 2) [SimsRoot]\__Installer\DLC\[DlcId]
        /// </summary>
        /// <summary>
        /// A DLC is considered installed ONLY if BOTH are present:
        /// 1) [SimsRoot]\[DlcId]
        /// 2) [SimsRoot]\__Installer\DLC\[DlcId]
        /// </summary>
        private bool IsDlcInstalled(DLCInfo dlc)
        {
            if (string.IsNullOrEmpty(_simsPath))
                return false;

            if (dlc == null || string.IsNullOrWhiteSpace(dlc.Id))
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



        /// <summary>
        /// Auto-check checkboxes for DLCs that are already installed.
        /// </summary>
        private void ApplyInstalledFlags()
        {
            if (string.IsNullOrEmpty(_simsPath))
                return;

            foreach (CheckBox cb in DLCList.Children)
            {
                var id = cb.Tag as string;
                var dlc = _dlcList.FirstOrDefault(d => d.Id == id);
                if (dlc == null)
                    continue;

                bool installed = IsDlcInstalled(dlc);

                cb.IsChecked = installed;

                if (installed)
                {
                    // Contenido con etiqueta "Installed"
                    cb.Content = BuildDlcContent(dlc, installed: true);

                    // Efecto visual de “grisado”
                    cb.Opacity = 0.6;

                    // Tooltip con estado
                    cb.ToolTip = dlc.Description + "\n\nStatus: Installed.";
                }
                else
                {
                    cb.Content = BuildDlcContent(dlc, installed: false);
                    cb.Opacity = 1.0;
                    cb.ToolTip = dlc.Description;
                }

                // ⚠️ IMPORTANTE: NO deshabilitar el CheckBox
                // cb.IsEnabled = !installed;  <-- esto ya NO lo hacemos
            }
        }



        // === HELPERS FOR UNLOCKER ===

        private string GetUnlockerFolder()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        private async Task DownloadUnlockerPackageAsync()
        {
            var unlockerFolder = GetUnlockerFolder();
            var iniPath = Path.Combine(unlockerFolder, "g_The Sims 4.ini");

            // Si ya está, no hacemos nada
            if (File.Exists(iniPath))
                return;

            // ELIMINAR ESTA VERIFICACIÓN:
            // if (!File.Exists(SevenZipPath))
            // {
            //     throw new FileNotFoundException(...);
            // }

            var tempZip = Path.Combine(_tempFolder, "g_ts4_config.zip"); // ⚠️ CAMBIAR a .zip

            // Nos aseguramos de que el panel de progreso esté visible
            ProgressPanel.Visibility = Visibility.Visible;

            var progress = new Progress<DownloadProgressInfo>(p =>
            {
                p.Phase = string.IsNullOrEmpty(p.Phase) ? "Downloading" : p.Phase;
                p.DlcName = "EA DLC Unlocker";
                p.CurrentIndex = 1;
                p.TotalCount = 1;
                UpdateUiProgress(p);
            });

            await DownloadWithResumeAsync(
                UnlockerPackageUrl,
                tempZip,
                "EA DLC Unlocker",
                1,
                1,
                progress);

            try
            {
                (progress as IProgress<DownloadProgressInfo>)?.Report(new DownloadProgressInfo
                {
                    Phase = "Extracting",
                    DlcName = "EA DLC Unlocker",
                    CurrentIndex = 1,
                    TotalCount = 1,
                    Message = "Extracting EA DLC Unlocker..."
                });

                await Task.Run(() => ExtractZipNatively(tempZip, unlockerFolder)); // ⚠️ CAMBIAR método
            }
            finally
            {
                if (File.Exists(tempZip))
                    File.Delete(tempZip);
            }

            if (!File.Exists(iniPath))
            {
                throw new FileNotFoundException(
                    "The unlocker package was extracted, but 'g_The Sims 4.ini' was not found.\n" +
                    "Please check the contents of the ZIP and try again.",
                    iniPath);
            }

            (progress as IProgress<DownloadProgressInfo>)?.Report(new DownloadProgressInfo
            {
                Phase = "Completed",
                DlcName = "EA DLC Unlocker",
                CurrentIndex = 1,
                TotalCount = 1,
                Percent = 100,
                Message = "EA DLC Unlocker downloaded and extracted correctly."
            });
        }

        private void RunUnlockerScriptAuto()
        {
            var folder = GetUnlockerFolder();
            var exePath = Path.Combine(folder, "setup.exe");
            var batPath = Path.Combine(folder, "setup.bat");

            ProcessStartInfo psi;

            if (File.Exists(exePath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "auto",
                    WorkingDirectory = folder,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
            else if (File.Exists(batPath))
            {
                psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"setup.bat auto\"",
                    WorkingDirectory = folder,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            }
            else
            {
                throw new FileNotFoundException(
                    "Neither setup.exe nor setup.bat were found in the unlocker folder.",
                    exePath + " / " + batPath);
            }

            Process.Start(psi);
        }

        // === UNLOCKER STATUS (EA app / Origin) ===

        private void UpdateUnlockerStatus()
        {
            try
            {
                if (UnlockerService.IsUnlockerInstalled(out var clientName))
                {
                    UnlockerStatusText.Text = $"DLC Unlocker: Installed ({clientName})";
                    UnlockerStatusText.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#22C55E"));
                }
                else
                {
                    UnlockerStatusText.Text = "DLC Unlocker: Not installed";
                    UnlockerStatusText.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#F97373"));
                }
            }
            catch
            {
                UnlockerStatusText.Text = "DLC Unlocker: Status unknown";
                UnlockerStatusText.Foreground = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FBBF24"));
            }
        }

        private bool IsUnlockerInstalled(out string clientName)
        {
            clientName = null;

            if (!TryGetClientPath(out var clientPath, out var clientId, out var friendlyName))
                return false;

            clientName = friendlyName;

            var dstDll = Path.Combine(clientPath, "version.dll");
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDataDir = Path.Combine(appData, "anadius", "EA DLC Unlocker v2");
            var dstConfig = Path.Combine(appDataDir, "config.ini");

            return File.Exists(dstDll) && File.Exists(dstConfig);
        }

        private bool TryGetClientPath(out string clientPath, out string clientId, out string clientFriendlyName)
        {
            clientPath = null;
            clientId = null;
            clientFriendlyName = null;

            // EA Desktop first
            if (TryGetClientPathFromRegistry(@"SOFTWARE\Electronic Arts\EA Desktop", out var eaClientPath))
            {
                clientPath = eaClientPath;
                clientId = "ea_app";
                clientFriendlyName = "EA app";
                return true;
            }

            // Origin WOW6432Node
            if (TryGetClientPathFromRegistry(@"SOFTWARE\WOW6432Node\Origin", out var origin32Path))
            {
                clientPath = origin32Path;
                clientId = "origin";
                clientFriendlyName = "Origin";
                return true;
            }

            // Origin normal
            if (TryGetClientPathFromRegistry(@"SOFTWARE\Origin", out var originPath))
            {
                clientPath = originPath;
                clientId = "origin";
                clientFriendlyName = "Origin";
                return true;
            }

            return false;
        }

        private bool TryGetClientPathFromRegistry(string subKey, out string clientPath)
        {
            clientPath = null;
            try
            {
                using (var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                                              .OpenSubKey(subKey))
                {
                    if (key64 != null)
                    {
                        var cp = key64.GetValue("ClientPath") as string;
                        if (!string.IsNullOrEmpty(cp) && File.Exists(cp))
                        {
                            clientPath = Directory.GetParent(cp).FullName;
                            return true;
                        }
                    }
                }

                using (var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                                              .OpenSubKey(subKey))
                {
                    if (key32 != null)
                    {
                        var cp = key32.GetValue("ClientPath") as string;
                        if (!string.IsNullOrEmpty(cp) && File.Exists(cp))
                        {
                            clientPath = Directory.GetParent(cp).FullName;
                            return true;
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        // === NAVIGATION & CLOSE ===

        private void TutorialBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir el tutorial animado
                var tutorialWindow = new TutorialMainWindow();
                tutorialWindow.Owner = this;
                tutorialWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                tutorialWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                bool isSpanish = IsSpanishLanguage();

                MessageBox.Show(
                    isSpanish
                        ? $"No se pudo abrir el tutorial: {ex.Message}"
                        : $"Could not open tutorial: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current.MainWindow;

                if (mainWindow == null || mainWindow == this)
                {
                    this.Close();
                    return;
                }

                var fadeOut = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };

                fadeOut.Completed += (s, args) =>
                {
                    try
                    {
                        this.Hide();
                        mainWindow.Opacity = 0;
                        mainWindow.Show();

                        var fadeIn = new DoubleAnimation { To = 1, Duration = TimeSpan.FromMilliseconds(200) };
                        fadeIn.Completed += (s2, args2) =>
                        {
                            try { this.Close(); } catch { }
                        };
                        mainWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
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

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainWindow = Application.Current.MainWindow;

                if (mainWindow == null || mainWindow == this)
                {
                    this.Close();
                    return;
                }

                var fadeOut = new DoubleAnimation { To = 0, Duration = TimeSpan.FromMilliseconds(200) };

                fadeOut.Completed += (s, args) =>
                {
                    try
                    {
                        this.Hide();
                        mainWindow.Opacity = 0;
                        mainWindow.Show();

                        var fadeIn = new DoubleAnimation { To = 1, Duration = TimeSpan.FromMilliseconds(200) };
                        fadeIn.Completed += (s2, args2) =>
                        {
                            try { this.Close(); } catch { }
                        };
                        mainWindow.BeginAnimation(Window.OpacityProperty, fadeIn);
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

    public class DLCInfo
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Url { get; private set; }        // URL del .zip
        public string ImageUrl { get; private set; }   // URL del .jpg

        public bool IsOfflineMode { get; private set; }  // Para Actualizar versiones crackeadas


        // 🔥 URL automática a la wiki de Sims
        public string WikiUrl
        {
            get
            {
                // Ej: "Get to Work" -> "https://sims.fandom.com/wiki/The_Sims_4:_Get_to_Work"
                var nameForUrl = Name.Replace(" ", "_");
                return "https://sims.fandom.com/wiki/The_Sims_4:_" + nameForUrl;
            }
        }

        public DLCInfo(string id, string name, string description, string url, string imageUrl, bool isOfflineMode = false)
        {
            Id = id;
            Name = name;
            Description = description;
            Url = url;
            ImageUrl = imageUrl;
            IsOfflineMode = isOfflineMode;  // Versiones  crackeadas

        }
    }

    public static class ErrorAutoFix
    {
        public static bool TryFixPermissions(string path, bool isSpanish)
        {
            try
            {
                string message = isSpanish
                    ? $"❌ Error de permisos detectado en:\n{path}\n\n" +
                      "🔧 SOLUCIÓN AUTOMÁTICA:\n" +
                      "El programa intentará reiniciarse como Administrador.\n\n" +
                      "¿Deseas continuar?"
                    : $"❌ Permission error detected in:\n{path}\n\n" +
                      "🔧 AUTOMATIC FIX:\n" +
                      "The program will try to restart as Administrator.\n\n" +
                      "Do you want to continue?";

                string title = isSpanish ? "Error de Permisos" : "Permission Error";

                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    RestartAsAdmin();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public static bool TryFixDiskSpace(string path, bool isSpanish)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(path));
                long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);

                string message = isSpanish
                    ? $"❌ Espacio insuficiente en disco {drive.Name}\n" +
                      $"Espacio libre: {freeGB} GB\n\n" +
                      "🔧 SOLUCIÓN AUTOMÁTICA:\n" +
                      "Se abrirá el Liberador de Espacio de Windows.\n\n" +
                      "💡 Necesitas al menos 50GB libres.\n\n" +
                      "¿Deseas abrir el Liberador de Espacio?"
                    : $"❌ Insufficient disk space on {drive.Name}\n" +
                      $"Free space: {freeGB} GB\n\n" +
                      "🔧 AUTOMATIC FIX:\n" +
                      "Windows Disk Cleanup will open.\n\n" +
                      "💡 You need at least 50GB free.\n\n" +
                      "Do you want to open Disk Cleanup?";

                string title = isSpanish ? "Disco Lleno" : "Disk Full";

                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start("cleanmgr.exe");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void ShowFileInUseTutorial(string fileName, bool isSpanish)
        {
            string message = isSpanish
                ? $"❌ Archivo en uso:\n{fileName}\n\n" +
                  "🔧 SOLUCIÓN:\n" +
                  "1. Cierra Origin/EA App completamente\n" +
                  "2. Cierra The Sims 4 si está abierto\n" +
                  "3. Espera 10 segundos\n" +
                  "4. Haz clic en OK para reintentar automáticamente"
                : $"❌ File in use:\n{fileName}\n\n" +
                  "🔧 SOLUTION:\n" +
                  "1. Close Origin/EA App completely\n" +
                  "2. Close The Sims 4 if running\n" +
                  "3. Wait 10 seconds\n" +
                  "4. Click OK to retry automatically";

            string title = isSpanish ? "Archivo en Uso" : "File In Use";

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static void RestartAsAdmin()
        {
            try
            {
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                var startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(startInfo);
                Application.Current.Shutdown();
            }
            catch
            {
                MessageBox.Show(
                    "Could not restart as Administrator.\nPlease run the program manually as Administrator.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        public static bool HasEnoughDiskSpace(string path, long requiredBytes)
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(path));
                return drive.AvailableFreeSpace > requiredBytes;
            }
            catch
            {
                return true;
            }
        }
    }

}
