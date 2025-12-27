using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Controls;
using ModernDesign.Localization;
using ModernDesign.Profile;
using System.IO.Compression;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class S4SCategoriesWindow : Window
    {
        private const string TOOLS_DOWNLOAD_URL = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/Leuans%20Sims%204%20ToolKit%20-%20Modding%20Tools.zip"; // Reemplaza con tu URL
        private bool _isDownloading = false;

        public S4SCategoriesWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            LoadMedalBadges();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Crear un Mod - Categorías" : "Create a Mod - Categories";
            TitleText.Text = es ? "🎨 Elige qué Crear" : "🎨 Choose What to Create";
            SubtitleText.Text = es ? "Selecciona una categoría para ver tutoriales paso a paso" : "Select a category to see step-by-step tutorials";

            CardInteractionTitle.Text = es ? "Interacción Social" : "Social Interaction";
            CardInteractionDesc.Text = es ? "Crea interacciones personalizadas" : "Create custom interactions";

            CardCareerTitle.Text = es ? "Carrera" : "Career";
            CardCareerDesc.Text = es ? "Crea una nueva carrera" : "Create a new career path";

            CardTraitTitle.Text = es ? "Rasgo" : "Trait";
            CardTraitDesc.Text = es ? "Crea un rasgo de personalidad" : "Create a personality trait";

            CardSocialEventTitle.Text = es ? "Ropa" : "Clothing";
            CardSocialEventDesc.Text = es ? "Crea o recolorea ropa" : "Create or recolor clothes";

            CardHolidayTitle.Text = es ? "Objeto / CC" : "Object / CC";
            CardHolidayDesc.Text = es ? "Crea muebles o decoración" : "Create furniture or decor";

            CardBuffTitle.Text = es ? "Buff / Moodlet" : "Buff / Moodlet";
            CardBuffDesc.Text = es ? "Crea estados emocionales" : "Create emotional states";

            DownloadToolsTitle.Text = es ? "📦 Herramientas de Modding Requeridas" : "📦 Modding Tools Required";
            DownloadToolsDesc.Text = es
                ? "Descarga e instala todas las herramientas necesarias para empezar a crear mods"
                : "Download and install all necessary tools to start creating mods";
            DownloadButtonText.Text = es ? "Descargar Herramientas" : "Download Tools";

            CloseButton.Content = es ? "Cerrar" : "Close";
        }

        private void LoadMedalBadges()
        {
            // Cargar medallas existentes para cada categoría
            UpdateCardMedal(CardTrait, "tutorial_trait");
            UpdateCardMedal(CardInteraction, "tutorial_interaction");
            UpdateCardMedal(CardCareer, "tutorial_career");
            UpdateCardMedal(CardBuff, "tutorial_buff");
            UpdateCardMedal(CardSocialEvent, "tutorial_clothing");
            UpdateCardMedal(CardHoliday, "tutorial_object");
        }

        private void UpdateCardMedal(Border card, string tutorialId)
        {
            var medal = ProfileManager.GetTutorialMedal(tutorialId);
            if (medal == MedalType.None) return;

            var grid = card.Child as Grid;
            if (grid == null) return;

            string medalEmoji;
            Color medalColor;

            switch (medal)
            {
                case MedalType.Bronze:
                    medalEmoji = "🥉";
                    medalColor = (Color)ColorConverter.ConvertFromString("#CD7F32");
                    break;
                case MedalType.Silver:
                    medalEmoji = "🥈";
                    medalColor = (Color)ColorConverter.ConvertFromString("#C0C0C0");
                    break;
                case MedalType.Gold:
                    medalEmoji = "🥇";
                    medalColor = (Color)ColorConverter.ConvertFromString("#FFD700");
                    break;
                default:
                    return;
            }

            var medalBadge = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, medalColor.R, medalColor.G, medalColor.B)),
                BorderBrush = new SolidColorBrush(medalColor),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(15),
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5, 5, 0, 0)
            };

            medalBadge.Effect = new DropShadowEffect
            {
                Color = medalColor,
                BlurRadius = 12,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            medalBadge.Child = new TextBlock
            {
                Text = medalEmoji,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Panel.SetZIndex(medalBadge, 10);
            grid.Children.Add(medalBadge);
        }

        private void CardInteraction_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("interaction");
        private void CardCareer_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("career");
        private void CardTrait_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("trait");
        private void CardObject_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("object");
        private void CardClothing_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("clothing");
        private void CardBuff_Click(object sender, MouseButtonEventArgs e) => OpenTutorial("buff");

        private void OpenTutorial(string category)
        {
            var win = new S4STutorialWindow(category) { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            win.ShowDialog();

            // Recargar medallas después de cerrar el tutorial
            LoadMedalBadges();
        }

        private async void DownloadToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isDownloading)
                return;

            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;


            _isDownloading = true;
            DownloadToolsButton.IsEnabled = false;
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadPercentText.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            DownloadButtonText.Text = es ? "Descargando..." : "Downloading...";

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string targetFolder = Path.Combine(desktopPath, "Leuan - Sims 4 ToolKit");
                string tempZipFile = Path.Combine(Path.GetTempPath(), "s4s_tools.zip");

                // Crear carpeta de destino si no existe
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                // Descargar el archivo con progreso
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, args) =>
                    {
                        DownloadProgressBar.Value = args.ProgressPercentage;
                        DownloadPercentText.Text = $"{args.ProgressPercentage}%";
                    };

                    await client.DownloadFileTaskAsync(new Uri(TOOLS_DOWNLOAD_URL), tempZipFile);
                }

                // Extraer con 7-Zip
                DownloadProgressBar.Value = 100;
                DownloadPercentText.Text = "100%";
                DownloadButtonText.Text = es ? "Extrayendo archivos..." : "Extracting files...";

                await Task.Run(() =>
                {
                    // Extraer usando .NET nativo
                    if (Directory.Exists(targetFolder))
                    {
                        Directory.Delete(targetFolder, true);
                    }
                    Directory.CreateDirectory(targetFolder);

                    ZipFile.ExtractToDirectory(tempZipFile, targetFolder);
                });

                // Eliminar archivo temporal
                if (File.Exists(tempZipFile))
                {
                    File.Delete(tempZipFile);
                }

                // Abrir carpeta
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetFolder,
                    UseShellExecute = true
                });

                MessageBox.Show(
                    es ? $"¡Herramientas descargadas y extraídas exitosamente!\n\nUbicación: {targetFolder}"
                       : $"Tools downloaded and extracted successfully!\n\nLocation: {targetFolder}",
                    es ? "Descarga Completa" : "Download Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    es ? $"Error al descargar herramientas:\n{ex.Message}"
                       : $"Error downloading tools:\n{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isDownloading = false;
                DownloadToolsButton.IsEnabled = true;
                DownloadProgressBar.Visibility = Visibility.Collapsed;
                DownloadPercentText.Visibility = Visibility.Collapsed;
                DownloadProgressBar.Value = 0;
                DownloadButtonText.Text = es ? "Descargar Herramientas" : "Download Tools";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}