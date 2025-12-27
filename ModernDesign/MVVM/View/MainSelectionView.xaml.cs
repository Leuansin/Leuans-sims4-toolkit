using System;
using System.Windows;
using System.Windows.Controls;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class MainSelectionView : UserControl
    {
        private readonly ILanguageManager _lm;

        public MainSelectionView()
        {
            _lm = ServiceLocator.Get<ILanguageManager>();
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool isSpanish = IsSpanishLanguage();

            HeaderText.Text = _lm.Get("MainSelectionViewHeaderText");
            SubHeaderText.Text = _lm.Get("MainSelectionViewSubHeaderText");
            DownloadDLCsBtn.Content = _lm.Get("MainSelectionViewDownloadDLCsBtn");
            DownloadDLCsBtn.ToolTip = _lm.Get("MainSelectionViewDownloadDLCsBtnTooltip");
            UpdateGameBtn.Content = _lm.Get("MainSelectionViewUpdateGameBtn");
            UpdateGameBtn.ToolTip = _lm.Get("MainSelectionViewUpdateGameBtnTooltip");
        }

        private static bool IsSpanishLanguage()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string languagePath = System.IO.Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "language.ini");

                if (!System.IO.File.Exists(languagePath))
                    return false;

                var lines = System.IO.File.ReadAllLines(languagePath);
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

        private void DownloadDLCsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Abrir InstallModeSelector como ventana
            var installModeSelector = new InstallModeSelector();
            installModeSelector.Owner = Window.GetWindow(this);
            installModeSelector.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            installModeSelector.ShowDialog();

            // ✅ CERRAR MainSelectionWindow después de abrir InstallModeSelector
            Window.GetWindow(this)?.Close();
        }

        private void UpdateGameBtn_Click(object sender, RoutedEventArgs e)
        {
            string message = _lm.Get("MainSelectionViewUpdateWarning");

            string title = _lm.Get("Warning");

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Abrir UpdateVersionSelectorWindow como ventana
                var versionSelector = new UpdateVersionSelectorWindow();
                versionSelector.Owner = Window.GetWindow(this);
                versionSelector.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                versionSelector.ShowDialog();

                // ✅ CERRAR MainSelectionWindow después de abrir UpdateVersionSelectorWindow
                Window.GetWindow(this)?.Close();
            }
        }
    }
}