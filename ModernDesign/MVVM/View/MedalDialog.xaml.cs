using ModernDesign.Localization;
using ModernDesign.Profile;
using System.Windows;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class MedalDialog : Window
    {
        private readonly string _tutorialId;
        private readonly string _tutorialName;

        public MedalDialog(string tutorialId, string tutorialName)
        {
            InitializeComponent();
            _tutorialId = tutorialId;
            _tutorialName = tutorialName;
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            TitleText.Text = es ? "🎉 ¡Tutorial Completado!" : "🎉 Tutorial Completed!";
            SubtitleText.Text = es
                ? $"¿Cómo calificarías tu entendimiento de:\n{_tutorialName}?"
                : $"Rate your understanding of:\n{_tutorialName}";

            BronzeText.Text = es ? "Bronce" : "Bronze";
            BronzeDesc.Text = es ? "Entendimiento\nbásico" : "Basic\nunderstanding";

            SilverText.Text = es ? "Plata" : "Silver";
            SilverDesc.Text = es ? "Buen\ndominio" : "Good\ngrasp";

            GoldText.Text = es ? "Oro" : "Gold";
            GoldDesc.Text = es ? "¡Lo dominé!" : "Mastered\nit!";

            HintText.Text = es
                ? "💡 Siempre puedes volver a hacer el tutorial para mejorar tu medalla"
                : "💡 You can always retake the tutorial to improve your medal";

            SkipButton.Content = es ? "Omitir (Sin medalla)" : "Skip (No medal)";
        }

        private void BronzeButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMedalAndClose(MedalType.Bronze);
        }

        private void SilverButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMedalAndClose(MedalType.Silver);
        }

        private void GoldButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMedalAndClose(MedalType.Gold);
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveMedalAndClose(MedalType medal)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            // Guardar la medalla
            ProfileManager.SetTutorialMedal(_tutorialId, medal);

            // Mostrar mensaje de éxito
            string medalName;
            switch (medal)
            {
                case MedalType.Bronze:
                    medalName = es ? "Bronce" : "Bronze";
                    break;
                case MedalType.Silver:
                    medalName = es ? "Plata" : "Silver";
                    break;
                case MedalType.Gold:
                    medalName = es ? "Oro" : "Gold";
                    break;
                default:
                    medalName = "";
                    break;
            }

            string message = es
                ? $"¡Felicidades! Has obtenido la medalla de {medalName} 🎉"
                : $"Congratulations! You've earned the {medalName} medal 🎉";

            MessageBox.Show(message,
                es ? "Medalla Obtenida" : "Medal Earned",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
    }
}