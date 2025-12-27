using ModernDesign.Localization;
using ModernDesign.Profile;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class MedalPopupView : Window
    {
        private MedalType _medal;
        private MediaPlayer _mediaPlayer;

        public MedalPopupView(MedalType medal)
        {
            InitializeComponent();
            _medal = medal;
            ConfigureMedal();
            CenterWindow();
            PlaySound();
        }

        private void CenterWindow()
        {
            // Centrar en la pantalla principal
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            this.Left = (screenWidth - this.Width) / 2;
            this.Top = (screenHeight - this.Height) / 2;
        }

        private void PlaySound()
        {
            try
            {
                _mediaPlayer = new MediaPlayer();

                // Obtener la ruta del ejecutable
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string soundPath = System.IO.Path.Combine(baseDir, "Assets", "Sounds", "medal_sound.mp3");

                if (System.IO.File.Exists(soundPath))
                {
                    _mediaPlayer.Open(new Uri(soundPath, UriKind.Absolute));
                    _mediaPlayer.Volume = 0.5;
                    _mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                // Para debug: ver qué error da
                MessageBox.Show($"Error al reproducir sonido: {ex.Message}");
            }
        }

        private void ConfigureMedal()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string medalEmoji;
            string medalName;
            Color medalColor;
            Color borderColor;

            switch (_medal)
            {
                case MedalType.Bronze:
                    medalEmoji = "🥉";
                    medalName = es ? "Bronce" : "Bronze";
                    medalColor = (Color)ColorConverter.ConvertFromString("#CD7F32");
                    borderColor = (Color)ColorConverter.ConvertFromString("#8B4513");
                    break;
                case MedalType.Silver:
                    medalEmoji = "🥈";
                    medalName = es ? "Plata" : "Silver";
                    medalColor = (Color)ColorConverter.ConvertFromString("#C0C0C0");
                    borderColor = (Color)ColorConverter.ConvertFromString("#A8A8A8");
                    break;
                case MedalType.Gold:
                    medalEmoji = "🥇";
                    medalName = es ? "Oro" : "Gold";
                    medalColor = (Color)ColorConverter.ConvertFromString("#FFD700");
                    borderColor = (Color)ColorConverter.ConvertFromString("#DAA520");
                    break;
                default:
                    Close();
                    return;
            }

            MedalEmoji.Text = medalEmoji;
            MedalText.Text = es ? $"¡Medalla de {medalName}!" : $"{medalName} Medal!";
            MedalText.Foreground = new SolidColorBrush(medalColor);
            MedalBorder.BorderBrush = new SolidColorBrush(borderColor);
            GlowEffect.Color = medalColor;

            // Aplicar color al emoji mediante el glow
            EmojiGlow.Color = medalColor;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Asegurar centrado perfecto
            CenterWindow();

            // Aparición instantánea (sin fade in)
            MainGrid.Opacity = 1;

            // Animación Fade Out (después de 2.5 segundos)
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                BeginTime = TimeSpan.FromSeconds(1.0),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (s, args) =>
            {
                // Liberar recursos del MediaPlayer
                _mediaPlayer?.Stop();
                _mediaPlayer?.Close();
                Close();
            };

            MainGrid.BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}