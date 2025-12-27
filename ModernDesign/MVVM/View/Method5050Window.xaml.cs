using ModernDesign.Localization;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using LeuanS4ToolKit.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace ModernDesign.MVVM.View
{
    public partial class Method5050Window : Window
    {
        private string _modsFolderPath;
        private int _currentIteration = 1;

        public Method5050Window()
        {
            InitializeComponent();
            ApplyLanguage();
            DetectModsFolder();
            UpdateIterationDisplay();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Método 50/50" : "50/50 Method";
            TitleText.Text = es ? "🔎 Método 50/50" : "🔎 50/50 Method";
            DescText.Text = es
                ? "Encuentra mods problemáticos rápidamente desactivando la mitad de tus mods a la vez"
                : "Find problematic mods quickly by disabling half of your mods at a time";

            WhatIsText.Text = es ? "¿Qué es el Método 50/50?" : "What is the 50/50 Method?";
            ExplanationText.Text = es
                ? "Es una técnica eficiente para identificar mods problemáticos:\n\n1. Desactiva el 50% de tus mods\n2. Prueba el juego\n3. Si el error persiste, el problema está en la otra mitad\n4. Si el error desaparece, el problema está en la mitad desactivada\n5. Repite el proceso con la mitad problemática hasta encontrar el mod exacto"
                : "It's an efficient technique to identify problematic mods:\n\n1. Disable 50% of your mods\n2. Test the game\n3. If the error persists, the problem is in the other half\n4. If the error disappears, the problem is in the disabled half\n5. Repeat the process with the problematic half until you find the exact mod";

            DisableHalfButton.Content = es ? "🔄 Desactivar 50% de Mods" : "🔄 Disable 50% of Mods";
            EnableAllButton.Content = es ? "✅ Reactivar Todos" : "✅ Enable All";
            ResetButton.Content = es ? "🔁 Reiniciar" : "🔁 Reset";
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
                    UpdateModCount();
                    return;
                }
            }

            UpdateStatus(ServiceLocator.Get<ILanguageManager>().IsSpanish
                ? "❌ No se encontró la carpeta Mods. Selecciónala manualmente."
                : "❌ Mods folder not found. Select it manually.", true);
        }

        private void UpdateStatus(string message, bool isError)
        {
            StatusText.Text = message;
            StatusBorder.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.ColorConverter.ConvertFromString(isError ? "#7F1D1D" : "#064E3B") as System.Windows.Media.Color? ?? System.Windows.Media.Colors.Gray);
            StatusText.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.ColorConverter.ConvertFromString(isError ? "#FCA5A5" : "#6EE7B7") as System.Windows.Media.Color? ?? System.Windows.Media.Colors.White);
        }

        private void UpdateModCount()
        {
            if (string.IsNullOrEmpty(_modsFolderPath) || !Directory.Exists(_modsFolderPath))
                return;

            int activePackages = Directory.GetFiles(_modsFolderPath, "*.package", SearchOption.AllDirectories).Length;
            int activeScripts = Directory.GetFiles(_modsFolderPath, "*.ts4script", SearchOption.AllDirectories).Length;
            int disabledPackages = Directory.GetFiles(_modsFolderPath, "*.leupackage", SearchOption.AllDirectories).Length;
            int disabledScripts = Directory.GetFiles(_modsFolderPath, "*.leuts4script", SearchOption.AllDirectories).Length;

            ModCountText.Text = ServiceLocator.Get<ILanguageManager>().IsSpanish
                ? $"📊 Mods activos: {activePackages + activeScripts} | Desactivados: {disabledPackages + disabledScripts}"
                : $"📊 Active mods: {activePackages + activeScripts} | Disabled: {disabledPackages + disabledScripts}";
        }

        private void UpdateIterationDisplay()
        {
            IterationText.Text = ServiceLocator.Get<ILanguageManager>().IsSpanish
                ? $"Iteración actual: {_currentIteration}"
                : $"Current iteration: {_currentIteration}";
        }

        private void DisableHalfButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_modsFolderPath) || !Directory.Exists(_modsFolderPath))
            {
                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Selecciona la carpeta Mods primero." : "Select the Mods folder first.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                var packageFiles = Directory.GetFiles(_modsFolderPath, "*.package", SearchOption.AllDirectories).ToList();
                var scriptFiles = Directory.GetFiles(_modsFolderPath, "*.ts4script", SearchOption.AllDirectories).ToList();

                var allActiveFiles = packageFiles.Concat(scriptFiles).ToList();

                if (allActiveFiles.Count == 0)
                {
                    MessageBox.Show(
                        ServiceLocator.Get<ILanguageManager>().IsSpanish
                            ? "No hay mods activos para desactivar. Usa 'Reactivar Todos' primero."
                            : "No active mods to disable. Use 'Enable All' first.",
                        ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Sin Mods" : "No Mods",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                int halfCount = allActiveFiles.Count / 2;
                int disabledCount = 0;

                // Desactivar la primera mitad
                for (int i = 0; i < halfCount; i++)
                {
                    string file = allActiveFiles[i];
                    string extension = file.EndsWith(".package") ? ".leupackage" : ".leuts4script";
                    string newName = file + extension;
                    File.Move(file, newName);
                    disabledCount++;
                }

                _currentIteration++;
                UpdateIterationDisplay();
                UpdateModCount();

                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"✅ Se desactivaron {disabledCount} mods (50% del total)"
                        : $"✅ Disabled {disabledCount} mods (50% of total)",
                    false);

                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"Se desactivaron {disabledCount} mods.\n\nAhora prueba el juego:\n• Si el error persiste → el problema está en los mods activos\n• Si el error desaparece → el problema está en los desactivados"
                        : $"Disabled {disabledCount} mods.\n\nNow test the game:\n• If error persists → problem is in active mods\n• If error disappears → problem is in disabled mods",
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Paso Completado" : "Step Completed",
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
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Selecciona la carpeta Mods primero." : "Select the Mods folder first.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            try
            {
                int enabledCount = 0;

                var leuPackageFiles = Directory.GetFiles(_modsFolderPath, "*.leupackage", SearchOption.AllDirectories);
                foreach (var file in leuPackageFiles)
                {
                    string newName = file.Replace(".package.leupackage", ".package");
                    File.Move(file, newName);
                    enabledCount++;
                }

                var leuScriptFiles = Directory.GetFiles(_modsFolderPath, "*.leuts4script", SearchOption.AllDirectories);
                foreach (var file in leuScriptFiles)
                {
                    string newName = file.Replace(".ts4script.leuts4script", ".ts4script");
                    File.Move(file, newName);
                    enabledCount++;
                }

                UpdateModCount();
                UpdateStatus(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"✅ Se reactivaron {enabledCount} mods"
                        : $"✅ Enabled {enabledCount} mods",
                    false);

                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"Se reactivaron {enabledCount} mods."
                        : $"Enabled {enabledCount} mods.",
                    ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Listo" : "Done",
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

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _currentIteration = 1;
            UpdateIterationDisplay();
            EnableAllButton_Click(sender, e);
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
                UpdateModCount();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}