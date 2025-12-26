using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class AnnouncementWindow : Window
    {
        public AnnouncementWindow(string announcementText, string imageUrl = null, string logoUrl = null)
        {
            InitializeComponent();
            this.Loaded += AnnouncementWindow_Loaded;

            // Establecer el texto del anuncio
            AnnouncementTextBlock.Text = announcementText;

            // Cargar imagen si existe
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    AnnouncementImage.Source = new BitmapImage(new Uri(imageUrl));
                    ImageBorder.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Si falla la carga de imagen, simplemente no la mostramos
                    ImageBorder.Visibility = Visibility.Collapsed;
                }
            }

            // Cargar logo si existe
            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                try
                {
                    LogoImage.Source = new BitmapImage(new Uri(logoUrl));
                    LogoImage.Visibility = Visibility.Visible;
                }
                catch
                {
                    // Si falla la carga del logo, simplemente no lo mostramos
                    LogoImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AnnouncementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            HeaderText.Text = LanguageManager.Get("AnnouncementWindow.HeaderText");
            CloseButton.Content = LanguageManager.Get("AnnouncementWindow.CloseButton");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}