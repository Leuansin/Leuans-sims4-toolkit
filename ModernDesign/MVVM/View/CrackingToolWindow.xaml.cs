using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using ModernDesign.Core;

namespace ModernDesign.MVVM.View
{
    public partial class CrackingToolWindow : Window
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string _gamePath = "";
        private string _detectedVersion = "";
        private bool _isCracking = false;
        private bool _isCracked = false;

        public CrackingToolWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            this.MouseLeftButtonDown += (s, e) =>
            {
                try { this.DragMove(); } catch { }
            };

            Loaded += async (s, e) => await AutoDetectGame();
        }

        private void ApplyLanguage()
        {
            bool isSpanish = IsSpanishLanguage();

            if (isSpanish)
            {
                HeaderText.Text = "⚡ PROTOCOLO DE CRACKEO ⚡";
                SubHeaderText.Text = "SISTEMA: LEUAN v2.0 | ESTADO: ARMADO";
                PathLabel.Text = "[RUTA_OBJETIVO]";
                PathText.Text = "ESCANEANDO...";
                SelectPathBtn.Content = "📂 SELECCIONAR OBJETIVO";
                StatusLabel.Text = "LISTO";
                CloseBtn.Content = "[X] ABORTAR";
                ConsoleText.Text = ">> SISTEMA INICIALIZADO\n>> ESPERANDO SELECCIÓN DE OBJETIVO...";
            }
            else
            {
                HeaderText.Text = "⚡ CRACKING PROTOCOL ⚡";
                SubHeaderText.Text = "SYSTEM: LEUAN v2.0 | STATUS: ARMED";
                PathLabel.Text = "[TARGET_PATH]";
                PathText.Text = "SCANNING...";
                SelectPathBtn.Content = "📂 SELECT TARGET";
                StatusLabel.Text = "READY";
                CloseBtn.Content = "[X] ABORT";
                ConsoleText.Text = ">> SYSTEM INITIALIZED\n>> AWAITING TARGET SELECTION...";
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

        private async Task AutoDetectGame()
        {
            bool isSpanish = IsSpanishLanguage();

            AddConsoleLog(isSpanish ? ">> INICIANDO ESCANEO..." : ">> INITIATING SCAN...");

            if (Sims4PathFinder.FindSims4Path(out var path))
            {
                _gamePath = path;
                PathText.Text = path;
                AddConsoleLog(isSpanish ? ">> OBJETIVO DETECTADO" : ">> TARGET DETECTED");
                await ValidateGamePath(path);
                return;
            }

            PathText.Text = isSpanish ? "NO DETECTADO" : "NOT DETECTED";
            AddConsoleLog(isSpanish ? ">> DETECCIÓN AUTOMÁTICA FALLIDA" : ">> AUTO-DETECTION FAILED");
            AddConsoleLog(isSpanish ? ">> SELECCIÓN MANUAL REQUERIDA" : ">> MANUAL SELECTION REQUIRED");
        }

        private void SelectPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = IsSpanishLanguage()
                    ? "Selecciona la carpeta raíz de The Sims 4"
                    : "Select The Sims 4 root folder"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _gamePath = dialog.SelectedPath;
                PathText.Text = _gamePath;
                AddConsoleLog(IsSpanishLanguage() ? ">> OBJETIVO SELECCIONADO MANUALMENTE" : ">> TARGET MANUALLY SELECTED");
                _ = ValidateGamePath(_gamePath);
            }
        }

        private async Task ValidateGamePath(string path)
        {
            bool isSpanish = IsSpanishLanguage();

            AddConsoleLog(isSpanish ? ">> ANALIZANDO OBJETIVO..." : ">> ANALYZING TARGET...");

            // Verificar archivos de crack en Game/Bin
            string binPath = Path.Combine(path, "Game", "Bin");
            string[] requiredCrackFiles = new[]
            {
                "leuan.dll",
                "leuan-toolkitS4.dll",
                "leuan-u.dll",
                "leuan-v.dll",
                "leuans4.cfg"
            };

            int missingFiles = 0;
            foreach (var file in requiredCrackFiles)
            {
                if (!File.Exists(Path.Combine(binPath, file)))
                {
                    missingFiles++;
                }
            }

            // Si ya está crackeado completamente
            if (missingFiles == 0)
            {
                _detectedVersion = "Cracked";
                _isCracked = true;
                AddConsoleLog(isSpanish ? ">> VERSIÓN YA CRACKEADA DETECTADA" : ">> ALREADY CRACKED VERSION DETECTED");
                AddConsoleLog(isSpanish ? ">> TODOS LOS ARCHIVOS PRESENTES" : ">> ALL FILES PRESENT");
                AddConsoleLog(isSpanish ? ">> NO SE REQUIERE ACCIÓN" : ">> NO ACTION REQUIRED");

                TransitionToCrackedState(isSpanish);
                SkullBtn.IsEnabled = false;
                return;
            }

            // Si faltan 1-2 archivos (instalación corrupta)
            if (missingFiles >= 1 && missingFiles <= 2)
            {
                _detectedVersion = "Corrupted";
                AddConsoleLog(isSpanish ? ">> INSTALACIÓN CORRUPTA DETECTADA" : ">> CORRUPTED INSTALLATION DETECTED");
                AddConsoleLog(isSpanish ? $">> FALTAN {missingFiles} ARCHIVO(S)" : $">> MISSING {missingFiles} FILE(S)");
                AddConsoleLog(isSpanish ? ">> REQUIERE RE-CRACKEO" : ">> REQUIRES RE-CRACKING");

                TransitionToCorruptedState(isSpanish);
                EnableCrackButton();
                return;
            }

            // Si faltan 3 o más archivos, verificar si es legítimo
            bool hasEAappInstaller = File.Exists(Path.Combine(path, "EAappInstaller_installScript.vdf"));
            bool hasEAStore = File.Exists(Path.Combine(path, "EAStore.ini"));
            bool hasInstallScript = File.Exists(Path.Combine(path, "installScript.vdf"));
            bool hasSteamAppId = File.Exists(Path.Combine(path, "steam_appid.txt"));

            string overlayPath = Path.Combine(path, "__overlay");
            bool hasOverlayFolder = Directory.Exists(overlayPath);
            bool hasOverlayInjector = hasOverlayFolder && File.Exists(Path.Combine(overlayPath, "overlayinjector.exe"));
            bool hasSteamApi = hasOverlayFolder && File.Exists(Path.Combine(overlayPath, "steam_api.dll"));

            if (hasOverlayFolder && hasOverlayInjector && hasSteamApi && hasSteamAppId)
            {
                _detectedVersion = "Steam";
                AddConsoleLog(isSpanish ? ">> VERSIÓN STEAM DETECTADA" : ">> STEAM VERSION DETECTED");
                AddConsoleLog(isSpanish ? ">> OBJETIVO VÁLIDO - LISTO PARA CRACKEAR" : ">> VALID TARGET - READY TO CRACK");
                EnableCrackButton();
            }
            else if (!hasOverlayFolder && !hasSteamAppId && (hasEAappInstaller || hasEAStore || hasInstallScript))
            {
                _detectedVersion = "EA";
                AddConsoleLog(isSpanish ? ">> VERSIÓN EA APP/ORIGIN DETECTADA" : ">> EA APP/ORIGIN VERSION DETECTED");
                AddConsoleLog(isSpanish ? ">> OBJETIVO VÁLIDO - LISTO PARA CRACKEAR" : ">> VALID TARGET - READY TO CRACK");
                EnableCrackButton();
            }
            else
            {
                _detectedVersion = "Unknown";
                AddConsoleLog(isSpanish ? ">> VERSIÓN DESCONOCIDA" : ">> UNKNOWN VERSION");
                AddConsoleLog(isSpanish ? ">> VERIFICA LA RUTA" : ">> VERIFY PATH");
                SkullBtn.IsEnabled = false;
            }

            await Task.Delay(100);
        }

        private void EnableCrackButton()
        {
            SkullBtn.IsEnabled = true;

            // Animar el brillo del skull
            var pulseAnimation = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            SkullShadow.BeginAnimation(DropShadowEffect.OpacityProperty, pulseAnimation);
        }

        private void TransitionToCorruptedState(bool isSpanish)
        {
            // Cambiar a naranja
            var colorOrange = Color.FromRgb(255, 165, 0);

            var colorAnim1 = new ColorAnimation
            {
                To = colorOrange,
                Duration = TimeSpan.FromSeconds(0.8)
            };
            var colorAnim2 = new ColorAnimation
            {
                To = Color.FromRgb(200, 100, 0),
                Duration = TimeSpan.FromSeconds(0.8)
            };

            NeonColor1.BeginAnimation(GradientStop.ColorProperty, colorAnim1);
            NeonColor2.BeginAnimation(GradientStop.ColorProperty, colorAnim2);
            NeonColor3.BeginAnimation(GradientStop.ColorProperty, colorAnim1);

            BorderGlow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);
            LineGlow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);
            LineColor.BeginAnimation(GradientStop.ColorProperty, colorAnim1);

            SkullStrokeBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim1);
            SkullShadow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);

            StatusLabel.Foreground = new SolidColorBrush(colorOrange);
            StatusLabel.Text = isSpanish ? "CORRUPTO" : "CORRUPTED";
            SkullIcon.Text = "⚠️";
        }

        private void KillProcessesByVersion(string version)
        {
            bool isSpanish = IsSpanishLanguage();

            try
            {
                if (version == "Steam")
                {
                    AddConsoleLog(isSpanish ? ">> TERMINANDO PROCESOS STEAM..." : ">> TERMINATING STEAM PROCESSES...");
                    foreach (var processName in new[] { "steam", "steamwebhelper" })
                    {
                        foreach (var process in Process.GetProcessesByName(processName))
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(3000);
                            }
                            catch { }
                        }
                    }
                }
                else if (version == "EA")
                {
                    AddConsoleLog(isSpanish ? ">> TERMINANDO PROCESOS EA..." : ">> TERMINATING EA PROCESSES...");
                    foreach (var processName in new[] { "ea", "eabackgroundservice" })
                    {
                        foreach (var process in Process.GetProcessesByName(processName))
                        {
                            try
                            {
                                process.Kill();
                                process.WaitForExit(3000);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddConsoleLog($">> ERROR: {ex.Message}");
            }
        }

        private async void CrackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_isCracking || _isCracked) return;

            bool isSpanish = IsSpanishLanguage();

            if (string.IsNullOrEmpty(_gamePath))
            {
                MessageBox.Show(
                    isSpanish ? "[ERROR] Selecciona el objetivo primero" : "[ERROR] Select target first",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            _isCracking = true;
            SkullBtn.IsEnabled = false;
            SelectPathBtn.IsEnabled = false;

            // Cambiar skull a candado cerrado
            SkullIcon.Text = "🔒";
            StatusLabel.Text = isSpanish ? "CRACKEANDO..." : "CRACKING...";

            // Iniciar animación de rotación
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            SkullIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            AddConsoleLog(isSpanish ? ">> INICIANDO PROTOCOLO DE CRACKEO..." : ">> INITIATING CRACKING PROTOCOL...");

            if (_detectedVersion == "Steam" || _detectedVersion == "EA")
            {
                KillProcessesByVersion(_detectedVersion);
            }

            try
            {
                // Simular proceso de crackeo
                await SimulateCrackingProcess(isSpanish);

                // Descargar archivo
                AddConsoleLog(isSpanish ? ">> DESCARGANDO PAYLOAD..." : ">> DOWNLOADING PAYLOAD...");
                string downloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Offline/Updater/LeuanVersion/LatestLeuanVersion.zip";
                string zipPath = Path.Combine(Path.GetTempPath(), "LatestLeuanVersion.zip");

                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;
                        int lastProgress = 0;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes != -1)
                            {
                                var progress = (int)((double)totalRead / totalBytes * 100);
                                if (progress % 10 == 0 && progress != lastProgress)
                                {
                                    AddConsoleLog($">> {progress}% [{totalRead / 1024 / 1024} MB / {totalBytes / 1024 / 1024} MB]");
                                    lastProgress = progress;
                                }
                            }
                        }
                    }
                }

                AddConsoleLog(isSpanish ? ">> PAYLOAD DESCARGADO" : ">> PAYLOAD DOWNLOADED");
                AddConsoleLog(isSpanish ? ">> EXTRAYENDO ARCHIVOS..." : ">> EXTRACTING FILES...");

                await Task.Run(() =>
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        int totalFiles = archive.Entries.Count;
                        int processed = 0;
                        int lastReported = 0;

                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destinationPath = Path.Combine(_gamePath, entry.FullName);

                            if (string.IsNullOrEmpty(entry.Name))
                            {
                                Directory.CreateDirectory(destinationPath);
                            }
                            else
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                                entry.ExtractToFile(destinationPath, overwrite: true);
                            }

                            processed++;
                            if (processed % 100 == 0 && processed != lastReported)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    AddConsoleLog($">> {processed} / {totalFiles} FILES");
                                });
                                lastReported = processed;
                            }
                        }
                    }
                });

                AddConsoleLog(isSpanish ? ">> EXTRACCIÓN COMPLETA" : ">> EXTRACTION COMPLETE");
                AddConsoleLog(isSpanish ? ">> LIMPIANDO ARCHIVOS TEMPORALES..." : ">> CLEANING TEMP FILES...");

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                AddConsoleLog(isSpanish ? ">> LIMPIEZA COMPLETA" : ">> CLEANUP COMPLETE");
                await Task.Delay(500);

                // Cambiar a estado CRACKED
                TransitionToCrackedState(isSpanish);
            }
            catch (Exception ex)
            {
                AddConsoleLog($">> ERROR: {ex.Message}");
                MessageBox.Show(
                    isSpanish ? $"[ERROR] {ex.Message}" : $"[ERROR] {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                SkullBtn.IsEnabled = true;
                SelectPathBtn.IsEnabled = true;
                _isCracking = false;
                SkullIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        private async Task SimulateCrackingProcess(bool isSpanish)
        {
            string[] crackingSteps = isSpanish
                ? new[]
                {
                    ">> DESENCRIPTANDO PROTECCIÓN DRM...",
                    ">> BYPASSEANDO VERIFICACIÓN STEAM...",
                    ">> PARCHEANDO EJECUTABLE...",
                    ">> INYECTANDO CRACK...",
                    ">> MODIFICANDO ARCHIVOS DE SISTEMA...",
                    ">> ELIMINANDO VERIFICACIONES ONLINE...",
                    ">> APLICANDO PARCHES DE COMPATIBILIDAD...",
                    ">> FINALIZANDO MODIFICACIONES..."
                }
                : new[]
                {
                    ">> DECRYPTING DRM PROTECTION...",
                    ">> BYPASSING STEAM VERIFICATION...",
                    ">> PATCHING EXECUTABLE...",
                    ">> INJECTING CRACK...",
                    ">> MODIFYING SYSTEM FILES...",
                    ">> REMOVING ONLINE CHECKS...",
                    ">> APPLYING COMPATIBILITY PATCHES...",
                    ">> FINALIZING MODIFICATIONS..."
                };

            foreach (var step in crackingSteps)
            {
                AddConsoleLog(step);
                await Task.Delay(400);
            }
        }

        private void TransitionToCrackedState(bool isSpanish)
        {
            _isCracked = true;
            _isCracking = false;

            // Detener rotación
            SkullIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);

            // Cambiar a candado abierto
            SkullIcon.Text = "🔓";
            StatusLabel.Text = isSpanish ? "CRACKEADO" : "CRACKED";
            StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 65));

            // Cambiar bordes a verde
            AnimateBorderToGreen();

            AddConsoleLog("");
            AddConsoleLog(isSpanish ? "╔════════════════════════════════════╗" : "╔════════════════════════════════════╗");
            AddConsoleLog(isSpanish ? "║   PROTOCOLO COMPLETADO CON ÉXITO   ║" : "║   PROTOCOL COMPLETED SUCCESSFULLY  ║");
            AddConsoleLog(isSpanish ? "╚════════════════════════════════════╝" : "╚════════════════════════════════════╝");
            AddConsoleLog("");
            AddConsoleLog(isSpanish ? ">> TU COPIA AHORA ES PORTABLE Y CRACKEADA" : ">> YOUR COPY IS NOW PORTABLE AND CRACKED");
            AddConsoleLog(isSpanish ? ">> VERSIÓN: LEUAN v2.0" : ">> VERSION: LEUAN v2.0");
            AddConsoleLog(isSpanish ? ">> SISTEMA LISTO PARA USO" : ">> SYSTEM READY FOR USE");

            MessageBox.Show(
                isSpanish
                    ? "╔════════════════════════════════════╗\n║   PROTOCOLO COMPLETADO CON ÉXITO   ║\n╚════════════════════════════════════╝\n\n>> Tu copia legítima ahora es una versión crackeada portable.\n>> Versión: LEUAN v2.0\n\n>> Para actualizar tu juego, pregunta al chatbot:\n   'necesito actualizar mi juego'"
                    : "╔════════════════════════════════════╗\n║   PROTOCOL COMPLETED SUCCESSFULLY  ║\n╚════════════════════════════════════╝\n\n>> Your legitimate copy is now a portable cracked version.\n>> Version: LEUAN v2.0\n\n>> To update your game, ask the chatbot:\n   'i need to update my game'",
                isSpanish ? "Protocolo Completado" : "Protocol Completed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void AnimateBorderToGreen()
        {
            var colorGreen = Color.FromRgb(0, 255, 65);
            var colorGreenDark = Color.FromRgb(0, 200, 50);

            var colorAnim1 = new ColorAnimation
            {
                To = colorGreen,
                Duration = TimeSpan.FromSeconds(1)
            };
            var colorAnim2 = new ColorAnimation
            {
                To = colorGreenDark,
                Duration = TimeSpan.FromSeconds(1)
            };

            NeonColor1.BeginAnimation(GradientStop.ColorProperty, colorAnim1);
            NeonColor2.BeginAnimation(GradientStop.ColorProperty, colorAnim2);
            NeonColor3.BeginAnimation(GradientStop.ColorProperty, colorAnim1);

            BorderGlow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);
            LineGlow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);
            LineColor.BeginAnimation(GradientStop.ColorProperty, colorAnim1);

            SkullStrokeBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim1);
            SkullShadow.BeginAnimation(DropShadowEffect.ColorProperty, colorAnim1);

            // Cambiar fondo de la ventana
            var bgAnim1 = new ColorAnimation
            {
                To = Color.FromRgb(0, 26, 0),
                Duration = TimeSpan.FromSeconds(1)
            };
            var bgAnim2 = new ColorAnimation
            {
                To = Color.FromRgb(0, 10, 0),
                Duration = TimeSpan.FromSeconds(1)
            };

            ((LinearGradientBrush)MainBorder.Background).GradientStops[0].BeginAnimation(GradientStop.ColorProperty, bgAnim1);
            ((LinearGradientBrush)MainBorder.Background).GradientStops[1].BeginAnimation(GradientStop.ColorProperty, bgAnim2);
        }

        private void AddConsoleLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                ConsoleText.Text += "\n" + message;
                ConsoleScroll.ScrollToEnd();
            });
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}