using ModernDesign.Localization;
using ModernDesign.Managers;
using ModernDesign.Profile;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class UserProfileCard : UserControl
    {
        private bool _isModdingExpanded = false;
        private bool _isDeveloperExpanded = false;
        private Point _dragStartPoint;
        private bool _isDragging = false;
        private Point _resizeStartPoint;
        private bool _isResizing = false;
        private Size _resizeStartSize;

        public UserProfileCard()
        {
            InitializeComponent();

            // Habilitar eventos de mouse para drag
            this.MouseLeftButtonDown += UserProfileCard_MouseLeftButtonDown;
            this.MouseLeftButtonUp += UserProfileCard_MouseLeftButtonUp;
            this.MouseMove += UserProfileCard_MouseMove;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProfile();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void DeveloperHelp_Click(object sender, MouseButtonEventArgs e)
        {
            // Prevenir que se expanda/colapse la categoría
            e.Handled = true;

            // Abrir ventana de progreso
            var progressWindow = new DeveloperProgressWindow
            {
                Owner = Window.GetWindow(this)
            };
            progressWindow.ShowDialog();

            // Recargar el perfil por si cambió algo
            LoadProfile();
        }

        private void ModdingCategory_Click(object sender, MouseButtonEventArgs e)
        {
            _isModdingExpanded = !_isModdingExpanded;
            ModdingDetailsPanel.Visibility = _isModdingExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DeveloperCategory_Click(object sender, MouseButtonEventArgs e)
        {
            _isDeveloperExpanded = !_isDeveloperExpanded;
            DeveloperDetailsPanel.Visibility = _isDeveloperExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Drag and Resize

        private void UserProfileCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            Point mousePos = e.GetPosition(this);

            // Detectar si está en la esquina inferior derecha (zona de resize: 20x20 px)
            if (mousePos.X >= this.ActualWidth - 20 && mousePos.Y >= this.ActualHeight - 20)
            {
                _isResizing = true;
                _resizeStartPoint = e.GetPosition(window);
                _resizeStartSize = new Size(this.Width, this.Height);
                this.CaptureMouse();
                e.Handled = true;
            }
            // Si hace clic en el banner (header), permitir drag
            else if (mousePos.Y <= 120) // Altura del banner
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(window);
                this.CaptureMouse();
                e.Handled = true;
            }
        }

        private void UserProfileCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging || _isResizing)
            {
                _isDragging = false;
                _isResizing = false;
                this.ReleaseMouseCapture();
            }
        }

        private void UserProfileCard_MouseMove(object sender, MouseEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window == null) return;

            // Cambiar cursor si está sobre la esquina de resize
            Point mousePos = e.GetPosition(this);
            if (mousePos.X >= this.ActualWidth - 20 && mousePos.Y >= this.ActualHeight - 20)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }

            // Drag
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(window);
                Vector offset = currentPosition - _dragStartPoint;

                window.Left += offset.X;
                window.Top += offset.Y;
            }

            // Resize
            if (_isResizing && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(window);
                Vector offset = currentPosition - _resizeStartPoint;

                double newWidth = Math.Max(400, _resizeStartSize.Width + offset.X);
                double newHeight = Math.Max(500, _resizeStartSize.Height + offset.Y);

                this.Width = newWidth;
                this.Height = newHeight;
            }
        }

        #endregion

        public void LoadProfile()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            var profile = ProfileManager.CurrentProfile;

            if (profile != null)
            {
                // ========== USER INFO ==========
                UserNameText.Text = profile.UserName;

                // Cargar avatar personalizado
                string savedAvatar = UserSettingsManager.GetAvatar();
                AvatarEmoji.Text = savedAvatar;

                MemberSinceText.Text = es
                    ? $"Miembro desde: {profile.CreatedDate:MMM yyyy}"
                    : $"Member since: {profile.CreatedDate:MMM yyyy}";

                // ========== MEDAL COUNTS ==========
                int goldCount = ProfileManager.GetTotalMedals(MedalType.Gold);
                int silverCount = ProfileManager.GetTotalMedals(MedalType.Silver);
                int bronzeCount = ProfileManager.GetTotalMedals(MedalType.Bronze);

                GoldCount.Text = goldCount.ToString();
                SilverCount.Text = silverCount.ToString();
                BronzeCount.Text = bronzeCount.ToString();

                GoldLabel.Text = es ? "Oro" : "Gold";
                SilverLabel.Text = es ? "Plata" : "Silver";
                BronzeLabel.Text = es ? "Bronce" : "Bronze";

                // ========== USER TITLE ==========
                int totalMedals = goldCount + silverCount + bronzeCount;
                string userTitle = GetUserTitle(totalMedals, es);
                UserTitleText.Text = userTitle;

                // ========== CATEGORIES TITLE ==========
                CategoriesTitle.Text = es ? "📚 Categorías" : "📚 Categories";

                // ========== MODDING CATEGORY ==========
                MedalType moddingCategoryMedal = LoadModdingCategory(es);

                // ========== DEVELOPER MODE CATEGORY ==========
                LoadDeveloperCategory(es, moddingCategoryMedal);

                // ========== PROGRESS TEXT ==========
                if (totalMedals == 0)
                {
                    ProgressText.Text = es
                        ? "¡Completa tutoriales para ganar medallas!"
                        : "Complete tutorials to earn medals!";
                }
                else if (totalMedals < 5)
                {
                    ProgressText.Text = es
                        ? $"¡Buen comienzo! {totalMedals} {(totalMedals == 1 ? "medalla obtenida" : "medallas obtenidas")}"
                        : $"Great start! {totalMedals} {(totalMedals == 1 ? "medal" : "medals")} earned";
                }
                else if (totalMedals < 10)
                {
                    ProgressText.Text = es
                        ? $"¡Vas muy bien! {totalMedals} medallas"
                        : $"Doing great! {totalMedals} medals";
                }
                else
                {
                    ProgressText.Text = es
                        ? $"¡Eres un maestro! {totalMedals} medallas"
                        : $"You're a master! {totalMedals} medals";
                }

                // ========== CLOSE BUTTON ==========
                CloseButton.Content = es ? "✕ Cerrar" : "✕ Close";
            }
            else
            {
                UserNameText.Text = es ? "Invitado" : "Guest";
                MemberSinceText.Text = es ? "Perfil no configurado" : "Profile not set up";
                UserTitleText.Text = es ? "👋 Nuevo Usuario" : "👋 New User";
                ProgressText.Text = es
                    ? "Configura tu perfil para comenzar"
                    : "Set up your profile to get started";
                CloseButton.Content = es ? "✕ Cerrar" : "✕ Close";
            }
        }

        private string GetUserTitle(int totalMedals, bool es)
        {
            if (totalMedals == 0)
                return es ? "👋 Principiante" : "👋 Beginner";
            else if (totalMedals < 3)
                return es ? "🌱 Aprendiz" : "🌱 Apprentice";
            else if (totalMedals < 6)
                return es ? "🏆 Modding Apprentice" : "🏆 Modding Apprentice";
            else if (totalMedals < 10)
                return es ? "⚡ Modder Avanzado" : "⚡ Advanced Modder";
            else
                return es ? "💎 Maestro Modder" : "💎 Master Modder";
        }

        private MedalType LoadModdingCategory(bool es)
        {
            // Lista de tutoriales de Modding
            string[] tutorialIds = new string[]
            {
                "beginner_guide",
                "tutorial_trait",
                "tutorial_interaction",
                "tutorial_career",
                "tutorial_buff",
                "tutorial_clothing",
                "tutorial_object"
            };

            int completedCount = 0;
            int totalTutorials = tutorialIds.Length;
            int goldMedalCount = 0;

            // Cargar medallas individuales con color
            MedalType medal1 = ProfileManager.GetTutorialMedal("beginner_guide");
            SetMedalWithColor(Medal_BeginnerGuide, medal1, ref completedCount, ref goldMedalCount);

            MedalType medal2 = ProfileManager.GetTutorialMedal("tutorial_trait");
            SetMedalWithColor(Medal_Trait, medal2, ref completedCount, ref goldMedalCount);

            MedalType medal3 = ProfileManager.GetTutorialMedal("tutorial_interaction");
            SetMedalWithColor(Medal_Interaction, medal3, ref completedCount, ref goldMedalCount);

            MedalType medal4 = ProfileManager.GetTutorialMedal("tutorial_career");
            SetMedalWithColor(Medal_Career, medal4, ref completedCount, ref goldMedalCount);

            MedalType medal5 = ProfileManager.GetTutorialMedal("tutorial_buff");
            SetMedalWithColor(Medal_Buff, medal5, ref completedCount, ref goldMedalCount);

            MedalType medal6 = ProfileManager.GetTutorialMedal("tutorial_clothing");
            SetMedalWithColor(Medal_Clothing, medal6, ref completedCount, ref goldMedalCount);

            MedalType medal7 = ProfileManager.GetTutorialMedal("tutorial_object");
            SetMedalWithColor(Medal_Object, medal7, ref completedCount, ref goldMedalCount);

            // Traducir nombres de tutoriales
            Tutorial_BeginnerGuide.Text = es ? "🚀 Guía para Principiantes" : "🚀 Beginner's Guide";
            Tutorial_Trait.Text = es ? "⭐ Rasgo" : "⭐ Trait";
            Tutorial_Interaction.Text = es ? "💬 Interacción Social" : "💬 Social Interaction";
            Tutorial_Career.Text = es ? "💼 Carrera" : "💼 Career";
            Tutorial_Buff.Text = es ? "😊 Buff / Moodlet" : "😊 Buff / Moodlet";
            Tutorial_Clothing.Text = es ? "👗 Ropa" : "👗 Clothing";
            Tutorial_Object.Text = es ? "🪑 Objeto / CC" : "🪑 Object / CC";

            // Calcular medalla de categoría general con NUEVA LÓGICA
            MedalType categoryMedal = MedalType.None;

            // ORO: Solo si TODOS los tutoriales tienen medalla de ORO
            if (goldMedalCount == totalTutorials)
            {
                categoryMedal = MedalType.Gold;
            }
            // PLATA: Si todos están completados (cualquier medalla) pero no todos son oro
            else if (completedCount == totalTutorials)
            {
                categoryMedal = MedalType.Silver;
            }
            // BRONCE: Si tiene al menos 3 completados
            else if (completedCount >= 3)
            {
                categoryMedal = MedalType.Bronze;
            }

            SetMedalWithColorSimple(ModdingCategoryMedal, categoryMedal);

            // Actualizar progreso
            ModdingCategoryName.Text = es ? "Modding" : "Modding";
            ModdingCategoryProgress.Text = es
                ? $"{completedCount}/{totalTutorials} completados"
                : $"{completedCount}/{totalTutorials} completed";

            return categoryMedal;
        }


        private void LoadDeveloperCategory(bool es, MedalType moddingCategoryMedal)
        {
            // Verificar si está desbloqueado con el nuevo sistema
            bool isUnlocked = DeveloperModeManager.IsDeveloperModeUnlocked();

            // Actualizar textos según idioma
            DeveloperCategoryName.Text = es ? "Developer Mode" : "Developer Mode";

            if (isUnlocked)
            {
                // DESBLOQUEADO
                DeveloperCategoryStatus.Text = es ? "✅ Desbloqueado" : "✅ Unlocked";
                DeveloperCategoryStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E"));
                DeveloperCategoryLock.Text = "✅";

                // Aplicar efecto de brillo al icono
                DeveloperCategoryIcon.Effect = new DropShadowEffect
                {
                    Color = (Color)ColorConverter.ConvertFromString("#22C55E"),
                    BlurRadius = 12,
                    ShadowDepth = 0,
                    Opacity = 0.7
                };

                // Habilitar opciones
                EnableDeveloperOption(Option_ChangeBackground, Option_ChangeBackgroundText, Option_ChangeBackgroundLock, es ? "🎨 Cambiar fondo de background general" : "🎨 Change Background Color");
                EnableDeveloperOption(Option_ChangeAvatar, Option_ChangeAvatarText, Option_ChangeAvatarLock, es ? "👤 Cambiar Avatar" : "👤 Change Avatar");
                EnableDeveloperOption(Option_CreateTranslation, Option_CreateTranslationText, Option_CreateTranslationLock, es ? "🌐 Crear Traducción" : "🌐 Create Translation");
            }
            else
            {
                // BLOQUEADO - Mostrar progreso
                var progress = DeveloperModeManager.GetProgress();

                string statusText = es
                    ? $"🔒 Bloqueado ({progress.ProgressPercentage}%)"
                    : $"🔒 Locked ({progress.ProgressPercentage}%)";

                DeveloperCategoryStatus.Text = statusText;
                DeveloperCategoryStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                DeveloperCategoryLock.Text = "🔒";
                DeveloperCategoryIcon.Effect = null;

                // Deshabilitar opciones
                DisableDeveloperOption(Option_ChangeBackground, Option_ChangeBackgroundText, Option_ChangeBackgroundLock);
                DisableDeveloperOption(Option_ChangeAvatar, Option_ChangeAvatarText, Option_ChangeAvatarLock);
                DisableDeveloperOption(Option_CreateTranslation, Option_CreateTranslationText, Option_CreateTranslationLock);
            }
        }

    private void EnableDeveloperOption(Border border, TextBlock textBlock, TextBlock lockIcon, string text)
        {
            border.Style = (Style)this.Resources["EnabledOption"];
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));
            lockIcon.Visibility = Visibility.Collapsed;
        }

        private void DisableDeveloperOption(Border border, TextBlock textBlock, TextBlock lockIcon)
        {
            border.Style = (Style)this.Resources["DisabledOption"];
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            lockIcon.Visibility = Visibility.Visible;
        }

        private void SetMedalWithColor(TextBlock textBlock, MedalType medal, ref int completedCount, ref int goldCount)
        {
            Color medalColor;
            string emoji;

            switch (medal)
            {
                case MedalType.Gold:
                    completedCount++;
                    goldCount++;
                    emoji = "🥇";
                    medalColor = (Color)ColorConverter.ConvertFromString("#FFD700");
                    break;
                case MedalType.Silver:
                    completedCount++;
                    emoji = "🥈";
                    medalColor = (Color)ColorConverter.ConvertFromString("#C0C0C0");
                    break;
                case MedalType.Bronze:
                    completedCount++;
                    emoji = "🥉";
                    medalColor = (Color)ColorConverter.ConvertFromString("#CD7F32");
                    break;
                default:
                    textBlock.Text = "—";
                    textBlock.Effect = null;
                    return;
            }

            textBlock.Text = emoji;
            textBlock.Effect = new DropShadowEffect
            {
                Color = medalColor,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            };
        }

        private void SetMedalWithColorSimple(TextBlock textBlock, MedalType medal)
        {
            Color medalColor;
            string emoji;

            switch (medal)
            {
                case MedalType.Gold:
                    emoji = "🥇";
                    medalColor = (Color)ColorConverter.ConvertFromString("#FFD700");
                    break;
                case MedalType.Silver:
                    emoji = "🥈";
                    medalColor = (Color)ColorConverter.ConvertFromString("#C0C0C0");
                    break;
                case MedalType.Bronze:
                    emoji = "🥉";
                    medalColor = (Color)ColorConverter.ConvertFromString("#CD7F32");
                    break;
                default:
                    textBlock.Text = "—";
                    textBlock.Effect = null;
                    return;
            }

            textBlock.Text = emoji;
            textBlock.Effect = new DropShadowEffect
            {
                Color = medalColor,
                BlurRadius = 15,
                ShadowDepth = 0,
                Opacity = 0.8
            };
        }

        #region Developer Mode Options

        private void ChangeBackground_Click(object sender, MouseButtonEventArgs e)
        {
            if (Option_ChangeBackground.Style == (Style)this.Resources["DisabledOption"])
                return;

            try
            {
                var currentColors = UserSettingsManager.GetBackgroundColors();
                var colorPicker = new ColorPickerWindow(currentColors);

                if (colorPicker.ShowDialog() == true)
                {
                    UserSettingsManager.SaveBackgroundColors(colorPicker.Color1, colorPicker.Color2, colorPicker.Color3);

                    // Aplicar cambios al MainWindow
                    ApplyBackgroundToMainWindow();

                    MessageBox.Show("Background color updated successfully! 🎨", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowDeveloperWarning();
            }
        }

        private void ChangeAvatar_Click(object sender, MouseButtonEventArgs e)
        {
            if (Option_ChangeAvatar.Style == (Style)this.Resources["DisabledOption"])
                return;

            try
            {
                var avatarSelector = new AvatarSelectorWindow();
                if (avatarSelector.ShowDialog() == true)
                {
                    UserSettingsManager.SaveAvatar(avatarSelector.SelectedAvatar);

                    // Actualizar avatar en la UI
                    AvatarEmoji.Text = avatarSelector.SelectedAvatar;

                    MessageBox.Show("Avatar updated successfully! 👤", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowDeveloperWarning();
            }
        }

        private void ShowDeveloperWarning()
        {
            var warningWindow = new DeveloperWarningWindow();
            warningWindow.ShowDialog();
        }

        private void ApplyBackgroundToMainWindow()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                var colors = UserSettingsManager.GetBackgroundColors();

                try
                {
                    var gradient = new LinearGradientBrush();
                    gradient.StartPoint = new Point(0, 0);
                    gradient.EndPoint = new Point(1, 1);
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[0]), 0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[1]), 0.45));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[2]), 1));

                    var mainBorder = mainWindow.FindName("MainBackgroundBorder") as Border;
                    if (mainBorder != null)
                    {
                        mainBorder.Background = gradient;
                    }
                }
                catch { }
            }
        }
        private void CreateTranslation_Click(object sender, MouseButtonEventArgs e)
        {
            // Verificar si está desbloqueado
            if (Option_CreateTranslation.Style == (Style)this.Resources["DisabledOption"])
                return;

            // Abrir URL del tutorial de traducción
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/tu-repositorio/translation-guide",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("No se pudo abrir el enlace del tutorial.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}