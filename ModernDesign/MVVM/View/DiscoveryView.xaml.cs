using Microsoft.Win32;
using ModernDesign.Localization;
using ModernDesign.Managers;
using ModernDesign.Profile;
using ModernDesign.Profile;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Threading;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class DiscoveryView : UserControl
    {
        private int _currentLesson = 1;
        private const int TotalLessons = 8;
        private string _overlayModsFolderPath;
        private string _overlayBaseFolderPath;
        private bool _overlayModsExists;

        public DiscoveryView()
        {
            InitializeComponent();


        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLanguage();
        }

        #region Multi-language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            TitleText.Text = es ? "Centro de Descubrimiento" : "Discovery Hub";
            SubtitleText.Text = es ? "Aprende todo sobre cómo modear The Sims 4" : "Learn everything about modding The Sims 4";

            FeaturedBadge.Text = es ? "⭐ DESTACADO" : "⭐ FEATURED";
            FeaturedTitle.Text = es ? "Guía completa para principiantes en mods" : "Complete Beginner's Guide to Modding";
            FeaturedDesc.Text = es ? "Parte desde cero: aprende a instalar, organizar y gestionar tus primeros mods." : "Start from zero: learn to install, organize and manage your first mods.";

            GettingStartedTitle.Text = es ? "🚀 Empezando" : "🚀 Getting Started";
            InstallModsTitle.Text = es ? "Instalar mods" : "Install Mods";
            InstallModsDesc.Text = es ? "Dónde poner los archivos .package y .ts4script" : "Where to put .package and .ts4script files";
            FindModsTitle.Text = es ? "Encontrar mods" : "Find Mods";
            FindModsDesc.Text = es ? "Mejores webs para descargar mods seguros" : "Best websites to download safe mods";
            OrganizeTitle.Text = es ? "Mod Manager" : "Mod Manager";
            OrganizeDesc.Text = es ? "Estructura de carpetas y buenas prácticas" : "Folder structure and best practices";

            CreateModsTitle.Text = es ? "🎨 Crea tus propios mods" : "🎨 Create Your Own";
            S4STitle.Text = "Basic Modding";
            S4SDesc.Text = es ? "Crear Interacciones, trabajos, carreras, etc" : "Create Jobs, Interactions, Holidays, Careers, Buffs, etc";
            BlenderTitle.Text = es ? "3D con Blender" : "3D with Blender";
            BlenderDesc.Text = es ? "Meshes personalizados" : "Custom meshes";
            ScriptingTitle.Text = es ? "Scripts en Python" : "Python Scripts";
            ScriptingDesc.Text = es ? "Bases para mods de script" : "Script mods basics";
            TuningTitle.Text = es ? "Ajustes XML" : "XML Tuning";
            TuningDesc.Text = es ? "Modificar valores del juego" : "Modify game values";

            TroubleshootTitle.Text = es ? "🔧 Solución de problemas" : "🔧 Troubleshooting";
            FixErrorsTitle.Text = es ? "❌ Errores comunes" : "❌ Fix Common Errors";
            FixErrorsDesc.Text = es ? "LastException, mods rotos, crasheos del juego" : "LastException, broken mods, game crashes";
            Method5050Title.Text = es ? "🔎 Método 50/50" : "🔎 50/50 Method";
            Method5050Desc.Text = es ? "Encuentra mods problemáticos rápido" : "Find problematic mods quickly";

            // Botón Profile
            ProfileButton.Content = es ? "👤 Perfil" : "👤 Profile";

            OverlayPrevButton.Content = es ? "Volver" : "Back";
            OverlayNextButton.Content = es ? "Siguiente lección" : "Next lesson";
            BeginnerCloseButton.Content = es ? "Cerrar" : "Close";
            OverlaySelectFolderButton.Content = es ? "¿Esta no es tu carpeta?" : "Not your folder?";

            // ============ SECTION: UTILITY ============
            UtilityTitle.Text = es ? "🛠️ Utilidades" : "🛠️ Utility";


            LoadingScreenTitle.Text = es ? "Loading Screen" : "Loading Screen";
            LoadingScreenDesc.Text = es ? "Personaliza pantallas de carga" : "Customize loading screens";

            CheatsGuideTitle.Text = es ? "Guía de Trucos" : "Cheats Guide";
            CheatsGuideDesc.Text = es ? "Todos los trucos y comandos del juego" : "All game cheats and commands";

            // ============ SECTION: YOU ============
            GalleryManagerTitle.Text = es ? "Gestor de Fotos" : "Screenshot Manager";
            GalleryManagerDesc.Text = es ? "Administra tu contenido de tus fotos" : "Manage your screenshot content";

            MusicManagerTitle.Text = es ? "Gestor de Música" : "Music Manager";
            MusicManagerDesc.Text = es ? "Música personalizada para tu juego" : "Custom music for your game";

            GameplayEnhancerTitle.Text = es ? "Mejoras de Juego" : "Gameplay Enhancer";
            GameplayEnhancerDesc.Text = es ? "Mejora tu experiencia de juego" : "Improve your gameplay experience";

            AutoExtractorCardTitle1.Text = es ? "Auto Extraer" : "Auto Extractor";
            AutoExtractorCardDesc1.Text = es ? "Extrae e instala automáticamente tus DLCs en formato .zip" : "Extracts and installs automatically your DLc's downloaded as .zip";

            InstallBaseGameTitle.Text = es ? "Instalar Sims 4" : "Install Sims 4";
            InstallBaseGameDesc.Text = es ? "Instala el juego de los Sims 4" : "Download the gamebase of The Sims 4";

            RepairGameTitle.Text = es ? "Reparar Juego" : "Repair Game";
            RepairGameDesc.Text = es ? "¿Juego con problemas?, ¡Reparalo aqui!" : "Your game is having troubles? repair it here!";

            LanguageSelectorTitle1.Text = es ? "Cambiar Idioma de los Sims" : "Change Sims Language";
            LanguageSelectorDesc1.Text = es ? "Cambia el Idioma del juego" : "Change the sims 4 game language";

            CrackingToolTitle1.Text = es ? "Crackear Juego" : "Cracking Tool";
            CrackingToolDesc1.Text = es ? "Crackea tu juego original" : "Make a legit version a cracked version";
            UpdateLessonUI();
        }
        #endregion

        #region Overlay Navigation
        private void FeaturedCard_Click(object sender, MouseButtonEventArgs e)
        {
            _currentLesson = 1;
            _overlayModsFolderPath = null;
            _overlayBaseFolderPath = null;
            _overlayModsExists = false;

            BeginnerGuideOverlay.Visibility = Visibility.Visible;
            AnimateOverlay(0.95, 1.0);
            UpdateLessonUI();
            CheckModsFolderOverlay();
        }

        private void CloseBeginnerGuideOverlay_Click(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.95,
                Duration = TimeSpan.FromMilliseconds(150),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            anim.Completed += (s, _) => BeginnerGuideOverlay.Visibility = Visibility.Collapsed;
            BeginnerOverlayScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            BeginnerOverlayScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void AnimateOverlay(double from, double to)
        {
            var anim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            BeginnerOverlayScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            BeginnerOverlayScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void OverlayPrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLesson > 1)
            {
                _currentLesson--;
                UpdateLessonUI();
                if (_currentLesson == 1)
                    CheckModsFolderOverlay();
            }
        }

        private void OverlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentLesson == 1 && !_overlayModsExists)
            {
                OverlayStatusText.Text = ServiceLocator.Get<ILanguageManager>().IsSpanish
                    ? "❌ Asegúrate de crear primero la carpeta Mods antes de continuar."
                    : "❌ Please make sure the Mods folder exists before continuing.";
                return;
            }

            if (_currentLesson < TotalLessons)
            {
                _currentLesson++;

                // Asignar medallas automáticamente según el progreso
                if (_currentLesson == 1)
                {
                    ProfileManager.SetTutorialMedal("beginner_guide", MedalType.Bronze);
                    ShowMedalNotification(MedalType.Bronze);
                }
                else if (_currentLesson == 3)
                {
                    ProfileManager.SetTutorialMedal("beginner_guide", MedalType.Silver);
                    ShowMedalNotification(MedalType.Silver);
                }
                else if (_currentLesson == TotalLessons)
                {
                    ProfileManager.SetTutorialMedal("beginner_guide", MedalType.Gold);
                    ShowMedalNotification(MedalType.Gold);
                }

                UpdateLessonUI();
            }
        }

        private bool _isShowingMedal = false;

        private void ShowMedalNotification(MedalType medal)
        {
            // Prevenir spam de medallas
            if (_isShowingMedal) return;
            _isShowingMedal = true;

            // Deshabilitar botones de navegación
            OverlayNextButton.IsEnabled = false;
            OverlayPrevButton.IsEnabled = false;
            BeginnerCloseButton.IsEnabled = false;

            // Mostrar popup de medalla
            var medalPopup = new MedalPopupView(medal);
            medalPopup.Closed += (s, e) =>
            {
                // Después de cerrar el popup, animar la medalla hacia el botón
                AnimateMedalToButton(medal, "beginner_guide");

                // Re-habilitar botones
                OverlayNextButton.IsEnabled = true;
                OverlayPrevButton.IsEnabled = true;
                BeginnerCloseButton.IsEnabled = true;
                _isShowingMedal = false;
            };
            medalPopup.Show();
        }

        private void AnimateMedalToButton(MedalType medal, string tutorialId)
        {
            // Obtener la posición del botón FeaturedCard
            var targetButton = FeaturedCard;
            var targetPosition = targetButton.TransformToAncestor(this).Transform(new Point(0, 0));

            // Crear el emoji de medalla
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

            // Crear el elemento visual de la medalla
            var medalBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, medalColor.R, medalColor.G, medalColor.B)),
                BorderBrush = new SolidColorBrush(medalColor),
                BorderThickness = new Thickness(3),
                CornerRadius = new CornerRadius(25),
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            medalBorder.Effect = new DropShadowEffect
            {
                Color = medalColor,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            var medalText = new TextBlock
            {
                Text = medalEmoji,
                FontSize = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            medalBorder.Child = medalText;

            // Agregar al grid principal
            var mainGrid = this.Content as Grid;
            if (mainGrid != null)
            {
                // Posición inicial (centro de la pantalla)
                var transform = new TranslateTransform
                {
                    X = (this.ActualWidth / 2) - 25,
                    Y = (this.ActualHeight / 2) - 25
                };
                medalBorder.RenderTransform = transform;

                Panel.SetZIndex(medalBorder, 10000);
                mainGrid.Children.Add(medalBorder);

                // Calcular posición final (esquina superior izquierda del botón)
                double targetX = targetPosition.X + 10;
                double targetY = targetPosition.Y + 10;

                // Animación de movimiento
                var animX = new DoubleAnimation
                {
                    From = transform.X,
                    To = targetX,
                    Duration = TimeSpan.FromMilliseconds(800),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                var animY = new DoubleAnimation
                {
                    From = transform.Y,
                    To = targetY,
                    Duration = TimeSpan.FromMilliseconds(800),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                // Animación de escala (se hace más pequeña)
                var scaleTransform = new ScaleTransform(1, 1);
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleTransform);
                transformGroup.Children.Add(transform);
                medalBorder.RenderTransform = transformGroup;

                var animScale = new DoubleAnimation
                {
                    From = 1,
                    To = 0.6,
                    Duration = TimeSpan.FromMilliseconds(800),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };

                animScale.Completed += (s, e) =>
                {
                    // Remover el elemento animado
                    mainGrid.Children.Remove(medalBorder);

                    // Actualizar el indicador de medalla en el botón
                    UpdateMedalBadgeOnButton(targetButton, medal);
                };

                transform.BeginAnimation(TranslateTransform.XProperty, animX);
                transform.BeginAnimation(TranslateTransform.YProperty, animY);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animScale);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animScale);
            }
        }

        private void UpdateMedalBadgeOnButton(Border targetButton, MedalType medal)
        {
            // Buscar si ya existe un badge de medalla
            var grid = targetButton.Child as Grid;
            if (grid == null) return;

            // Remover badge anterior si existe
            Border existingBadge = null;
            foreach (var child in grid.Children)
            {
                if (child is Border b && b.Name == "MedalBadge")
                {
                    existingBadge = b;
                    break;
                }
            }

            if (existingBadge != null)
                grid.Children.Remove(existingBadge);

            // Crear nuevo badge
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
                Name = "MedalBadge",
                Background = new SolidColorBrush(Color.FromArgb(220, medalColor.R, medalColor.G, medalColor.B)),
                BorderBrush = new SolidColorBrush(medalColor),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(20),
                Width = 40,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(15, 15, 0, 0) // Mucho más arriba y más a la esquina para Discovery
            };

            medalBadge.Effect = new DropShadowEffect
            {
                Color = medalColor,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.9
            };

            medalBadge.Child = new TextBlock
            {
                Text = medalEmoji,
                FontSize = 24,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Panel.SetZIndex(medalBadge, 10);
            grid.Children.Add(medalBadge);
        }
        #endregion

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new Window
            {
                Content = new UserProfileCard(),
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                Background = Brushes.Transparent,
                AllowsTransparency = true
            };

            profileWindow.ShowDialog();
        }

        #region Lesson UI
        private void UpdateLessonUI()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            LessonIndicator.Text = es
                ? $"Lección {_currentLesson} de {TotalLessons}"
                : $"Lesson {_currentLesson} of {TotalLessons}";

            OverlayPrevButton.Visibility = _currentLesson > 1 ? Visibility.Visible : Visibility.Collapsed;
            OverlayNextButton.Visibility = _currentLesson < TotalLessons ? Visibility.Visible : Visibility.Collapsed;
            OverlayNextButton.Content = _currentLesson == TotalLessons - 1
                ? (es ? "Finalizar" : "Finish")
                : (es ? "Siguiente lección" : "Next lesson");

            OverlayStatusBorder.Visibility = _currentLesson == 1 ? Visibility.Visible : Visibility.Collapsed;
            Lesson1ButtonsPanel.Visibility = _currentLesson == 1 ? Visibility.Visible : Visibility.Collapsed;

            OverlayNextButton.IsEnabled = _currentLesson != 1 || _overlayModsExists;

            LessonContentPanel.Children.Clear();
            LinksContainer.Children.Clear();
            LinksPanel.Visibility = Visibility.Collapsed;

            switch (_currentLesson)
            {
                case 1: BuildLesson1(es); break;
                case 2: BuildLesson2(es); break;
                case 3: BuildLesson3(es); break;
                case 4: BuildLesson4(es); break;
                case 5: BuildLesson5(es); break;
                case 6: BuildLesson6(es); break;
                case 7: BuildLesson7(es); break;
                case 8: BuildLesson8(es); break;
            }
        }

        private void BuildLesson1(bool es)
        {
            LessonTitleText.Text = es ? "Lección 1: Tu carpeta Mods" : "Lesson 1: Your Mods folder";
            LessonIntroText.Text = es
                ? "Vamos a comprobar si tu juego puede usar mods y, si no, la arreglamos creando la carpeta Mods correcta."
                : "We will check if your game can use mods and, if not, fix it by creating the correct Mods folder.";

            bool esSpanish = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            AddStep("1", "#22C55E",
                es ? "Abre la carpeta de Documentos de The Sims 4" : "Open The Sims 4 Documents folder",
                es ? "Normalmente está en Documents/Electronic Arts/Los Sims 4."
                    : "Usually in Documents/Electronic Arts/The Sims 4.");

            AddStep("2", "#38BDF8",
                es ? "Ubica o crea la carpeta Mods" : "Locate / create the Mods folder",
                es ? "Aquí van tus archivos .package y .ts4script."
                   : "This is where your .package and .ts4script files go.");

            UpdateVisuals("/Assets/Discovery/ModsStep1.png");

            AddLinks(es,
                ("https://www.ea.com/es/games/the-sims/the-sims-4/new-player-hub/mods",
                    es ? "• Ayuda oficial: EA – Custom Content & Mods" : "• Official help: EA – Custom Content & Mods"),
                ("https://www.youtube.com/results?search_query=how+to+install+mods+the+sims+4",
                    es ? "• Video tutorial: YouTube – Cómo instalar mods" : "• Video tutorial: YouTube – How to install mods"));
        }

        private void BuildLesson2(bool es)
        {
            LessonTitleText.Text = es ? "Lección 2: Activar los mods en el juego" : "Lesson 2: Enable mods in-game";
            LessonIntroText.Text = es
                ? "Ahora que tu carpeta Mods está lista, tienes que activar el contenido personalizado y los mods de script dentro del juego."
                : "Now that your Mods folder is ready, you must enable custom content and script mods inside the game.";

            AddStep("1", "#F97316",
                es ? "Abre las Opciones de juego" : "Open Game Options",
                es ? "En el menú principal, haz clic en Opciones → Opciones de juego."
                   : "In the main menu, click Options → Game Options.");

            AddStep("2", "#22C55E",
                es ? "Activa contenido personalizado y mods de script" : "Enable custom content and script mods",
                es ? "Ve a la pestaña 'Otro' y activa ambas opciones, luego reinicia el juego."
                   : "Go to the 'Other' tab and enable both options, then restart the game.");

            AddStep("3", "#38BDF8",
                es ? "Revisa la lista de mods al iniciar" : "Check the Mods list on startup",
                es ? "Si todo está bien, al iniciar el juego aparecerá una ventana listando tus mods instalados."
                   : "If everything is correct, a window listing your installed mods will appear when the game starts.");

            UpdateVisuals("/Assets/Discovery/ActivateMods.jpg");

        }

        private void BuildLesson3(bool es)
        {
            LessonTitleText.Text = es ? "Lección 3: ¡Instalar todos los mods que quieras!" : "Lesson 3: Install all the mods you want!";
            LessonIntroText.Text = es
                ? "Ahora puedes descargar e instalar mods. Aquí tienes las mejores páginas para encontrar mods seguros."
                : "Now you can download and install mods. Here are the best sites to find safe mods.";

            AddStep("1", "#22D3EE",
                es ? "Descarga mods de sitios confiables" : "Download mods from trusted sites",
                es ? "Evita sitios sospechosos. Usa las páginas recomendadas abajo."
                   : "Avoid suspicious sites. Use the recommended pages below.");

            AddStep("2", "#A78BFA",
                es ? "Extrae los archivos" : "Extract the files",
                es ? "Si descargaste un .zip, extrae los archivos .package y .ts4script."
                   : "If you downloaded a .zip, extract the .package and .ts4script files.");

            AddStep("3", "#22C55E",
                es ? "Colócalos en tu carpeta Mods" : "Place them in your Mods folder",
                es ? "Arrastra los archivos a la carpeta Mods que configuraste en la Lección 1."
                   : "Drag the files to the Mods folder you set up in Lesson 1.");

            UpdateVisuals("/Assets/Discovery/InfinityMods.png");

            AddLinks(es,
                ("https://www.thesimsresource.com/", "• The Sims Resource (TSR)"),
                ("https://www.patreon.com/", "• Patreon (creators)"),
                ("https://modthesims.info/", "• Mod The Sims"),
                ("https://www.curseforge.com/sims4", "• CurseForge"),
                ("https://tumblr.com/tagged/ts4cc", "• Tumblr #ts4cc"));
        }

        private void BuildLesson4(bool es)
        {
            LessonTitleText.Text = es ? "Lección 4: Optimiza con FPS Booster" : "Lesson 4: Optimize with FPS Booster";
            LessonIntroText.Text = es
                ? "Después de instalar mods, usa FPS Booster de esta app para detectar archivos que pueden ralentizar tu juego."
                : "After installing mods, use FPS Booster from this app to detect files that may slow down your game.";

            AddStep("1", "#EF4444",
                es ? "Abre FPS Booster en esta app" : "Open FPS Booster in this app",
                es ? "Ve al menú lateral y selecciona 'FPS Booster'."
                   : "Go to the side menu and select 'FPS Booster'.");

            AddStep("2", "#F97316",
                es ? "Analiza tu carpeta Mods" : "Analyze your Mods folder",
                es ? "FPS Booster escaneará y te mostrará archivos pesados o problemáticos."
                   : "FPS Booster will scan and show you heavy or problematic files.");

            AddStep("3", "#22C55E",
                es ? "Sigue las recomendaciones" : "Follow the recommendations",
                es ? "Elimina o reemplaza los archivos que más impactan el rendimiento."
                   : "Remove or replace files that impact performance the most.");

            UpdateVisuals("/Assets/Discovery/fpsbooster.png");

        }

        private void BuildLesson5(bool es)
        {

            LessonTitleText.Text = es ? "Lección 5: Diferencia entre .package y .ts4script" : "Lesson 6: Difference between .package and .ts4script";
            LessonIntroText.Text = es
                ? "Es crucial entender la diferencia para evitar errores."
                : "It's crucial to understand the difference to avoid errors.";

            AddStep("📦", "#22C55E",
                ".package",
                es
                    ? "Contienen objetos, ropa, texturas, tuning XML. Pueden estar en subcarpetas y se pueden mergear."
                    : "Contain objects, clothing, textures, XML tuning. Can be in subfolders and can be merged.");

            AddStep("💻", "#38BDF8",
                ".ts4script",
                es
                    ? "Contienen código Python. DEBEN estar en la raíz de Mods, SIN Subcarpetas. NO se pueden mergear ni anidar."
                    : "Contain Python code. MUST be in Mods root WITHOUT subfolders. CANNOT be merged or nested.");

            AddWarning(es
                ? "⚠️ Importante: Los .ts4script NUNCA deben estar dentro de carpetas anidadas (ej: Mods/Scripts/Autor/mod.ts4script NO funcionará). Máximo: Mods/mod.ts4script"
                : "⚠️ Important: .ts4script files must NEVER be in nested folders (e.g., Mods/Scripts/Author/mod.ts4script will NOT work). Max: Mods/mod.ts4script");

            UpdateVisuals("/Assets/Discovery/package_vs_script.png");
        }

        private void BuildLesson6(bool es)
        {
            LessonTitleText.Text = es ? "Lección 6: Merge Packages con Sims 4 Studio" : "Lesson 5: Merge Packages with Sims 4 Studio";
            LessonIntroText.Text = es
                ? "Aprende a combinar múltiples .package en uno solo para que el juego cargue más rápido."
                : "Learn to combine multiple .package files into one so the game loads faster.";

            AddStep("1", "#22D3EE",
                es ? "Descarga Sims 4 Studio" : "Download Sims 4 Studio",
                es ? "Es gratuito y esencial para crear y gestionar mods."
                   : "It's free and essential for creating and managing mods.");

            AddStep("2", "#A78BFA",
                es ? "Usa 'Content Management' → 'Merge Packages'" : "Use 'Content Management' → 'Merge Packages'",
                es ? "Selecciona los .package que quieras combinar y añadelos."
                   : "Select the .package files you want to combine and add them.");

            AddStep("3", "#F97316",
                es ? "Guarda el nuevo .package" : "Save the new .package",
                es ? "Reemplaza / Desactiva los originales, y deja solo el archivo .package nuevo que creaste."
                   : "Remove / Deactivate the originals, and leave only the combined .package file you just created.");

            AddWarning(es
                ? "⚠️ Advertencia: Mergear packages puede dificultar identificar qué mods tienes instalados con herramientas como 'Mod Manager', ya que busca por nombres de archivo."
                : "⚠️ Warning: Merging packages can make it harder to identify which mods you have installed with tools like 'Mod Manager', as it searches by file names.");

            UpdateVisuals("/Assets/Discovery/mergepackages.png");


            AddLinks(es,
                ("https://sims4studio.com/", "• Sims 4 Studio (download)"));

            AddLinks(es,
                ("https://www.youtube.com/results?search_query=how+to+install+sims+4+studio", "YT Video Tutorial"),
                ("https://sims4studio.com/", "• Sims 4 Studio (download)"));
        


        }

        private void BuildLesson7(bool es)
        {
            LessonTitleText.Text = es ? "Lección 7: ¡Disfruta y mantente actualizado!" : "Lesson 7: Enjoy and stay updated!";
            LessonIntroText.Text = es
                ? "¡Ya estás listo para disfrutar tus mods! Pero recuerda mantenerlos actualizados."
                : "You're ready to enjoy your mods! But remember to keep them updated.";

            AddStep("🎮", "#22C55E",
                es ? "¡Juega y disfruta!" : "Play and enjoy!",
                es ? "Inicia el juego y prueba todos tus nuevos mods."
                   : "Start the game and try all your new mods.");

            AddStep("🔔", "#F97316",
                es ? "Mantente atento a actualizaciones" : "Stay alert for updates",
                es ? "Después de cada parche del juego, revisa si tus mods necesitan actualización."
                   : "After each game patch, check if your mods need updating.");

            AddStep("🗑️", "#EF4444",
                es ? "Elimina mods desactualizados" : "Remove outdated mods",
                es ? "Los mods rotos pueden causar errores. Revisa el archivo LastException."
                   : "Broken mods can cause errors. Check the LastException file.");

            UpdateVisuals("/Assets/Discovery/ending.png");


            AddLinks(es,
                ("https://twitter.com/search?q=sims4%20mod%20update",
                    es ? "• Twitter: Actualizaciones de mods" : "• Twitter: Mod updates"),
                ("https://www.patreon.com/",
                    es ? "• Patreon: Sigue a tus creadores favoritos" : "• Patreon: Follow your favorite creators"));
        }

        private void BuildLesson8(bool es)
        {
            LessonTitleText.Text = es ? "Lección 8: ¿Crear tu primer mod?" : "Lesson 8: Create your first mod?";
            LessonIntroText.Text = es
                ? "¿Listo para el siguiente nivel? Aprende a crear tus propios mods usando las herramientas del Discovery Hub."
                : "Ready for the next level? Learn to create your own mods using the Discovery Hub tools.";

            AddStep("🛠️", "#22D3EE",
                "Sims 4 Studio y Mod Constructor v5",
                es ? "Crea CC, recolors y edita objetos existentes."
                   : "Create CC, recolors and edit existing objects.");

            AddStep("🎭", "#EA580C",
                "Blender",
                es ? "Crea meshes 3D personalizados para ropa y objetos."
                   : "Create custom 3D meshes for clothing and objects.");

            AddStep("💻", "#21b96b",
                es ? "Scripts en Python" : "Python Scripts",
                es ? "Añade nuevas funcionalidades al juego con código."
                   : "Add new functionality to the game with code.");

            AddStep("⚙️", "#6366F1",
                "XML Tuning",
                es ? "Modifica valores del juego como precios, tiempos, etc."
                   : "Modify game values like prices, times, etc.");

            var btn = new Button
            {
                Content = es ? "🎨 Explorar herramientas de creación" : "🎨 Explore creation tools",
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 10, 0, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED")),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => { CloseBeginnerGuideOverlay_Click(s, e); };
            LessonContentPanel.Children.Add(btn);

            UpdateVisuals("/Assets/Discovery/createyourmod.png");

        }
        #endregion

        #region UI Helpers
        private void AddStep(string num, string color, string title, string desc)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827"))
            };

            var sp = new StackPanel { Orientation = Orientation.Horizontal };

            sp.Children.Add(new TextBlock
            {
                Text = num,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top
            });

            var inner = new StackPanel();
            inner.Children.Add(new TextBlock
            {
                Text = title,
                Style = (Style)Resources["TitleTextBlock"],
                FontSize = 12
            });
            inner.Children.Add(new TextBlock
            {
                Text = desc,
                Style = (Style)Resources["BodyTextBlock"],
                FontSize = 10
            });

            sp.Children.Add(inner);
            border.Child = sp;
            LessonContentPanel.Children.Add(border);
        }

        private void AddWarning(string text)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"))
            };

            border.Child = new TextBlock
            {
                Text = text,
                Style = (Style)Resources["BodyTextBlock"],
                FontSize = 10,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"))
            };

            LessonContentPanel.Children.Add(border);
        }

        private void AddLinks(bool es, params (string url, string text)[] links)
        {
            LinksPanel.Visibility = Visibility.Visible;
            BeginnerLinksTitle.Text = es ? "Links recomendados" : "Recommended links";

            foreach (var (url, text) in links)
            {
                var tb = new TextBlock { FontSize = 10, Style = (Style)Resources["BodyTextBlock"] };
                var hl = new Hyperlink { NavigateUri = new Uri(url) };
                hl.Inlines.Add(text);
                hl.RequestNavigate += Hyperlink_RequestNavigate;
                tb.Inlines.Add(hl);
                LinksContainer.Children.Add(tb);
            }
        }

        private void UpdateVisuals(string imagePath)
        {
            try
            {
                GuideScreenshotImage.Source =
                    new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            catch
            {
                // ignore
            }

            // Opcional: limpias cualquier cosa visual vieja
            ZoomBorder.BorderBrush = Brushes.Transparent;

            if (ZoomBorder.Effect is DropShadowEffect dse)
                dse.Color = Colors.Transparent;

            ZoomLabelBorder.Background = Brushes.Transparent;
            BeginnerZoomLabel.Text = string.Empty;
        }
        #endregion

        #region Mods Folder Check
        private void CheckModsFolderOverlay()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] basePaths =
                {
                    Path.Combine(docs, "Electronic Arts", "The Sims 4"),
                    Path.Combine(docs, "Electronic Arts", "Los Sims 4"),
                    Path.Combine(docs, "Origin", "The Sims 4"),
                    Path.Combine(docs, "Origin", "Los Sims 4")
                };

                _overlayBaseFolderPath = null;
                _overlayModsFolderPath = null;
                _overlayModsExists = false;

                foreach (var bp in basePaths)
                {
                    if (Directory.Exists(bp) && _overlayBaseFolderPath == null)
                        _overlayBaseFolderPath = bp;

                    string mods = Path.Combine(bp, "Mods");
                    if (Directory.Exists(mods))
                    {
                        _overlayModsFolderPath = mods;
                        _overlayModsExists = true;
                        break;
                    }
                }

                OverlayStatusBorder.Visibility = Visibility.Visible;
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (_overlayModsFolderPath != null)
                {
                    OverlayFixButton.Visibility = Visibility.Collapsed;
                    OverlayStatusBorder.Background =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#064E3B"));
                    OverlayStatusText.Foreground =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6EE7B7"));

                    OverlayStatusText.Text = es
                        ? $"✅ Se encontró una carpeta Mods válida:\n{_overlayModsFolderPath}\n\nTu juego puede usar mods sin problemas."
                        : $"✅ A valid Mods folder was found:\n{_overlayModsFolderPath}\n\nYour game can use mods correctly.";

                    _overlayModsExists = true;
                    OverlayNextButton.IsEnabled = true;
                }
                else if (_overlayBaseFolderPath != null)
                {
                    OverlayFixButton.Visibility = Visibility.Visible;
                    OverlayStatusBorder.Background =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
                    OverlayStatusText.Foreground =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));

                    OverlayStatusText.Text = es
                        ? $"❌ No se encontró la carpeta Mods en:\n{_overlayBaseFolderPath}\n\nPulsa \"Fix\" para crearla automáticamente."
                        : $"❌ Mods folder was not found in:\n{_overlayBaseFolderPath}\n\nPress \"Fix\" to create it automatically.";

                    _overlayModsExists = false;
                    OverlayNextButton.IsEnabled = false;
                }
                else
                {
                    OverlayFixButton.Visibility = Visibility.Collapsed;
                    OverlayStatusBorder.Background =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
                    OverlayStatusText.Foreground =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));

                    OverlayStatusText.Text = es
                        ? "❌ No se encontró la carpeta Mods en ninguna de las rutas típicas.\n\nNo se pudo detectar automáticamente la carpeta de documentos de tu juego. Usa \"¿Esta no es tu carpeta?\" para seleccionarla manualmente."
                        : "❌ Mods folder was not found in any of the typical paths.\n\nWe couldn't auto-detect your game's documents folder. Use \"Not your folder?\" to select it manually.";

                    _overlayModsExists = false;
                    OverlayNextButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                OverlayStatusBorder.Background =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
                OverlayStatusText.Foreground =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));

                _overlayModsExists = false;
                OverlayStatusText.Text = ServiceLocator.Get<ILanguageManager>().IsSpanish
                    ? $"No se pudo comprobar la carpeta Mods:\n{ex.Message}"
                    : $"Could not check Mods folder:\n{ex.Message}";

                OverlayNextButton.IsEnabled = false;
            }
        }

        private void OverlayFixButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (string.IsNullOrEmpty(_overlayBaseFolderPath))
                {
                    OverlayStatusBorder.Background =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
                    OverlayStatusText.Foreground =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));

                    OverlayStatusText.Text = es
                        ? "❌ No se encontró la carpeta Mods en ninguna de las rutas típicas.\n\nNo se pudo detectar automáticamente la carpeta de documentos de tu juego. Usa \"¿Esta no es tu carpeta?\" para seleccionarla manualmente."
                        : "❌ Mods folder was not found in any of the typical paths.\n\nWe couldn't auto-detect your game's documents folder. Use \"Not your folder?\" to select it manually.";

                    _overlayModsExists = false;
                    OverlayNextButton.IsEnabled = false;
                    return;
                }

                string modsPath = Path.Combine(_overlayBaseFolderPath, "Mods");
                if (!Directory.Exists(modsPath))
                    Directory.CreateDirectory(modsPath);

                _overlayModsFolderPath = modsPath;
                _overlayModsExists = true;

                CheckModsFolderOverlay();
                UpdateLessonUI();
            }
            catch (Exception ex)
            {
                OverlayStatusBorder.Background =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
                OverlayStatusText.Foreground =
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));

                _overlayModsExists = false;
                OverlayStatusText.Text = ServiceLocator.Get<ILanguageManager>().IsSpanish
                    ? $"No se pudo crear la carpeta Mods:\n{ex.Message}"
                    : $"Could not create Mods folder:\n{ex.Message}";
                OverlayNextButton.IsEnabled = false;
            }
        }

        // Agregar este método helper en DiscoveryView.xaml.cs

        private Border CreateTutorialCard(string tutorialId, string title, string desc, string emoji, string colorStart, string colorEnd, bool es, MouseButtonEventHandler clickHandler)
        {
            var card = new Border
            {
                Width = 230,
                Height = 120,
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(0, 0, 15, 15),
                Style = (Style)Resources["GuideCard"]
            };

            card.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop((Color)ColorConverter.ConvertFromString(colorStart), 0),
            new GradientStop((Color)ColorConverter.ConvertFromString("#0F172A"), 1)
        }
            };

            var grid = new Grid { Margin = new Thickness(15) };

            var mainStack = new StackPanel();
            mainStack.Children.Add(new TextBlock
            {
                Text = emoji,
                FontSize = 24,
                Margin = new Thickness(0, 0, 0, 8)
            });
            mainStack.Children.Add(new TextBlock
            {
                Text = title,
                Style = (Style)Resources["TitleTextBlock"],
                FontSize = 14
            });
            mainStack.Children.Add(new TextBlock
            {
                Text = desc,
                Style = (Style)Resources["BodyTextBlock"],
                FontSize = 11
            });

            grid.Children.Add(mainStack);

            // Medal indicator
            var medal = ProfileManager.GetTutorialMedal(tutorialId);
            if (medal != MedalType.None)
            {
                string medalEmoji;
                switch (medal)
                {
                    case MedalType.Bronze:
                        medalEmoji = "🥉";
                        break;
                    case MedalType.Silver:
                        medalEmoji = "🥈";
                        break;
                    case MedalType.Gold:
                        medalEmoji = "🥇";
                        break;
                    default:
                        medalEmoji = "";
                        break;
                }

                Color medalColor;
                switch (medal)
                {
                    case MedalType.Bronze:
                        medalColor = (Color)ColorConverter.ConvertFromString("#CD7F32");
                        break;
                    case MedalType.Silver:
                        medalColor = (Color)ColorConverter.ConvertFromString("#C0C0C0");
                        break;
                    case MedalType.Gold:
                        medalColor = (Color)ColorConverter.ConvertFromString("#FFD700");
                        break;
                    default:
                        medalColor = Colors.Transparent;
                        break;
                }

                var medalBorder = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(40, medalColor.R, medalColor.G, medalColor.B)),
                    BorderBrush = new SolidColorBrush(medalColor),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(6, 3, 6, 3),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top
                };

                medalBorder.Child = new TextBlock
                {
                    Text = medalEmoji,
                    FontSize = 16
                };

                medalBorder.Effect = new DropShadowEffect
                {
                    Color = medalColor,
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.6
                };

                grid.Children.Add(medalBorder);
            }

            card.Child = grid;
            card.MouseLeftButtonUp += clickHandler;

            return card;
        }

        // Ejemplo de uso en S4SCard_Click modificado:
        private void S4SCard_Click(object sender, MouseButtonEventArgs e)
        {
            var owner = Window.GetWindow(this);
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            var dialog = new ModdingLevelDialog
            {
                Owner = owner,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (dialog.ShowDialog() == true)
            {
                Window win = null;

                switch (dialog.SelectedLevel)
                {
                    case "basic":
                        win = new S4SCategoriesWindow
                        {
                            Owner = owner,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        break;

                    case "intermediate":
                        win = new IntermediateCategoryWindow
                        {
                            Owner = owner,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        break;

                    case "highlevel":
                        win = new HighCategoryWindow
                        {
                            Owner = owner,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        break;
                }

                if (win != null)
                {
                    win.ShowDialog();

                    // Refrescar las medallas después de cerrar el tutorial
                    RefreshMedalIndicators();
                }
            }
        }

        private void RefreshMedalIndicators()
        {
            // Forzar actualización visual
            // Esto depende de cómo estructures tus cards
            // Podrías recrear las cards dinámicamente o usar data binding
        }

        private void OverlaySelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = es
                    ? "Selecciona la carpeta de documentos de The Sims 4 (donde está o debería estar la carpeta Mods)"
                    : "Select The Sims 4 documents folder (where the Mods folder is or should be)",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selected = dialog.SelectedPath;
                string modsPath = Path.Combine(selected, "Mods");

                if (Directory.Exists(modsPath))
                {
                    _overlayBaseFolderPath = selected;
                    _overlayModsFolderPath = modsPath;
                    _overlayModsExists = true;
                }
                else
                {
                    var result = MessageBox.Show(
                        es
                            ? $"No se encontró una carpeta 'Mods' en:\n{selected}\n\n¿Deseas crearla ahora?"
                            : $"No 'Mods' folder found in:\n{selected}\n\nDo you want to create it now?",
                        es ? "Crear carpeta Mods" : "Create Mods folder",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            Directory.CreateDirectory(modsPath);
                            _overlayBaseFolderPath = selected;
                            _overlayModsFolderPath = modsPath;
                            _overlayModsExists = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                es
                                    ? $"Error al crear la carpeta:\n{ex.Message}"
                                    : $"Error creating folder:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        _overlayBaseFolderPath = selected;
                        _overlayModsFolderPath = null;
                        _overlayModsExists = false;
                    }
                }

                CheckModsFolderOverlay();
                UpdateLessonUI();
            }
        }
        #endregion

        #region Links & URLs
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OpenUrl(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ServiceLocator.Get<ILanguageManager>().IsSpanish
                        ? $"No se pudo abrir el enlace:\n{ex.Message}"
                        : $"Could not open link:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Card Clicks


        private void SaveGameManagerCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new SaveGamesView
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }

        private void InstallModsCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new InstallModsCheckWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("install_mods");
        }

        private void FindModsCard_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://www.thesimsresource.com/");
        }

        private void OrganizeCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new OrganizeModsWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("mod_manager");

        }

        private void BlenderCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new ThreeDModelingWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }

        private void ModGalleryCard_Click(object sender, MouseButtonEventArgs e)
        {
            // Tu lógica aquí
        }

        private void InstallBaseGameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new InstallMethodSelectorWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }

        private void RepairGameCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new RepairLoggerWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }

        private void LanguageSelector_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new LanguageSelectorWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }

        private void CrackingToolCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new CrackingToolWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
        }
        private void ScriptingCard_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://sims4studio.com/thread/15145/started-python-scripting");
        }

        private void TuningCard_Click(object sender, MouseButtonEventArgs e)
        {
            OpenUrl("https://leroidetout.medium.com/sims-4-tuning-101-a-deep-dive-into-how-tuning-is-generated-from-python-part-1-3086efab9e6f");
        }

        private void FixErrorsCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new FixCommonErrorsWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("fix_common_errors");

        }

        private void Method5050Card_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new Method5050Window
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("method_5050");

        }


        private void LoadingScreenCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new LoadingScreenSelectorWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("loading_screen");
        }

        private void CheatsGuideCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new CheatsGuideView
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("cheats_guide");
        }

        // Categoria Tu
        private void GalleryManagerCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new GalleryManagerWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("gallery_manager");
        }

        private void MusicManagerCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new MusicManagerView
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("music_manager");
        }

        private void GameplayEnhancerCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new GameplayEnhancerView
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
            DeveloperModeManager.MarkFeatureAsVisited("gameplay_enhancer");
        }

        private void AutoExtractorCard_Click(object sender, MouseButtonEventArgs e)
        {
            var win = new SemiAutoInstallerWindow
            {
                Owner = Window.GetWindow(this),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            win.ShowDialog();
        }
        #endregion
    }
}
