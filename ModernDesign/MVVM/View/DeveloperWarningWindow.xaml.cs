using ModernDesign.Localization;
using ModernDesign.Managers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class DeveloperWarningWindow : Window
    {
        public DeveloperWarningWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            ShowProgress();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            TitleText.Text = es ? "⚠️ Acceso Denegado" : "⚠️ Access Denied";

            if (es)
            {
                MessageText.Text = "Oh oh! Al parecer no eres un Developer real y has hecho alguna especie de trampa para desbloquear este modo... 😏\n\n" +
                                   "¡Te invitamos a convertirte en Developer REAL solo donando $1 USD! 💰\n\n" +
                                   "$1 USD... ¡¡¡NO ES NADA!!! para todo lo que te ofrezco con esta hermosa herramienta. ✨\n\n" +
                                   "¿Quieres apoyar el desarrollo? 💖";
                DonateButton.Content = "💖 Donar $1 USD";
                CloseButton.Content = "Cerrar";
            }
            else
            {
                MessageText.Text = "Oh oh! It seems you're not a real Developer and you've done some kind of trick to unlock this mode... 😏\n\n" +
                                   "We invite you to become a REAL Developer by donating just $1 USD! 💰\n\n" +
                                   "$1 USD... IT'S NOTHING!!! for everything this beautiful tool offers you. ✨\n\n" +
                                   "Want to support development? 💖";
                DonateButton.Content = "💖 Donate $1 USD";
                CloseButton.Content = "Close";
            }
        }

        private void ShowProgress()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            var progress = DeveloperModeManager.GetProgress();

            ProgressItems.Children.Clear();

            // Medallas de Oro
            AddProgressItem(
                progress.HasAllGoldMedals,
                es ? "🥇 Obtener todas las medallas de ORO" : "🥇 Get all GOLD medals"
            );

            // Features visitadas
            AddProgressItem(
                progress.AllFeaturesVisited,
                es ? $"🎯 Visitar todas las funciones ({progress.FeaturesVisited}/{progress.TotalFeatures})"
                   : $"🎯 Visit all features ({progress.FeaturesVisited}/{progress.TotalFeatures})"
            );

            // Donación
            AddProgressItem(
                progress.HasDonated,
                es ? "💰 Donar mínimo $1 USD" : "💰 Donate at least $1 USD"
            );
        }

        private void AddProgressItem(bool completed, string text)
        {
            var textBlock = new TextBlock
            {
                Text = completed ? $"✅ {text}" : $"❌ {text}",
                Foreground = new SolidColorBrush(completed
                    ? (Color)ColorConverter.ConvertFromString("#22C55E")
                    : (Color)ColorConverter.ConvertFromString("#EF4444")),
                FontSize = 12,
                Margin = new Thickness(0, 3, 0, 0),
                FontFamily = new FontFamily("Bahnschrift Light")
            };

            ProgressItems.Children.Add(textBlock);
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://ko-fi.com/leuandev", // Cambia por tu link real
                    UseShellExecute = true
                });
            }
            catch { }

            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}