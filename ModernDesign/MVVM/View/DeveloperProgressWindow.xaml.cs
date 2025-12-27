using ModernDesign.Localization;
using ModernDesign.Managers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class DeveloperProgressWindow : Window
    {
        public DeveloperProgressWindow()
        {
            InitializeComponent();
            LoadProgress();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            TitleText.Text = es ? "Progreso Developer Mode" : "Developer Mode Progress";
            SubtitleText.Text = es ? "Completa todos los requisitos para desbloquear" : "Complete all requirements to unlock";

            GoldMedalsDesc.Text = es
                ? "Completa todos los 7 tutoriales con medallas de ORO"
                : "Complete all 7 tutorials with Gold medals";

            DonationDesc.Text = es
                ? "Apoya el desarrollo con una donación mínima de $1 USD"
                : "Support the development with a minimum $1 USD donation";

            DonateButton.Content = es ? "💖 Donar Ahora" : "💖 Donate Now";
            CloseButton.Content = es ? "Cerrar" : "Close";
        }

        private void LoadProgress()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            var progress = DeveloperModeManager.GetProgress();

            // Overall Progress
            int percentage = progress.ProgressPercentage;
            ProgressPercentageText.Text = es ? $"Progreso: {percentage}%" : $"Progress: {percentage}%";
            ProgressBar.Width = percentage * 4.5; // Max width ~450px

            // Gold Medals
            GoldMedalsStatus.Text = progress.HasAllGoldMedals ? "✅" : "❌";
            GoldMedalsStatus.Foreground = new SolidColorBrush(progress.HasAllGoldMedals
                ? (Color)ColorConverter.ConvertFromString("#22C55E")
                : (Color)ColorConverter.ConvertFromString("#EF4444"));

            // Features
            FeaturesTitle.Text = es
                ? $"🎯 Visitar Todas las Funciones ({progress.FeaturesVisited}/{progress.TotalFeatures})"
                : $"🎯 Visit All Features ({progress.FeaturesVisited}/{progress.TotalFeatures})";

            FeaturesStatus.Text = progress.AllFeaturesVisited ? "✅" : "❌";
            FeaturesStatus.Foreground = new SolidColorBrush(progress.AllFeaturesVisited
                ? (Color)ColorConverter.ConvertFromString("#22C55E")
                : (Color)ColorConverter.ConvertFromString("#EF4444"));

            // Lista de features
            LoadFeaturesList(es);

            // Donation
            DonationStatus.Text = progress.HasDonated ? "✅" : "❌";
            DonationStatus.Foreground = new SolidColorBrush(progress.HasDonated
                ? (Color)ColorConverter.ConvertFromString("#22C55E")
                : (Color)ColorConverter.ConvertFromString("#EF4444"));
        }

        private void LoadFeaturesList(bool es)
        {
            var features = new (string id, string nameEs, string nameEn)[]
            {
                ("install_mods", "Instalar Mods", "Install Mods"),
                ("mod_manager", "Mod Manager", "Mod Manager"),
                ("loading_screen", "Loading Screen", "Loading Screen"),
                ("cheats_guide", "Guía de Trucos", "Cheats Guide"),
                ("gallery_manager", "Gestor de Galería", "Gallery Manager"),
                ("music_manager", "Gestor de Música", "Music Manager"),
                ("gameplay_enhancer", "Mejoras de Juego", "Gameplay Enhancer"),
                ("fix_common_errors", "Errores Comunes", "Fix Common Errors"),
                ("method_5050", "Método 50/50", "50/50 Method")
            };

            FeaturesList.Children.Clear();

            foreach (var feature in features)
            {
                bool visited = DeveloperModeManager.IsFeatureVisited(feature.id);
                string name = es ? feature.nameEs : feature.nameEn;

                var textBlock = new TextBlock
                {
                    Text = visited ? $"✅ {name}" : $"❌ {name}",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(visited
                        ? (Color)ColorConverter.ConvertFromString("#22C55E")
                        : (Color)ColorConverter.ConvertFromString("#94A3B8")),
                    Margin = new Thickness(0, 2, 0, 2),
                    FontFamily = new FontFamily("Bahnschrift Light")
                };

                FeaturesList.Children.Add(textBlock);
            }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/leuan", // Cambia por tu link real
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}