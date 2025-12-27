using System.Windows;
using System.Windows.Input;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class ModdingLevelDialog : Window
    {
        public string SelectedLevel { get; private set; }

        public ModdingLevelDialog()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            Title = es ? "Comienza a Crear" : "Start Creating";
            TitleText.Text = es ? "¿Listo para Crear Tu Primer Mod?" : "Ready to Create Your First Mod?";
            DescText.Text = es ? "Comienza desde lo básico y aprende paso a paso. ¡No necesitas experiencia previa!" : "Start from the basics and learn step by step. No prior experience needed!";
            BasicTitle.Text = es ? "Empezar a Aprender" : "Start Learning Now";
            CancelButton.Content = es ? "Quizás Después" : "Maybe Later";
        }

        private void BasicCard_Click(object sender, MouseButtonEventArgs e)
        {
            SelectedLevel = "basic";
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}