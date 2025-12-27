using ModernDesign.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeuanS4ToolKit.Core;
using WinForms = System.Windows.Forms;

namespace ModernDesign.MVVM.View
{
    public partial class LoadingScreenSelectorWindow : Window
    {
        private string _modsFolder = "";
        private List<LoadingScreenItem> _loadingScreens = new List<LoadingScreenItem>();

        public LoadingScreenSelectorWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            InitializeLoadingScreens();
        }

        private const string GLITCHED_ICON_FIX_URL = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/LeuanToolKit_LoadingScreen_FixGlitchedIcon.package"; // Reemplaza con URL real
        private const string GLITCHED_ICON_FIX_FILENAME = "LeuanToolKit_GlitchedIconFix.package";

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            this.Title = es ? "Selector de Pantallas de Carga" : "Loading Screen Selector";
            TitleText.Text = es ? "🎨 Selector de Pantallas de Carga" : "🎨 Loading Screen Selector";
            SubtitleText.Text = es
                ? "Elige y personaliza tus pantallas de carga del juego"
                : "Choose and customize your game loading screens";

            ChangeFolderButton.Content = es ? "📂 Cambiar" : "📂 Change";
            RandomizeTitle.Text = es ? "Aleatorizar Pantalla de Carga" : "Randomize Loading Screen";
            RandomizeDesc.Text = es
                ? "Cambia la pantalla de carga aleatoriamente en cada inicio del juego"
                : "Change loading screen randomly on each game launch";
            RandomizeButton.Content = es ? "Habilitar" : "Enable";

            GlitchedIconTitle.Text = es ? "¿Icono Glitcheado?" : "Glitched Icon?";
            GlitchedIconDesc.Text = es
                ? "Corrige el parpadeo del icono verde de Los Sims a gris/glitcheado después de instalar una loading screen"
                : "Fix green Sims icon flickering to gray/glitched after installing loading screen";
            GlitchedIconButton.Content = es ? "Reparar Ahora" : "Fix Now";
        }

        private void InitializeLoadingScreens()
        {
            // Definir las 10 pantallas de carga disponibles con URLs de imágenes
            _loadingScreens = new List<LoadingScreenItem>
            {
                new LoadingScreenItem
                {
                    Name = "Hello Sunshine",
                    NameES = "Hello Sunshine",
                    Description = "Soft pastel colors with dreamy aesthetics",
                    DescriptionES = "Colores pastel suaves con estética soñadora",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tumblr%20loading%20screen%201.package",
                    FileName = "LeuanToolKit_LoadingScreen_HelloSunshine.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tum%20screen%201.png", // Reemplaza con URL real
                    GradientStart = "#F472B6",
                    GradientEnd = "#C084FC"
                },
                new LoadingScreenItem
                {
                    Name = "Flowers Sunset",
                    NameES = "Flores en Ocaso",
                    Description = "Soft pastel colors with dreamy aesthetics",
                    DescriptionES = "Colores pastel suaves con estética soñadora",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tumblr%20loading%20screen%202.package",
                    FileName = "LeuanToolKit_LoadingScreen_FlowersSunset.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tum%20screen%202.png",
                    GradientStart = "#06B6D4",
                    GradientEnd = "#8B5CF6"
                },
                new LoadingScreenItem
                {
                    Name = "Feathers",
                    NameES = "Plumas",
                    Description = "Soft pastel colors with dreamy aesthetics",
                    DescriptionES = "Colores pastel suaves con estética soñadora",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tumblr%20loading%20screen%203.package",
                    FileName = "LeuanToolKit_LoadingScreen_Feathers.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/tum%20screen%203.png",
                    GradientStart = "#22C55E",
                    GradientEnd = "#84CC16"
                },
                new LoadingScreenItem
                {
                    Name = "Vintage Retro",
                    NameES = "Retro Vintage",
                    Description = "Retro 80s and 90s vibes",
                    DescriptionES = "Vibras retro de los 80s y 90s",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/citiskyline.package",
                    FileName = "LeuanToolKit_LoadingScreen_VintageRetro.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/citiesskyline.jpg",
                    GradientStart = "#F59E0B",
                    GradientEnd = "#DC2626"
                },
                new LoadingScreenItem
                {
                    Name = "Sylvan Glade",
                    NameES = "Glaceado de Leuan",
                    Description = "No description.",
                    DescriptionES = "Sin descripcion.",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/SylvanGlade.package",
                    FileName = "LeuanToolKit_LoadingScreen_GalaxySpace.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/SylvanGlade.gif",
                    GradientStart = "#6366F1",
                    GradientEnd = "#1F2937"
                },
                new LoadingScreenItem
                {
                    Name = "Minimalist Dark",
                    NameES = "Negro Minimalista",
                    Description = "Clean and minimalist black design",
                    DescriptionES = "Diseño negro limpio y minimalista",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/teanmoon_DarkerLoadingScreens_BG.package",
                    FileName = "LeuanToolKit_LoadingScreen_MinimalistWhite.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/DarkLoadingScreen.png",
                    GradientStart = "#F3F4F6",
                    GradientEnd = "#9CA3AF"
                },
                new LoadingScreenItem
                {
                    Name = "Brindleton",
                    NameES = "Brindleton",
                    Description = "Clean and minimalist white design",
                    DescriptionES = "Diseño blanco limpio y minimalista",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/brindleton3.package",
                    FileName = "LeuanToolKit_LoadingScreen_MinimalistWhite.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/brindleton.jpeg",
                    GradientStart = "#F3F4F6",
                    GradientEnd = "#9CA3AF"
                },
                new LoadingScreenItem
                {
                    Name = "Warm Nature",
                    NameES = "Naturaleza Acogedora",
                    Description = "Clean and minimalist nature design",
                    DescriptionES = "Diseño de naturaleza limpio y minimalista",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/Sunivaa_WarmNatureloadingScreen01.package",
                    FileName = "LeuanToolKit_LoadingScreen_MinimalistWhite.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/WarmNatureLoading01.png",
                    GradientStart = "#F3F4F6",
                    GradientEnd = "#9CA3AF"
                },
                new LoadingScreenItem
                {
                    Name = "WW1",
                    NameES = "WW1",
                    Description = "Clean and minimalist werewolf design",
                    DescriptionES = "Diseño de lobos limpio y minimalista",
                    DownloadUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/WW1.package",
                    FileName = "LeuanToolKit_LoadingScreen_MinimalistWhite.package",
                    ImageUrl = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LoadingScreens/WW1.png",
                    GradientStart = "#F3F4F6",
                    GradientEnd = "#9CA3AF"
                }
            };
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DetectModsFolder();
            CreateLoadingScreenCards();
            UpdateRandomizeButtonState();
            UpdateGlitchedIconPanelVisibility();

        }

        private async Task DetectModsFolder()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string[] possiblePaths = new string[]
                {
                    Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Mods"),
                    Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Mods")
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        _modsFolder = path;
                        ModsFolderText.Text = AbbreviatePath(_modsFolder, 70);
                        return;
                    }
                }

                ModsFolderText.Text = es ? "Carpeta Mods no detectada" : "Mods folder not detected";
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                ModsFolderText.Text = es ? "Error detectando carpeta" : "Error detecting folder";
            }
        }

        private string AbbreviatePath(string path, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            int charsToShow = (maxLength - 3) / 2;
            return path.Substring(0, charsToShow) + "..." + path.Substring(path.Length - charsToShow);
        }

        private void CreateLoadingScreenCards()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            foreach (var screen in _loadingScreens)
            {
                Border card = new Border
                {
                    Style = (Style)FindResource("LoadingScreenCard")
                };

                Grid cardGrid = new Grid();
                cardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(180) }); // Era 160, ahora 180
                cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Thumbnail Image
                Border imageBorder = new Border
                {
                    Height = 180, // Era 160, ahora 180
                    CornerRadius = new CornerRadius(16, 16, 0, 0),
                    ClipToBounds = true
                };

                // Background gradient si no hay imagen
                LinearGradientBrush gradient = new LinearGradientBrush();
                gradient.StartPoint = new Point(0, 0);
                gradient.EndPoint = new Point(1, 1);
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(screen.GradientStart), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(screen.GradientEnd), 1));

                Grid imageGrid = new Grid
                {
                    Background = gradient
                };

                // Intentar cargar imagen
                try
                {
                    Image thumbnail = new Image
                    {
                        Stretch = Stretch.UniformToFill,
                        Source = new BitmapImage(new Uri(screen.ImageUrl))
                    };
                    imageGrid.Children.Add(thumbnail);
                }
                catch
                {
                    // Si falla, mostrar solo el gradiente con el nombre
                    TextBlock placeholderText = new TextBlock
                    {
                        Text = es ? screen.NameES : screen.Name,
                        Foreground = Brushes.White,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        FontFamily = new FontFamily("Bahnschrift Light"),
                        FontStyle = FontStyles.Italic,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };
                    placeholderText.Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        ShadowDepth = 2,
                        Opacity = 0.9,
                        BlurRadius = 10
                    };
                    imageGrid.Children.Add(placeholderText);
                }

                // Overlay oscuro
                Border overlay = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0))
                };
                imageGrid.Children.Add(overlay);

                imageBorder.Child = imageGrid;
                Grid.SetRow(imageBorder, 0);
                cardGrid.Children.Add(imageBorder);

                // Info Panel
                Border infoPanel = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                    CornerRadius = new CornerRadius(0, 0, 16, 16),
                    Padding = new Thickness(15, 12, 15, 15)
                };

                Grid infoGrid = new Grid();
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                infoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                TextBlock nameText = new TextBlock
                {
                    Text = es ? screen.NameES : screen.Name,
                    Foreground = Brushes.White,
                    FontSize = 15,
                    FontWeight = FontWeights.Bold,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontStyle = FontStyles.Italic,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                nameText.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    ShadowDepth = 1,
                    Opacity = 0.8,
                    BlurRadius = 6
                };
                Grid.SetRow(nameText, 0);
                infoGrid.Children.Add(nameText);

                TextBlock descText = new TextBlock
                {
                    Text = es ? screen.DescriptionES : screen.Description,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    FontSize = 11,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold,
                    FontStyle = FontStyles.Italic,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 4, 0, 12), // Era 10, ahora 12
                    Height = 30 // Era 32, ahora 30
                };
                Grid.SetRow(descText, 1);
                infoGrid.Children.Add(descText);

                // Download Button
                Button downloadBtn = new Button
                {
                    Content = "⬇️ " + (es ? "Descargar" : "Download"),
                    Style = (Style)FindResource("ActionButton"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Tag = screen
                };

                // Verificar si está instalada
                string installedPath = Path.Combine(_modsFolder, screen.FileName);
                bool isInstalled = !string.IsNullOrEmpty(_modsFolder) && File.Exists(installedPath);

                if (isInstalled)
                {
                    downloadBtn.Content = "✅ " + (es ? "Instalado" : "Installed");
                    downloadBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                    downloadBtn.IsEnabled = false;
                }
                else
                {
                    downloadBtn.Content = "⬇️ " + (es ? "Descargar" : "Download");
                }


                downloadBtn.Click += DownloadButton_Click;
                Grid.SetRow(downloadBtn, 2);
                infoGrid.Children.Add(downloadBtn);

                infoPanel.Child = infoGrid;
                Grid.SetRow(infoPanel, 1);
                cardGrid.Children.Add(infoPanel);

                card.Child = cardGrid;
                LoadingScreensPanel.Children.Add(card);
            }
        }

        private async void GlitchedIconButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (string.IsNullOrEmpty(_modsFolder) || !Directory.Exists(_modsFolder))
                {
                    MessageBox.Show(
                        es ? "Por favor selecciona la carpeta Mods primero." : "Please select the Mods folder first.",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    es ? "¿Descargar el fix para el icono glitcheado?\n\nEsto instalará un archivo que corrige el parpadeo del icono verde de Los Sims."
                       : "Download the glitched icon fix?\n\nThis will install a file that fixes the green Sims icon flickering.",
                    es ? "Confirmar" : "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                GlitchedIconButton.IsEnabled = false;
                GlitchedIconButton.Content = es ? "⏳ Descargando..." : "⏳ Downloading...";

                // Descargar el archivo
                string destinationPath = Path.Combine(_modsFolder, GLITCHED_ICON_FIX_FILENAME);
                await DownloadFile(GLITCHED_ICON_FIX_URL, destinationPath);

                GlitchedIconButton.Content = "✅ " + (es ? "Instalado" : "Installed");
                GlitchedIconButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

                MessageBox.Show(
                    es ? "¡Fix instalado correctamente!\n\nReinicia el juego para que los cambios surtan efecto."
                       : "Fix installed successfully!\n\nRestart the game for changes to take effect.",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                UpdateGlitchedIconPanelVisibility();

                await Task.Delay(2000);
                GlitchedIconButton.IsEnabled = true;
                GlitchedIconButton.Content = es ? "Fix Now" : "Fix Now";
                GlitchedIconButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error descargando fix: " : "Error downloading fix: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                GlitchedIconButton.IsEnabled = true;
                GlitchedIconButton.Content = es ? "Fix Now" : "Fix Now";
                GlitchedIconButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                Button btn = sender as Button;
                LoadingScreenItem screen = btn.Tag as LoadingScreenItem;

                if (string.IsNullOrEmpty(_modsFolder) || !Directory.Exists(_modsFolder))
                {
                    MessageBox.Show(
                        es ? "Por favor selecciona la carpeta Mods primero." : "Please select the Mods folder first.",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Confirmar descarga
                var result = MessageBox.Show(
                    es ? $"¿Descargar '{screen.NameES}'?\n\nEsto desactivará cualquier otra pantalla de carga activa."
                       : $"Download '{screen.Name}'?\n\nThis will disable any other active loading screen.",
                    es ? "Confirmar Descarga" : "Confirm Download",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                btn.IsEnabled = false;
                btn.Content = es ? "⏳ Descargando..." : "⏳ Downloading...";

                // Desactivar otras loading screens
                await DisableOtherLoadingScreens();

                // Desactivar SimMattically si existe
                DisableSimMatticallyLoadingScreen();

                // Descargar el archivo
                string destinationPath = Path.Combine(_modsFolder, screen.FileName);
                await DownloadFile(screen.DownloadUrl, destinationPath);

                btn.Content = "✅ " + (es ? "Instalado" : "Installed");
                btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));

                MessageBox.Show(
                    es ? $"¡Pantalla de carga '{screen.NameES}' instalada correctamente!\n\nReinicia el juego para ver los cambios."
                       : $"Loading screen '{screen.Name}' installed successfully!\n\nRestart the game to see changes.",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reactivar todos los botones
                RefreshAllButtons();
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error descargando: " : "Error downloading: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                RefreshAllButtons();
            }
        }

        private async Task DisableOtherLoadingScreens()
        {
            try
            {
                // Buscar todos los archivos de loading screen de LeuanToolKit
                var loadingScreenFiles = Directory.GetFiles(_modsFolder, "LeuanToolKit_LoadingScreen_*.package", SearchOption.TopDirectoryOnly);

                foreach (var file in loadingScreenFiles)
                {
                    // Renombrar a .leupackage para desactivar
                    string newPath = Path.ChangeExtension(file, ".leupackage");
                    if (File.Exists(file) && !File.Exists(newPath))
                    {
                        File.Move(file, newPath);
                    }
                }
            }
            catch { }
        }

        private void DisableSimMatticallyLoadingScreen()
        {
            try
            {
                // Buscar archivos que contengan "SimMattically" y "LoadingScreen" en el nombre
                var files = Directory.GetFiles(_modsFolder, "*LeuanToolKit*LoadingScreen*.package", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    string newPath = Path.ChangeExtension(file, ".leupackage");
                    if (File.Exists(file) && !File.Exists(newPath))
                    {
                        File.Move(file, newPath);
                    }
                }
            }
            catch { }
        }

        private async Task DownloadFile(string url, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] data = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(destinationPath, data);
            }
        }

        private void RefreshAllButtons()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            foreach (Border card in LoadingScreensPanel.Children)
            {
                Grid cardGrid = card.Child as Grid;
                if (cardGrid != null)
                {
                    foreach (var child in cardGrid.Children)
                    {
                        if (child is Border border && border.Child is Grid infoGrid)
                        {
                            foreach (var infoChild in infoGrid.Children)
                            {
                                if (infoChild is Button btn && btn.Tag is LoadingScreenItem screen)
                                {
                                    // Verificar si el archivo existe
                                    string installedPath = Path.Combine(_modsFolder, screen.FileName);
                                    bool isInstalled = !string.IsNullOrEmpty(_modsFolder) && File.Exists(installedPath);

                                    if (isInstalled)
                                    {
                                        btn.Content = "✅ " + (es ? "Instalado" : "Installed");
                                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                                        btn.IsEnabled = false;
                                    }
                                    else
                                    {
                                        btn.Content = "⬇️ " + (es ? "Descargar" : "Download");
                                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                                        btn.IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void RandomizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                // Obtener el estado actual
                bool isCurrentlyEnabled = GetRandomizeLoadingScreenSetting();

                if (isCurrentlyEnabled)
                {
                    // Desactivar
                    var result = MessageBox.Show(
                        es ? "¿Desactivar la aleatorización de pantallas de carga?"
                           : "Disable loading screen randomization?",
                        es ? "Confirmar" : "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SetRandomizeLoadingScreenSetting(false);
                        RandomizeButton.Content = es ? "Habilitar" : "Enable";
                        RandomizeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B5CF6"));

                        MessageBox.Show(
                            es ? "Aleatorización desactivada."
                               : "Randomization disabled.",
                            es ? "Éxito" : "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                else
                {
                    // Activar
                    var result = MessageBox.Show(
                        es ? "¿Habilitar la aleatorización de pantallas de carga?\n\nLa pantalla de carga cambiará aleatoriamente en cada inicio del juego."
                           : "Enable loading screen randomization?\n\nLoading screen will change randomly on each game launch.",
                        es ? "Confirmar" : "Confirm",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        SetRandomizeLoadingScreenSetting(true);
                        RandomizeButton.Content = es ? "Desactivar" : "Disable";
                        RandomizeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));

                        MessageBox.Show(
                            es ? "¡Aleatorización habilitada!\n\nLa pantalla de carga cambiará aleatoriamente en cada inicio del juego."
                               : "Randomization enabled!\n\nLoading screen will change randomly on each game launch.",
                            es ? "Éxito" : "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error: " : "Error: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool GetRandomizeLoadingScreenSetting()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string profilePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "profile.ini");

                if (!File.Exists(profilePath))
                    return false;

                var lines = File.ReadAllLines(profilePath);
                bool inMiscSection = false;

                foreach (var line in lines)
                {
                    string trimmed = line.Trim();

                    if (trimmed == "[Misc]")
                    {
                        inMiscSection = true;
                        continue;
                    }

                    if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                    {
                        inMiscSection = false;
                        continue;
                    }

                    if (inMiscSection && trimmed.StartsWith("randomizeLoadingScreen", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = trimmed.Split('=');
                        if (parts.Length >= 2)
                        {
                            string value = parts[1].Trim();
                            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
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

        private void SetRandomizeLoadingScreenSetting(bool enabled)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string toolkitFolder = Path.Combine(appData, "Leuan's - Sims 4 ToolKit");
                string profilePath = Path.Combine(toolkitFolder, "profile.ini");

                // Crear carpeta si no existe
                if (!Directory.Exists(toolkitFolder))
                    Directory.CreateDirectory(toolkitFolder);

                List<string> lines;

                if (File.Exists(profilePath))
                {
                    lines = File.ReadAllLines(profilePath).ToList();
                }
                else
                {
                    lines = File.ReadAllLines(profilePath).ToList();
                }

                // Buscar si existe la sección [Misc]
                int miscSectionIndex = -1;
                int randomizeLineIndex = -1;

                for (int i = 0; i < lines.Count; i++)
                {
                    string trimmed = lines[i].Trim();

                    if (trimmed == "[Misc]")
                    {
                        miscSectionIndex = i;
                    }

                    if (miscSectionIndex != -1 && trimmed.StartsWith("randomizeLoadingScreen", StringComparison.OrdinalIgnoreCase))
                    {
                        randomizeLineIndex = i;
                        break;
                    }

                    // Si encontramos otra sección después de [Misc], salir
                    if (miscSectionIndex != -1 && trimmed.StartsWith("[") && trimmed != "[Misc]")
                    {
                        break;
                    }
                }

                string newValue = $"randomizeLoadingScreen = {(enabled ? "true" : "false")}";

                if (randomizeLineIndex != -1)
                {
                    // Actualizar línea existente
                    lines[randomizeLineIndex] = newValue;
                }
                else if (miscSectionIndex != -1)
                {
                    // Agregar a sección [Misc] existente
                    lines.Insert(miscSectionIndex + 1, newValue);
                }
                else
                {
                    // Crear sección [Misc] al final
                    lines.Add("");
                    lines.Add("[Misc]");
                    lines.Add(newValue);
                }

                // Guardar archivo
                File.WriteAllLines(profilePath, lines);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving profile.ini: {ex.Message}");
            }
        }

        private void UpdateRandomizeButtonState()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            bool isEnabled = GetRandomizeLoadingScreenSetting();

            if (isEnabled)
            {
                RandomizeButton.Content = es ? "Desactivar" : "Disable";
                RandomizeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
            else
            {
                RandomizeButton.Content = es ? "Habilitar" : "Enable";
                RandomizeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B5CF6"));
            }
        }

        private void UpdateGlitchedIconPanelVisibility()
        {
            try
            {
                if (string.IsNullOrEmpty(_modsFolder) || !Directory.Exists(_modsFolder))
                {
                    GlitchedIconPanel.Visibility = Visibility.Visible;
                    return;
                }

                string fixPath = Path.Combine(_modsFolder, GLITCHED_ICON_FIX_FILENAME);

                if (File.Exists(fixPath))
                {
                    GlitchedIconPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    GlitchedIconPanel.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                GlitchedIconPanel.Visibility = Visibility.Visible;
            }
        }

        private async void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            var dialog = new WinForms.FolderBrowserDialog
            {
                Description = es ? "Selecciona la carpeta Mods de The Sims 4" : "Select The Sims 4 Mods folder",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                _modsFolder = dialog.SelectedPath;
                ModsFolderText.Text = AbbreviatePath(_modsFolder, 70);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }

    // Clase auxiliar para los items de loading screen
    public class LoadingScreenItem
    {
        public string Name { get; set; }
        public string NameES { get; set; }
        public string Description { get; set; }
        public string DescriptionES { get; set; }
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
        public string ImageUrl { get; set; } // NUEVO
        public string GradientStart { get; set; }
        public string GradientEnd { get; set; }
    }
}