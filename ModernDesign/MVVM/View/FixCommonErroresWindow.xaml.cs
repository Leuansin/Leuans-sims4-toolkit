using ModernDesign.Localization;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using LeuanS4ToolKit.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ModernDesign.MVVM.View
{
    public partial class FixCommonErrorsWindow : Window
    {
        private string _modsFolderPath;

        public FixCommonErrorsWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            DetectModsFolder();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Arreglar Errores Comunes" : "Fix Common Errors";
            TitleText.Text = es ? "🔧 Arreglar Errores Comunes" : "🔧 Fix Common Errors";
            DescText.Text = es
                ? "Esta herramienta desactivará TODOS tus mods temporalmente renombrándolos. Esto te ayudará a identificar mods problemáticos."
                : "This tool will temporarily disable ALL your mods by renaming them. This will help you identify problematic mods.";

            WarningText.Text = es
                ? "⚠️ ADVERTENCIA: Todos los archivos .package y .ts4script serán renombrados a .leupackage y .leuts4script. Podrás reactivarlos manualmente después."
                : "⚠️ WARNING: All .package and .ts4script files will be renamed to .leupackage and .leuts4script. You can manually reactivate them later.";

            RecommendationText.Text = es
                ? "💡 Recomendación: Después de desactivar todos los mods, ve activándolos uno por uno (renombrándolos de vuelta) para identificar cuál causa el problema."
                : "💡 Recommendation: After disabling all mods, activate them one by one (renaming them back) to identify which one causes the problem.";

            DisableAllButton.Content = es ? "🚫 Desactivar Todos los Mods" : "🚫 Disable All Mods";
            EnableAllButton.Content = es ? "✅ Reactivar Todos los Mods" : "✅ Enable All Mods";
            CloseButton.Content = es ? "Cerrar" : "Close";
        }

        private void DetectModsFolder()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string[] basePaths = {
                Path.Combine(docs, "Electronic Arts", "The Sims 4", "Mods"),
                Path.Combine(docs, "Electronic Arts", "Los Sims 4", "Mods"),
                Path.Combine(docs, "Origin", "The Sims 4", "Mods"),
                Path.Combine(docs, "Origin", "Los Sims 4", "Mods")
            };

            foreach (var path in basePaths)
            {
                if (Directory.Exists(path))
                {
                    _modsFolderPath = path;
                    UpdateStatus(ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"📁 Carpeta Mods detectada: {_modsFolderPath}"
                        : $"📁 Mods folder detected: {_modsFolderPath}", false);
                    return;
                }
            }

            UpdateStatus(ServiceLocator.Get<ILanguageManager>().IsSpanish
                ? "❌ No se encontró la carpeta Mods. Por favor, selecciónala manualmente."
                : "❌ Mods folder not found. Please select it manually.", true);
        }

        private void UpdateStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.ColorConverter.ConvertFromString(isError ? "#7F1D1D" : "#064E3B") as System.Windows.Media.Color? ?? System.Windows.Media.Colors.Gray);
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.ColorConverter.ConvertFromString(isError ? "#FCA5A5" : "#6EE7B7") as System.Windows.Media.Color? ?? System.Windows.Media.Colors.White);
        }

        private void DisableAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_modsFolderPath) || !Directory.Exists(_modsFolderPath))
            {
                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Por favor, selecciona la carpeta Mods primero." : "Please select the Mods folder first.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                ServiceLocator.Get<ILanguageManager>().IsSpanish
                    ? "¿Estás seguro de que deseas desactivar TODOS los mods?\n\nSe renombrarán a .leupackage y .leuts4script."
                    : "Are you sure you want to disable ALL mods?\n\nThey will be renamed to .leupackage and .leuts4script.",
                ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Confirmar" : "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                int packageCount = 0;
                int scriptCount = 0;

                // Renombrar todos los .package a .leupackage
                var packageFiles = Directory.GetFiles(_modsFolderPath, "*.package", SearchOption.AllDirectories);
                foreach (var file in packageFiles)
                {
                    string newName = file + ".leupackage";
                    File.Move(file, newName);
                    packageCount++;
                }

                // Renombrar todos los .ts4script a .leuts4script
                var scriptFiles = Directory.GetFiles(_modsFolderPath, "*.ts4script", SearchOption.AllDirectories);
                foreach (var file in scriptFiles)
                {
                    string newName = file + ".leuts4script";
                    File.Move(file, newName);
                    scriptCount++;
                }

                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"✅ Se desactivaron {packageCount} archivos .package y {scriptCount} archivos .ts4script"
                        : $"✅ Disabled {packageCount} .package files and {scriptCount} .ts4script files",
                    false);

                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"¡Listo! Se desactivaron:\n• {packageCount} archivos .package\n• {scriptCount} archivos .ts4script\n\nAhora puedes probar el juego sin mods."
                        : $"Done! Disabled:\n• {packageCount} .package files\n• {scriptCount} .ts4script files\n\nYou can now test the game without mods.",
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? $"❌ Error: {ex.Message}" : $"❌ Error: {ex.Message}",
                    true);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnableAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_modsFolderPath) || !Directory.Exists(_modsFolderPath))
            {
                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Por favor, selecciona la carpeta Mods primero." : "Please select the Mods folder first.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                int packageCount = 0;
                int scriptCount = 0;

                // Renombrar todos los .leupackage de vuelta a .package
                var leuPackageFiles = Directory.GetFiles(_modsFolderPath, "*.leupackage", SearchOption.AllDirectories);
                foreach (var file in leuPackageFiles)
                {
                    string newName = file.Replace(".package.leupackage", ".package");
                    File.Move(file, newName);
                    packageCount++;
                }

                // Renombrar todos los .leuts4script de vuelta a .ts4script
                var leuScriptFiles = Directory.GetFiles(_modsFolderPath, "*.leuts4script", SearchOption.AllDirectories);
                foreach (var file in leuScriptFiles)
                {
                    string newName = file.Replace(".ts4script.leuts4script", ".ts4script");
                    File.Move(file, newName);
                    scriptCount++;
                }

                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"✅ Se reactivaron {packageCount} archivos .package y {scriptCount} archivos .ts4script"
                        : $"✅ Enabled {packageCount} .package files and {scriptCount} .ts4script files",
                    false);

                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"¡Listo! Se reactivaron:\n• {packageCount} archivos .package\n• {scriptCount} archivos .ts4script"
                        : $"Done! Enabled:\n• {packageCount} .package files\n• {scriptCount} .ts4script files",
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? $"❌ Error: {ex.Message}" : $"❌ Error: {ex.Message}",
                    true);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = ServiceLocator.Get<ILanguageManager>().IsSpanish
                    ? "Selecciona la carpeta Mods de The Sims 4"
                    : "Select The Sims 4 Mods folder"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _modsFolderPath = dialog.SelectedPath;
                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"📁 Carpeta seleccionada: {_modsFolderPath}"
                        : $"📁 Folder selected: {_modsFolderPath}",
                    false);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}