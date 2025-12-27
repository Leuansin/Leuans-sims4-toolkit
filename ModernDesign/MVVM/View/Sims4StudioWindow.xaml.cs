using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class Sims4StudioWindow : Window
    {
        private int _currentTab = 0;

        public Sims4StudioWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            LoadVideosAndDocs();
        }

        #region Language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Sims 4 Modding Hub" : "Sims 4 Modding Hub";
            TitleText.Text = es ? "🛠️ Sims 4 Modding Hub" : "🛠️ Sims 4 Modding Hub";
            SubtitleText.Text = es ? "Aprende a crear mods, mergear packages y más" : "Learn to create mods, merge packages and more";

            TabCreateMod.Content = es ? "🎨 Crea tu Primer Mod" : "🎨 Creating Your First Mod";
            TabMerge.Content = es ? "📦 Cómo Mergear" : "📦 How to Merge";
            TabGuides.Content = es ? "📚 Guías y Tutoriales" : "📚 Guides & Tutorials";

            CreateModTitle.Text = es ? "🎨 Crea tu Primer Mod" : "🎨 Creating Your First Mod";
            CreateModDesc.Text = es ? "Sigue estas 3 lecciones para comenzar tu viaje en el modding" : "Follow these 3 lessons to start your modding journey";

            L1Title.Text = es ? "Descargar S4S" : "Download S4S";
            L1Desc.Text = es ? "Obtén Sims 4 Studio y sus dependencias" : "Get Sims 4 Studio and its dependencies";
            L2Title.Text = es ? "Planifica tu Mod" : "Plan Your Mod";
            L2Desc.Text = es ? "Ten claro qué quieres crear y la mentalidad correcta" : "Know what you want to create and have the right mindset";
            L3Title.Text = es ? "Pide Ayuda" : "Ask for Help";
            L3Desc.Text = es ? "IA, foros, repositorios - ¡no es trampa!" : "AI, forums, repositories - it's not cheating!";

            OpenCategoriesButton.Content = es ? "🚀 Empezar a Crear - Elige una Categoría" : "🚀 Start Creating - Choose a Category";

            MergeTitle.Text = es ? "📦 Cómo Mergear Packages" : "📦 How to Merge Packages";
            MergeDesc.Text = es ? "Combina múltiples archivos .package en uno para cargar más rápido" : "Combine multiple .package files into one for faster loading";
            MergeStep1.Text = es ? "Abre Sims 4 Studio y ve a Tools → Batch Fixes" : "Open Sims 4 Studio and go to Tools → Batch Fixes";
            MergeStep2.Text = es ? "Selecciona 'Merge Packages' y elige tus archivos .package" : "Select 'Merge Packages' and choose your .package files";
            MergeStep3.Text = es ? "Guarda el archivo combinado y elimina/respalda los originales" : "Save the merged file and delete/backup the originals";
            MergeWarning.Text = es ? "⚠️ Advertencia: ¡No mergees archivos .ts4script! Solo los .package se pueden mergear. Además, mergear dificulta identificar mods individuales después." : "⚠️ Warning: Don't merge .ts4script files! Only .package files can be merged. Also, merging makes it harder to identify individual mods later.";

            GuidesTitle.Text = es ? "📚 Guías y Tutoriales" : "📚 Guides & Tutorials";
            GuidesDesc.Text = es ? "Video tutoriales y documentación para dominar Sims 4 Studio" : "Video tutorials and documentation to master Sims 4 Studio";
            VideoSection.Text = es ? "🎬 Video Tutoriales" : "🎬 Video Tutorials";
            DocsSection.Text = es ? "📄 Documentación" : "📄 Documentation";

            DownloadS4SButton.Content = es ? "⬇️ Descargar Sims 4 Studio" : "⬇️ Download Sims 4 Studio";
            CloseButton.Content = es ? "Cerrar" : "Close";
        }

        private void LoadVideosAndDocs()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            VideosList.Items.Clear();
            DocsList.Items.Clear();

            var videos = new[] {
                ("https://www.youtube.com/results?search_query=sims+4+studio+beginner+tutorial", es ? "Tutorial para principiantes de S4S" : "S4S Beginner Tutorial"),
                ("https://www.youtube.com/results?search_query=sims+4+studio+create+cc", es ? "Cómo crear CC con S4S" : "How to Create CC with S4S"),
                ("https://www.youtube.com/results?search_query=sims+4+studio+recolor+tutorial", es ? "Tutorial de Recolors" : "Recolor Tutorial"),
                ("https://www.youtube.com/results?search_query=sims+4+studio+mesh+edit", es ? "Edición de Meshes" : "Mesh Editing")
            };

            var docs = new[] {
                ("https://sims4studio.com/", es ? "Sitio oficial de Sims 4 Studio" : "Sims 4 Studio Official Site"),
                ("https://sims4studio.com/thread/15145/started-python-scripting", es ? "Guía de Scripting en Python" : "Python Scripting Guide"),
                ("https://modthesims.info/wiki.php?title=Sims_4_Modding", es ? "Wiki de Modding de The Sims 4" : "The Sims 4 Modding Wiki"),
                ("https://lot51.cc/tdesc", es ? "Documentación de Tuning (Lot51)" : "Tuning Documentation (Lot51)")
            };

            foreach (var (url, title) in videos) AddLinkItem(VideosList, url, "🎬 " + title);
            foreach (var (url, title) in docs) AddLinkItem(DocsList, url, "📄 " + title);
        }

        private void AddLinkItem(ItemsControl list, string url, string title)
        {
            var border = new Border { Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")), CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 6), Cursor = Cursors.Hand };
            var tb = new TextBlock { FontSize = 12, FontFamily = new FontFamily("Bahnschrift Light") };
            var hl = new Hyperlink { NavigateUri = new Uri(url), Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8")) };
            hl.Inlines.Add(title);
            hl.RequestNavigate += (s, e) => { OpenUrl(e.Uri.AbsoluteUri); e.Handled = true; };
            tb.Inlines.Add(hl);
            border.Child = tb;
            border.MouseLeftButtonUp += (s, e) => OpenUrl(url);
            list.Items.Add(border);
        }
        #endregion

        #region Tabs
        private void TabCreateMod_Click(object sender, RoutedEventArgs e) => SwitchTab(0);
        private void TabMerge_Click(object sender, RoutedEventArgs e) => SwitchTab(1);
        private void TabGuides_Click(object sender, RoutedEventArgs e) => SwitchTab(2);

        private void SwitchTab(int tab)
        {
            _currentTab = tab;
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED"));
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            var activeFg = Brushes.White;
            var inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

            TabCreateMod.Background = tab == 0 ? activeColor : inactiveColor;
            TabCreateMod.Foreground = tab == 0 ? activeFg : inactiveFg;
            TabMerge.Background = tab == 1 ? activeColor : inactiveColor;
            TabMerge.Foreground = tab == 1 ? activeFg : inactiveFg;
            TabGuides.Background = tab == 2 ? activeColor : inactiveColor;
            TabGuides.Foreground = tab == 2 ? activeFg : inactiveFg;

            TabCreateModContent.Visibility = tab == 0 ? Visibility.Visible : Visibility.Collapsed;
            TabMergeContent.Visibility = tab == 1 ? Visibility.Visible : Visibility.Collapsed;
            TabGuidesContent.Visibility = tab == 2 ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region Lessons
        private void Lesson1Card_Click(object sender, MouseButtonEventArgs e) => ShowLessonMessage(1);
        private void Lesson2Card_Click(object sender, MouseButtonEventArgs e) => ShowLessonMessage(2);
        private void Lesson3Card_Click(object sender, MouseButtonEventArgs e) => ShowLessonMessage(3);

        private void ShowLessonMessage(int lesson)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string title = "", msg = "";
            switch (lesson)
            {
                case 1:
                    title = es ? "Lección 1: Descargar S4S" : "Lesson 1: Download S4S";
                    msg = es ? "1. Ve a sims4studio.com\n2. Descarga la última versión\n3. Instala .NET Framework si te lo pide\n4. Ejecuta Sims 4 Studio y configura tu carpeta del juego" : "1. Go to sims4studio.com\n2. Download the latest version\n3. Install .NET Framework if prompted\n4. Run Sims 4 Studio and set up your game folder";
                    break;
                case 2:
                    title = es ? "Lección 2: Planifica tu Mod" : "Lesson 2: Plan Your Mod";
                    msg = es ? "Antes de crear:\n• Define qué tipo de mod quieres (CC, script, tuning)\n• Investiga si ya existe algo similar\n• Empieza simple - un recolor es perfecto para empezar\n• Ten paciencia - el modding tiene curva de aprendizaje" : "Before creating:\n• Define what type of mod you want (CC, script, tuning)\n• Research if something similar exists\n• Start simple - a recolor is perfect for beginners\n• Be patient - modding has a learning curve";
                    break;
                case 3:
                    title = es ? "Lección 3: Pide Ayuda" : "Lesson 3: Ask for Help";
                    msg = es ? "No estás solo:\n• Usa IA (ChatGPT, Claude) para dudas de código\n• Pregunta en foros como ModTheSims\n• Revisa repositorios de GitHub\n• Únete a Discords de modding\n\n¡Pedir ayuda NO es trampa, es parte del proceso!" : "You're not alone:\n• Use AI (ChatGPT, Claude) for code questions\n• Ask on forums like ModTheSims\n• Check GitHub repositories\n• Join modding Discords\n\nAsking for help is NOT cheating, it's part of the process!";
                    break;
            }
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenCategories_Click(object sender, RoutedEventArgs e)
        {
            var win = new S4SCategoriesWindow { Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            win.ShowDialog();
        }
        #endregion

        #region Actions
        private void DownloadS4S_Click(object sender, RoutedEventArgs e) => OpenUrl("https://sims4studio.com/");
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        #endregion
    }
}