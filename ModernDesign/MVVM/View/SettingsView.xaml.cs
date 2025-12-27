using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LeuanS4ToolKit.Core;
using ModernDesign.Core;
using ModernDesign.Localization;
using Microsoft.Win32;

namespace ModernDesign.MVVM.View
{
    public partial class SettingsView : UserControl
    {
        private string _appDataFolder;
        private string _languageIniPath;
        private string _profileIniPath;
        private string _sims4Folder;
        private string _simsPath;
        private bool _isInitializing = true;

        // ===== CONFIGURA ESTAS VARIABLES =====
        private const string CURRENT_VERSION = "1.3.0";
        private const string VERSION_CHECK_URL = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/version.txt";
        // =====================================

        private readonly ILanguageManager _lm;
        
        public SettingsView()
        {
            _lm = ServiceLocator.Get<ILanguageManager>();
            InitializeComponent();
            InitializePaths();
            LoadLanguageFromIni();
            LoadPreloadImagesFromProfile();
            LoadDLCImagesFromProfile();
            InitLocalization();
            _lm.LanguageChanged += (_, __) => InitLocalization();
            SelectLanguageByTag(_lm.CurrentLocale);
            _isInitializing = false;

            // Cargar estados de forma asíncrona
            Loaded += async (s, e) => await LoadAllStatusAsync();
        }

        private void InitializePaths()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _appDataFolder = Path.Combine(appData, "Leuan's - Sims 4 ToolKit");
            _languageIniPath = Path.Combine(_appDataFolder, "language.ini");
            _profileIniPath = Path.Combine(_appDataFolder, "profile.ini");

            // Buscar carpeta de Sims 4 (Documentos)
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _sims4Folder = Path.Combine(docs, "Electronic Arts", "Los Sims 4");
            if (!Directory.Exists(_sims4Folder))
                _sims4Folder = Path.Combine(docs, "Electronic Arts", "The Sims 4");

            // Crear carpeta de app si no existe
            if (!Directory.Exists(_appDataFolder))
                Directory.CreateDirectory(_appDataFolder);
        }

        private void SaveLoadDLCImagesToProfile(bool enabled)
        {
            try
            {
                var lines = File.ReadAllLines(_profileIniPath).ToList();
                bool found = false;
                bool inMiscSection = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].Trim();

                    if (trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && !trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = false;
                    }

                    if (inMiscSection && trimmed.StartsWith("LoadDLCImages", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"LoadDLCImages = {enabled.ToString().ToLower()}";
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    int miscIndex = lines.FindIndex(l => l.Trim().Equals("[Misc]", StringComparison.OrdinalIgnoreCase));
                    if (miscIndex >= 0)
                    {
                        lines.Insert(miscIndex + 1, $"LoadDLCImages = {enabled.ToString().ToLower()}");
                    }
                    else
                    {
                        lines.Add("");
                        lines.Add("[Misc]");
                        lines.Add($"LoadDLCImages = {enabled.ToString().ToLower()}");
                    }
                }

                File.WriteAllLines(_profileIniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving LoadDLCImages setting: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDLCImagesCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            bool isChecked = LoadDLCImagesCheckBox.IsChecked ?? false;
            SaveLoadDLCImagesToProfile(isChecked);

            bool es = _lm.CurrentLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase);

            string message = isChecked
                ? (es ? "Las imágenes de DLC se cargarán en la próxima apertura del Updater.\n\n⚠️ Esto puede consumir hasta 1GB de RAM adicional."
                      : "DLC images will load on next Updater opening.\n\n⚠️ This may consume up to 1GB of additional RAM.")
                : (es ? "Las imágenes de DLC NO se cargarán.\n\nEsto reducirá significativamente el uso de RAM."
                      : "DLC images will NOT load.\n\nThis will significantly reduce RAM usage.");

            MessageBox.Show(
                message,
                es ? "Configuración Actualizada" : "Setting Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoadDLCImagesFromProfile()
        {
            try
            {
                if (!File.Exists(_profileIniPath))
                {
                    LoadDLCImagesCheckBox.IsChecked = true; // Default: activado
                    return;
                }

                string[] lines = File.ReadAllLines(_profileIniPath);
                bool inMiscSection = false;

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && !trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = false;
                    }

                    if (inMiscSection && trimmed.StartsWith("LoadDLCImages", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length >= 2)
                        {
                            string value = parts[1].Trim();
                            LoadDLCImagesCheckBox.IsChecked = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        }
                        return;
                    }
                }

                // Si no existe la línea, default a true
                LoadDLCImagesCheckBox.IsChecked = true;
            }
            catch
            {
                LoadDLCImagesCheckBox.IsChecked = true;
            }
        }
        private void LoadPreloadImagesFromProfile()
        {
            try
            {
                if (!File.Exists(_profileIniPath))
                {
                    PreloadImagesCheckBox.IsChecked = false;
                    return;
                }

                string[] lines = File.ReadAllLines(_profileIniPath);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("PreloadImages", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length >= 2)
                        {
                            string value = parts[1].Trim();
                            PreloadImagesCheckBox.IsChecked = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
                        }
                        return;
                    }
                }

                // Si no existe la línea, default a false
                PreloadImagesCheckBox.IsChecked = false;
            }
            catch
            {
                PreloadImagesCheckBox.IsChecked = false;
            }
        }

        private void SavePreloadImagesToProfile(bool enabled)
        {
            try
            {


                var lines = File.ReadAllLines(_profileIniPath).ToList();
                bool found = false;
                bool inMiscSection = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].Trim();

                    // Detectar sección [Misc]
                    if (trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = true;
                        continue;
                    }

                    // Si encontramos otra sección, salimos de [Misc]
                    if (trimmed.StartsWith("[") && !trimmed.Equals("[Misc]", StringComparison.OrdinalIgnoreCase))
                    {
                        inMiscSection = false;
                    }

                    // Si estamos en [Misc] y encontramos PreloadImages
                    if (inMiscSection && trimmed.StartsWith("PreloadImages", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"PreloadImages = {enabled.ToString().ToLower()}";
                        found = true;
                        break;
                    }
                }

                // Si no encontramos la línea, agregarla al final de [Misc] o crear la sección
                if (!found)
                {
                    int miscIndex = lines.FindIndex(l => l.Trim().Equals("[Misc]", StringComparison.OrdinalIgnoreCase));
                    if (miscIndex >= 0)
                    {
                        // Insertar después de [Misc]
                        lines.Insert(miscIndex + 1, $"PreloadImages = {enabled.ToString().ToLower()}");
                    }
                    else
                    {
                        // Crear sección [Misc]
                        lines.Add("");
                        lines.Add("[Misc]");
                        lines.Add($"PreloadImages = {enabled.ToString().ToLower()}");
                    }
                }

                File.WriteAllLines(_profileIniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving PreloadImages setting: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void PreloadImagesCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            bool isChecked = PreloadImagesCheckBox.IsChecked ?? false;
            SavePreloadImagesToProfile(isChecked);

            bool es = _lm.CurrentLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase);

            string message = isChecked
                ? (es ? "Las imágenes se precargarán en el próximo inicio de la aplicación.\n\nEsto puede aumentar el uso de RAM pero mejorará la respuesta de la interfaz."
                      : "Images will be preloaded on next application startup.\n\nThis may increase RAM usage but will improve interface responsiveness.")
                : (es ? "Las imágenes NO se precargarán en el próximo inicio.\n\nEsto reducirá el uso de RAM."
                      : "Images will NOT be preloaded on next startup.\n\nThis will reduce RAM usage.");

            MessageBox.Show(
                message,
                es ? "Configuración Actualizada" : "Setting Updated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private async Task InitLocalization()
        {
            TitleText.Text = _lm.Get("Title");
            SubtitleText.Text = _lm.Get("Subtitle");
    
            LanguageSectionTitle.Text = _lm.Get("LanguageSectionTitle");
            LanguageLabel.Text = _lm.Get("LanguageLabel");
            LanguageDesc.Text = _lm.Get("LanguageDesc");
    
            PerformanceSectionTitle.Text = _lm.Get("PerformanceSectionTitle");
            PreloadImagesLabel.Text = _lm.Get("PreloadImagesLabel");
            PreloadImagesDesc.Text = _lm.Get("PreloadImagesDesc");
            LoadDLCImagesLabel.Text = _lm.Get("LoadDLCImagesLabel");
            LoadDLCImagesDesc.Text = _lm.Get("LoadDLCImagesDesc");
    
            StatusSectionTitle.Text = _lm.Get("StatusSectionTitle");
            DLCStatusTitle.Text = _lm.Get("DLCStatusTitle");
            UnlockerStatusTitle.Text = _lm.Get("UnlockerStatusTitle");
            VersionTitle.Text = _lm.Get("VersionTitle");
            CurrentVersionLabel.Text = _lm.Get("CurrentVersionLabel");
            LatestVersionLabel.Text = _lm.Get("LatestVersionLabel");
    
            ActionsSectionTitle.Text = _lm.Get("ActionsSectionTitle");
            RefreshBtn.Content = _lm.Get("SettingsViewRefreshBtn");
            OpenFolderBtn.Content = _lm.Get("OpenFolderBtn");
            ResetBtn.Content = _lm.Get("ResetBtn");
    
            AppVersionText.Text = $"Version {CURRENT_VERSION}";
            CurrentVersion.Text = CURRENT_VERSION;
            await LoadAllStatusAsync();
        }

        private void LoadLanguageFromIni()
        {
            try
            {
                if (!File.Exists(_languageIniPath))
                {
                    CreateDefaultLanguageIni();
                    _lm.CurrentLocale = "en-US";
                    return;
                }

                foreach (var rawLine in File.ReadAllLines(_languageIniPath))
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("\""))
                        continue;

                    if (line.StartsWith("Language", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split('=');
                        if (parts.Length >= 2)
                        {
                            var value = parts[1].Trim();
                            if (!string.IsNullOrEmpty(value))
                                _lm.CurrentLocale = value;
                        }
                        break;
                    }
                }

                if (!_lm.LanguageSupported(_lm.CurrentLocale))
                    _lm.CurrentLocale = "en-US";
            }
            catch
            {
                _lm.CurrentLocale = "en-US";
            }
        }

        private void CreateDefaultLanguageIni()
        {
            string content = @"# ------------------------------------ Leuan's - Sims 4 ToolKit ------------------------------------
# Website: leuandev.com | munorals.store | leuan.dev
# Discord: leuan
 
# Avaible Languages: es-ES | en-US
# More languages will be added soon.
 
# Lenguajes Disponibles: es-ES | en-US
# Más lenguajes serán agregados prontos.
 
""Digital Culture was born to be shared,
but the big companies locked it away.
Piracy is just the human gesture
of ensuring that culture remains accessible to everyone,
and not only to those who can afford the luxury of paying.""
 
""La cultura digital nació para compartirse,
pero las grandes compañías lo privatizaron.
La piratería es solo un gesto humano que se asegura que esta cultura sea accesible para todos,
 y no solo para aquellos que tienen el lujo de poder pagar.""
# ------------------------------------ Leuan's - Sims 4 ToolKit ------------------------------------
 
[General]
Language = en-US
";
            try
            {
                File.WriteAllText(_languageIniPath, content);
            }
            catch { }
        }

        private void SaveLanguageToIni(string newLanguage)
        {
            try
            {
                if (!File.Exists(_languageIniPath))
                {
                    CreateDefaultLanguageIni();
                }

                var lines = File.ReadAllLines(_languageIniPath).ToList();
                bool found = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Trim().StartsWith("Language", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"Language = {newLanguage}";
                        found = true;
                        break;
                    }
                }

                if (!found)
                    lines.Add($"Language = {newLanguage}");

                File.WriteAllLines(_languageIniPath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving language: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SelectLanguageByTag(string current)
        {
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag.ToString() == current)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }
        }
        
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            var selected = LanguageComboBox.SelectedItem as ComboBoxItem;
            if (selected?.Tag == null) return;

            string newLang = selected.Tag.ToString();
            if (newLang == _lm.CurrentLocale) return;

            SaveLanguageToIni(newLang);
            _lm.CurrentLocale = newLang;

            bool es = _lm.CurrentLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase);
            
            var result = MessageBox.Show(
                es ? "El idioma se cambiará al reiniciar la aplicación.\n\n¿Reiniciar ahora?"
                   : "Language will change after restarting the application.\n\nRestart now?",
                es ? "Cambio de Idioma" : "Language Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RestartApplication();
            }
        }

        private void RestartApplication()
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    Process.Start(exePath);
                    Application.Current.Shutdown();
                }
            }
            catch { }
        }

        private async Task LoadAllStatusAsync()
        {
            await Task.WhenAll(
                LoadDLCStatusAsync(),
                LoadUnlockerStatusAsync(),
                LoadVersionStatusAsync()
            );
        }
        
        private static bool IsDlcInstalled(string simsPath, string dlcId)
        {
            if (string.IsNullOrEmpty(simsPath))
                return false;

            if (string.IsNullOrWhiteSpace(dlcId))
                return false;

            try
            {
                string rootDlcFolder = Path.Combine(simsPath, dlcId);
                bool rootExists = Directory.Exists(rootDlcFolder);

                string installerDlcFolder = Path.Combine(simsPath, "__Installer", "DLC", dlcId);
                bool installerExists = Directory.Exists(installerDlcFolder);

                return rootExists && installerExists;
            }
            catch
            {
                return false;
            }
        }

        private async Task LoadDLCStatusAsync()
        {
            await Task.Run(() =>
            {
                int installedCount = 0;
                int totalDlc = 0;
                bool simsFound = Sims4PathFinder.FindSims4Path(out _simsPath);

                try
                {
                    var dlcList = UpdaterWindow.GetDLCList();
                    totalDlc = dlcList.Count;

                    if (simsFound && totalDlc > 0)
                    {
                        installedCount = dlcList.Count(d => IsDlcInstalled(_simsPath, d.Id));
                    }
                }
                catch
                {
                    // ignoramos errores, mostramos 0
                }

                Dispatcher.Invoke(() =>
                {
                    if (!simsFound)
                    {
                        DLCCount.Text = "0";
                        DLCStatusDesc.Text = _lm.Get("SettingsViewDLCStatusNotFound");

                        DLCCount.Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#EF4444"));
                        return;
                    }

                    DLCCount.Text = installedCount.ToString();
                    DLCStatusDesc.Text = string.Format(
                        _lm.Get("SettingsViewDLCStatusFound"), 
                        installedCount, 
                        totalDlc);

                    double ratio = totalDlc > 0 ? (double)installedCount / totalDlc : 0;

                    string color;
                    if (ratio >= 0.9)
                        color = "#22C55E";
                    else if (ratio >= 0.5)
                        color = "#F59E0B";
                    else
                        color = "#EF4444";

                    DLCCount.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString(color));
                });
            });
        }

        private async Task LoadUnlockerStatusAsync()
        {
            await Task.Run(() =>
            {
                bool isInstalled = false;
                string clientName = null;

                try
                {
                    isInstalled = UnlockerService.IsUnlockerInstalled(out clientName);
                }
                catch
                {
                    // si falla, dejamos isInstalled = false
                }

                Dispatcher.Invoke(() =>
                {
                    if (isInstalled)
                    {
                        UnlockerStatus.Text = string.Format(
                            _lm.Get("SettingsViewUnlockerStatusInstalled"), 
                            clientName);

                        UnlockerStatus.Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#22C55E"));

                        UnlockerStatusDesc.Text = _lm.Get("SettingsViewUnlockerStatusDescWorking");
                    }
                    else
                    {
                        UnlockerStatus.Text = _lm.Get("SettingsViewUnlockerStatusNotInstalled");
                        UnlockerStatus.Foreground = new SolidColorBrush(
                            (Color)ColorConverter.ConvertFromString("#EF4444"));

                        UnlockerStatusDesc.Text = _lm.Get("SettingsViewUnlockerStatusDescInstall");
                    }
                });
            });
        }

        private async Task LoadVersionStatusAsync()
        {
            bool es = _lm.CurrentLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase);

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string latestVersion = await client.GetStringAsync(VERSION_CHECK_URL);
                    latestVersion = latestVersion.Trim();

                    Dispatcher.Invoke(() =>
                    {
                        LatestVersion.Text = latestVersion;

                        bool isUpToDate = CompareVersions(CURRENT_VERSION, latestVersion) >= 0;

                        if (isUpToDate)
                        {
                            VersionStatus.Text = _lm.Get("SettingsView.UpdaterLatestInstalled");
                            VersionStatus.Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#22C55E"));
                            LatestVersion.Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#22C55E"));
                        }
                        else
                        {
                            VersionStatus.Text = _lm.Get("SettingsView.UpdaterNewVersionAvailable");
                            VersionStatus.Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#F59E0B"));
                            LatestVersion.Foreground = new SolidColorBrush(
                                (Color)ColorConverter.ConvertFromString("#F59E0B"));
                        }
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    LatestVersion.Text = es ? "Error" : "Error";
                    VersionStatus.Text = es ? "⚠ Sin conexión" : "⚠ Offline";
                    VersionStatus.Foreground = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#94A3B8"));
                });
            }
        }

        private int CompareVersions(string v1, string v2)
        {
            try
            {
                var ver1 = new Version(v1);
                var ver2 = new Version(v2);
                return ver1.CompareTo(ver2);
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.Ordinal);
            }
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            RefreshBtn.IsEnabled = false;
            RefreshBtn.Content = _lm.Get("SettingsViewRefreshLoading");

            DLCCount.Text = "--";
            DLCStatusDesc.Text = _lm.Get("SettingsViewScanning");
            UnlockerStatus.Text = "...";
            UnlockerStatusDesc.Text = _lm.Get("SettingsViewChecking");
            LatestVersion.Text = "...";
            VersionStatus.Text = _lm.Get("SettingsViewVersionChecking");

            await LoadAllStatusAsync();

            RefreshBtn.IsEnabled = true;
            RefreshBtn.Content = _lm.Get("SettingsViewRefreshBtn");
        }

        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(_appDataFolder))
                    Directory.CreateDirectory(_appDataFolder);

                Process.Start(new ProcessStartInfo
                {
                    FileName = _appDataFolder,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            bool es = _lm.CurrentLocale.StartsWith("es", StringComparison.OrdinalIgnoreCase);

            var result = MessageBox.Show(
                es ? "¿Estás seguro de que quieres resetear la configuración?\n\nEsto restaurará el idioma a inglés."
                   : "Are you sure you want to reset settings?\n\nThis will restore language to English.",
                es ? "Confirmar Reset" : "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(_languageIniPath))
                        File.Delete(_languageIniPath);

                    CreateDefaultLanguageIni();

                    MessageBox.Show(
                        es ? "Configuración reseteada. Reinicia la aplicación para aplicar cambios."
                           : "Settings reset. Restart the application to apply changes.",
                        "Reset",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}