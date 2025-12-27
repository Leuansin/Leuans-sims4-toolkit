using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class OrganizeModsWindow : Window
    {
        private string _modsFolderPath;
        private List<ModEntry> _mods = new List<ModEntry>();
        private List<string> _scriptsInSubfolders = new List<string>();

        public OrganizeModsWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            TryAutoDetectModsFolder();
        }

        #region Language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Mod Manager" : "Mod Manager";
            TitleText.Text = es ? "📁 Mod Manager" : "📁 Mod Manager";
            SubtitleText.Text = es ? "Administra, activa, desactiva o elimina tus mods" : "Manage, enable, disable or delete your mods";
            SelectFolderButton.Content = es ? "📂 Seleccionar Carpeta" : "📂 Select Folder";
            RefreshButton.Content = es ? "🔄 Actualizar" : "🔄 Refresh";
            CloseButton.Content = es ? "Cerrar" : "Close";
            EmptyStateTitle.Text = es ? "No se encontraron mods" : "No mods found";
            EmptyStateDesc.Text = es ? "Selecciona tu carpeta Mods para escanear" : "Select your Mods folder to scan for mods";
            ScriptWarningTitle.Text = es ? "¡Scripts en subcarpetas detectados!" : "Scripts in subfolders detected!";
            ScriptWarningDesc.Text = es ? "Algunos archivos .ts4script están en subcarpetas y probablemente no funcionarán" : "Some .ts4script files are in subfolders and probably won't work";
            FixScriptsButton.Content = es ? "Reparar Ahora" : "Fix Now";
            UpdateStats();
        }

        private void UpdateStats()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            int total = _mods.Count;
            int active = _mods.Count(m => m.IsActive);
            int disabled = total - active;
            int packages = _mods.Count(m => m.HasPackage);
            int scripts = _mods.Count(m => m.HasScript);
            TotalModsText.Text = $"{(es ? "Total" : "Total")}: {total}";
            ActiveModsText.Text = $"{(es ? "Activos" : "Active")}: {active}";
            DisabledModsText.Text = $"{(es ? "Desactivados" : "Disabled")}: {disabled}";
            PackagesText.Text = $"Packages: {packages}";
            ScriptsText.Text = $"Scripts: {scripts}";
        }
        #endregion

        #region Folder Detection
        private void TryAutoDetectModsFolder()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string[] paths = {
                Path.Combine(docs, "Electronic Arts", "The Sims 4", "Mods"),
                Path.Combine(docs, "Electronic Arts", "Los Sims 4", "Mods"),
                Path.Combine(docs, "Origin", "The Sims 4", "Mods"),
                Path.Combine(docs, "Origin", "Los Sims 4", "Mods")
            };
            foreach (var p in paths)
            {
                if (Directory.Exists(p)) { _modsFolderPath = p; break; }
            }
            if (!string.IsNullOrEmpty(_modsFolderPath))
            {
                ModsFolderPathText.Text = _modsFolderPath;
                ScanMods();
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Selecciona tu carpeta Mods" : "Select your Mods folder",
                ShowNewFolderButton = false
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _modsFolderPath = dialog.SelectedPath;
                ModsFolderPathText.Text = _modsFolderPath;
                ScanMods();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_modsFolderPath)) ScanMods();
        }
        #endregion

        #region Scanning
        private void ScanMods()
        {
            _mods.Clear();
            _scriptsInSubfolders.Clear();

            if (string.IsNullOrEmpty(_modsFolderPath) || !Directory.Exists(_modsFolderPath))
            {
                UpdateUI();
                return;
            }

            var allFiles = Directory.GetFiles(_modsFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".package", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".ts4script", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".leupackage", StringComparison.OrdinalIgnoreCase) ||
                            f.EndsWith(".leuts4script", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var modDict = new Dictionary<string, ModEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                string dir = Path.GetDirectoryName(file);
                string baseName = GetBaseName(fileName);
                string key = Path.Combine(dir, baseName).ToLowerInvariant();

                // Check if .ts4script is in a subfolder
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if ((ext == ".ts4script" || ext == ".leuts4script") && !IsInModsRoot(file))
                {
                    _scriptsInSubfolders.Add(file);
                }

                if (!modDict.ContainsKey(key))
                {
                    modDict[key] = new ModEntry
                    {
                        BaseName = baseName,
                        Directory = dir,
                        ModsRoot = _modsFolderPath
                    };
                }

                var entry = modDict[key];
                if (ext == ".package") { entry.PackagePath = file; entry.PackageActive = true; }
                else if (ext == ".ts4script") { entry.ScriptPath = file; entry.ScriptActive = true; }
                else if (ext == ".leupackage") { entry.PackagePath = file; entry.PackageActive = false; }
                else if (ext == ".leuts4script") { entry.ScriptPath = file; entry.ScriptActive = false; }
            }

            _mods = modDict.Values.OrderBy(m => m.DisplayName).ToList();
            UpdateUI();
            UpdateScriptWarning();
        }

        private bool IsInModsRoot(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            return string.Equals(dir, _modsFolderPath, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateScriptWarning()
        {
            if (_scriptsInSubfolders.Count > 0)
            {
                ScriptWarningBanner.Visibility = Visibility.Visible;
            }
            else
            {
                ScriptWarningBanner.Visibility = Visibility.Collapsed;
            }
        }

        private string GetBaseName(string fileName)
        {
            string name = fileName;
            string[] exts = { ".package", ".ts4script", ".leupackage", ".leuts4script" };
            foreach (var ext in exts)
            {
                if (name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - ext.Length);
                    break;
                }
            }
            // Remove common suffixes
            string[] suffixes = { "_Scripts", "_Script", "_scripts", "_script", "_Merged", "_merged" };
            foreach (var suf in suffixes)
            {
                if (name.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - suf.Length);
                    break;
                }
            }
            return name;
        }

        private void UpdateUI()
        {
            ModsListPanel.ItemsSource = null;
            ModsListPanel.ItemsSource = _mods;
            EmptyStatePanel.Visibility = _mods.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateStats();
        }
        #endregion

        #region Script Fix
        private void FixScriptsButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            if (_scriptsInSubfolders.Count == 0)
            {
                MessageBox.Show(
                    es ? "No hay scripts en subcarpetas para reparar." : "No scripts in subfolders to fix.",
                    es ? "Información" : "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            string confirmMessage = es
                ? $"Se encontraron {_scriptsInSubfolders.Count} archivo(s) .ts4script en subcarpetas.\n\n¿Deseas moverlos a la carpeta raíz de Mods para que funcionen correctamente?"
                : $"Found {_scriptsInSubfolders.Count} .ts4script file(s) in subfolders.\n\nDo you want to move them to the Mods root folder so they work correctly?";

            var result = MessageBox.Show(
                confirmMessage,
                es ? "Confirmar reparación" : "Confirm fix",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int movedCount = 0;
                int errorCount = 0;
                List<string> errorMessages = new List<string>();

                foreach (var scriptPath in _scriptsInSubfolders.ToList())
                {
                    try
                    {
                        string fileName = Path.GetFileName(scriptPath);
                        string newPath = Path.Combine(_modsFolderPath, fileName);

                        // Handle duplicate names
                        if (File.Exists(newPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            int counter = 1;
                            while (File.Exists(newPath))
                            {
                                newPath = Path.Combine(_modsFolderPath, $"{nameWithoutExt}_{counter}{ext}");
                                counter++;
                            }
                        }

                        File.Move(scriptPath, newPath);
                        movedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errorMessages.Add($"{Path.GetFileName(scriptPath)}: {ex.Message}");
                    }
                }

                // Show results
                if (errorCount == 0)
                {
                    string successMessage = es
                        ? $"¡Reparación completada!\n\n{movedCount} archivo(s) movido(s) exitosamente a la carpeta raíz de Mods."
                        : $"Fix completed!\n\n{movedCount} file(s) successfully moved to Mods root folder.";

                    MessageBox.Show(
                        successMessage,
                        es ? "Éxito" : "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    string errorDetails = string.Join("\n", errorMessages.Take(5));
                    if (errorMessages.Count > 5)
                    {
                        int remaining = errorMessages.Count - 5;
                        string moreText = es ? "más" : "more";
                        errorDetails += $"\n... y {remaining} {moreText}";
                    }

                    string partialMessage = es
                        ? $"Reparación parcial:\n\n✓ {movedCount} archivo(s) movido(s)\n✗ {errorCount} error(es)\n\nErrores:\n{errorDetails}"
                        : $"Partial fix:\n\n✓ {movedCount} file(s) moved\n✗ {errorCount} error(s)\n\nErrors:\n{errorDetails}";

                    MessageBox.Show(
                        partialMessage,
                        es ? "Advertencia" : "Warning",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }

                // Refresh the scan
                ScanMods();
            }
        }
        #endregion

        #region Mod Actions
        private void ToggleModButton_Click(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.Button)sender).Tag is ModEntry mod)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                try
                {
                    if (mod.IsActive)
                    {
                        // Disable
                        if (!string.IsNullOrEmpty(mod.PackagePath) && mod.PackageActive)
                        {
                            string newPath = Path.ChangeExtension(mod.PackagePath, ".leupackage");
                            File.Move(mod.PackagePath, newPath);
                            mod.PackagePath = newPath;
                            mod.PackageActive = false;
                        }
                        if (!string.IsNullOrEmpty(mod.ScriptPath) && mod.ScriptActive)
                        {
                            string newPath = Path.ChangeExtension(mod.ScriptPath, ".leuts4script");
                            File.Move(mod.ScriptPath, newPath);
                            mod.ScriptPath = newPath;
                            mod.ScriptActive = false;
                        }
                    }
                    else
                    {
                        // Enable
                        if (!string.IsNullOrEmpty(mod.PackagePath) && !mod.PackageActive)
                        {
                            string newPath = Path.ChangeExtension(mod.PackagePath, ".package");
                            File.Move(mod.PackagePath, newPath);
                            mod.PackagePath = newPath;
                            mod.PackageActive = true;
                        }
                        if (!string.IsNullOrEmpty(mod.ScriptPath) && !mod.ScriptActive)
                        {
                            string newPath = Path.ChangeExtension(mod.ScriptPath, ".ts4script");
                            File.Move(mod.ScriptPath, newPath);
                            mod.ScriptPath = newPath;
                            mod.ScriptActive = true;
                        }
                    }
                    mod.NotifyChanges();
                    UpdateStats();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(es ? $"Error al cambiar estado del mod:\n{ex.Message}" : $"Error changing mod state:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteModButton_Click(object sender, RoutedEventArgs e)
        {
            if (((System.Windows.Controls.Button)sender).Tag is ModEntry mod)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                var result = MessageBox.Show(
                    es ? $"¿Estás seguro de eliminar permanentemente '{mod.DisplayName}'?\n\nEsta acción no se puede deshacer." : $"Are you sure you want to permanently delete '{mod.DisplayName}'?\n\nThis action cannot be undone.",
                    es ? "Confirmar eliminación" : "Confirm deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(mod.PackagePath) && File.Exists(mod.PackagePath))
                            File.Delete(mod.PackagePath);
                        if (!string.IsNullOrEmpty(mod.ScriptPath) && File.Exists(mod.ScriptPath))
                            File.Delete(mod.ScriptPath);
                        _mods.Remove(mod);
                        UpdateUI();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(es ? $"Error al eliminar el mod:\n{ex.Message}" : $"Error deleting mod:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }

    public class ModEntry : INotifyPropertyChanged
    {
        public string BaseName { get; set; }
        public string Directory { get; set; }
        public string ModsRoot { get; set; }
        public string PackagePath { get; set; }
        public string ScriptPath { get; set; }
        public bool PackageActive { get; set; }
        public bool ScriptActive { get; set; }

        public bool HasPackage => !string.IsNullOrEmpty(PackagePath);
        public bool HasScript => !string.IsNullOrEmpty(ScriptPath);
        public bool IsActive => (HasPackage && PackageActive) || (HasScript && ScriptActive);

        public string DisplayName => BaseName.Replace("_", " ").Replace("-", " ");

        public string TypeInfo
        {
            get
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                if (HasPackage && HasScript) return es ? "📦 Package + 💻 Script" : "📦 Package + 💻 Script";
                if (HasPackage) return es ? "📦 Solo Package" : "📦 Package only";
                return es ? "💻 Solo Script" : "💻 Script only";
            }
        }

        public string FileSizeText
        {
            get
            {
                long size = 0;
                if (HasPackage && File.Exists(PackagePath)) size += new FileInfo(PackagePath).Length;
                if (HasScript && File.Exists(ScriptPath)) size += new FileInfo(ScriptPath).Length;
                if (size < 1024) return $"{size} B";
                if (size < 1024 * 1024) return $"{size / 1024.0:F1} KB";
                return $"{size / (1024.0 * 1024.0):F2} MB";
            }
        }

        public string RelativePath
        {
            get
            {
                string path = HasPackage ? PackagePath : ScriptPath;
                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ModsRoot)) return "";
                return path.Replace(ModsRoot, "").TrimStart('\\', '/');
            }
        }

        public string StatusText => IsActive ? (ServiceLocator.Get<ILanguageManager>().IsSpanish ? "ACTIVO" : "ACTIVE") : (ServiceLocator.Get<ILanguageManager>().IsSpanish ? "DESACTIVADO" : "DISABLED");
        public Brush StatusColor => IsActive ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316"));
        public string ToggleButtonText => IsActive ? (ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Desactivar" : "Disable") : (ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Activar" : "Enable");
        public Brush ToggleButtonColor => IsActive ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
        public string DeleteTooltip => ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Eliminar mod permanentemente" : "Delete mod permanently";

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyChanges()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToggleButtonText)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ToggleButtonColor)));
        }
    }
}