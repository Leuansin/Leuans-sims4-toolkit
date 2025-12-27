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
    public partial class ThreeDModelingWindow : Window
    {
        private int _currentTab = 0;

        public ThreeDModelingWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            LoadVideosAndDocs();
        }

        #region Language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Hub de Modelado 3D" : "3D Modeling Hub";
            TitleText.Text = es ? "🎭 Hub de Modelado 3D" : "🎭 3D Modeling Hub";
            SubtitleText.Text = es ? "Aprende a crear meshes 3D personalizados con Blender" : "Learn to create custom 3D meshes with Blender";

            TabGettingStarted.Content = es ? "🚀 Empezando" : "🚀 Getting Started";
            TabTools.Content = es ? "🛠️ Herramientas y Configuración" : "🛠️ Tools & Setup";
            TabGuides.Content = es ? "📚 Guías y Tutoriales" : "📚 Guides & Tutorials";

            GettingStartedTitle.Text = es ? "🚀 Empezando con Modelado 3D" : "🚀 Getting Started with 3D Modeling";
            GettingStartedDesc.Text = es ? "Sigue estos pasos para empezar a crear contenido 3D para The Sims 4" : "Follow these steps to start creating 3D content for The Sims 4";

            Step1Title.Text = es ? "Descargar Blender" : "Download Blender";
            Step1Desc.Text = es ? "Obtén Blender 3.x o superior (gratis y de código abierto)" : "Get Blender 3.x or higher (free and open source)";
            Step2Title.Text = es ? "Instalar Plugin S4S de Blender" : "Install S4S Blender Plugin";
            Step2Desc.Text = es ? "CAS Tools... Addon esencial para importar/exportar meshes de Sims 4" : "Essential addon for importing/exporting Sims 4 meshes";
            Step3Title.Text = es ? "Aprender lo Básico de Blender" : "Learn Blender Basics";
            Step3Desc.Text = es ? "Domina técnicas básicas de modelado 3D" : "Master basic 3D modeling techniques";

            OpenBlenderTutorialsButton.Content = es ? "📚 Ver Lista Completa de Tutoriales" : "📚 View Full Tutorial List";

            ToolsTitle.Text = es ? "🛠️ Herramientas Esenciales y Configuración" : "🛠️ Essential Tools & Setup";
            ToolsDesc.Text = es ? "Todo lo que necesitas para empezar a modelar en 3D para The Sims 4" : "Everything you need to start 3D modeling for The Sims 4";

            BlenderInfo.Text = es ? "Suite gratuita de creación 3D. Descarga la versión 3.0 o superior desde blender.org" : "Free 3D creation suite. Download version 3.0 or higher from blender.org";
            S4SPluginInfo.Text = es ? "Plugin para importar/exportar archivos .blend a formato de Sims 4. Disponible en sims4studio.com" : "Plugin to import/export .blend files to Sims 4 format. Available on sims4studio.com";
            S4SMainInfo.Text = es ? "Para empaquetar tus modelos 3D en archivos .package y añadirlos al juego" : "To pack your 3D models into .package files and add them to the game";
            SetupWarning.Text = es ? "⚠️ Importante: ¡Asegúrate de que las versiones de Blender y el plugin S4S sean compatibles!" : "⚠️ Important: Make sure Blender and the S4S plugin versions are compatible!";

            GuidesTitle.Text = es ? "📚 Guías y Tutoriales" : "📚 Guides & Tutorials";
            GuidesDesc.Text = es ? "Video tutoriales y documentación para dominar el modelado 3D" : "Video tutorials and documentation to master 3D modeling";
            VideoSection.Text = es ? "🎬 Video Tutoriales" : "🎬 Video Tutorials";
            DocsSection.Text = es ? "📄 Documentación" : "📄 Documentation";

            DownloadBlenderButton.Content = es ? "⬇️ Descargar Blender" : "⬇️ Download Blender";
            CloseButton.Content = es ? "Cerrar" : "Close";
        }

        private void LoadVideosAndDocs()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            VideosList.Items.Clear();
            DocsList.Items.Clear();

            var videos = new[] {
                ("https://www.youtube.com/results?search_query=blender+beginners+tutorial", es ? "Tutorial de Blender para principiantes" : "Blender Beginner Tutorial"),
                ("https://www.youtube.com/watch?v=UwItqO_F_I8", es ? "Cómo crear meshes para Sims 4" : "How to Create Meshes for Sims 4"),
                ("https://www.youtube.com/watch?v=AIQ1mjQKDxk", es ? "Crear ropa con Blender para Sims 4" : "Create Clothing with Blender for Sims 4"),
                ("https://www.youtube.com/watch?v=3xgblkKUNT4", es ? "Crear cabello con Blender" : "Create Hair with Blender"),
                ("https://www.youtube.com/results?search_query=sims+4+blender+furniture", es ? "Crear muebles con Blender" : "Create Furniture with Blender")
            };

            var docs = new[] {
                ("https://www.blender.org/", es ? "Sitio oficial de Blender" : "Blender Official Site"),
                ("https://sims4studio.com/thread/9/blender-tutorial-links", es ? "Sims 4 Studio - Guía de Blender" : "Sims 4 Studio - Blender Guide"),
                ("https://docs.blender.org/manual/en/latest/", es ? "Manual oficial de Blender" : "Blender Official Manual"),
                ("https://www.youtube.com/watch?v=wuAPGjBgJKQ", es ? "Guía de Meshing" : "Meshing Guide")
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
        private void TabGettingStarted_Click(object sender, RoutedEventArgs e) => SwitchTab(0);
        private void TabTools_Click(object sender, RoutedEventArgs e) => SwitchTab(1);
        private void TabGuides_Click(object sender, RoutedEventArgs e) => SwitchTab(2);

        private void SwitchTab(int tab)
        {
            _currentTab = tab;
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7C3AED"));
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            var activeFg = Brushes.White;
            var inactiveFg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));

            TabGettingStarted.Background = tab == 0 ? activeColor : inactiveColor;
            TabGettingStarted.Foreground = tab == 0 ? activeFg : inactiveFg;
            TabTools.Background = tab == 1 ? activeColor : inactiveColor;
            TabTools.Foreground = tab == 1 ? activeFg : inactiveFg;
            TabGuides.Background = tab == 2 ? activeColor : inactiveColor;
            TabGuides.Foreground = tab == 2 ? activeFg : inactiveFg;

            TabGettingStartedContent.Visibility = tab == 0 ? Visibility.Visible : Visibility.Collapsed;
            TabToolsContent.Visibility = tab == 1 ? Visibility.Visible : Visibility.Collapsed;
            TabGuidesContent.Visibility = tab == 2 ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion

        #region Steps
        private void Step1Card_Click(object sender, MouseButtonEventArgs e) => ShowStepMessage(1);
        private void Step2Card_Click(object sender, MouseButtonEventArgs e) => ShowStepMessage(2);
        private void Step3Card_Click(object sender, MouseButtonEventArgs e) => ShowStepMessage(3);

        private void ShowStepMessage(int step)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string title = "", msg = "";
            switch (step)
            {
                case 1:
                    title = es ? "Paso 1: Descargar Blender" : "Step 1: Download Blender";
                    msg = es ? "1. Ve a www.blender.org\n2. Descarga la última versión estable (3.x o superior)\n3. Instala siguiendo las instrucciones\n4. Abre Blender y familiarízate con la interfaz" : "1. Go to www.blender.org\n2. Download the latest stable version (3.x or higher)\n3. Install following instructions\n4. Open Blender and familiarize yourself with the interface";
                    break;
                case 2:
                    title = es ? "Paso 2: Instalar Plugin S4S" : "Step 2: Install S4S Plugin";
                    msg = es
                        ? "1. Abre Sims 4 Studio\n2. Ve a Configuración (Settings)\n3. Selecciona la ruta de Blender (Blender Path)\n4. Navega hasta _:/__/Sims4Studio/Blender/Scripts/\n5. Elige io_sims.py y selecciona 'Instalar'\n6. ¡Listo!"
                        : "1. Open Sims 4 Studio\n2. Open Settings\n3. Choose Blender Path\n4. Navigate to _:/__/Sims4Studio/Blender/Scripts/\n5. Choose io_sims.py, click on Install from file\n6. Done";
                    break;
                case 3:
                    title = es ? "Paso 3: Aprender lo Básico" : "Step 3: Learn Basics";
                    msg = es ? "Conceptos clave a dominar:\n• Navegación en viewport (girar, hacer zoom, paneo)\n• Modos: Object, Edit, Sculpt\n• Herramientas básicas: mover, rotar, escalar\n• Modelado básico: extruir, subdividir\n• UV mapping\n• Exportar/Importar meshes" : "Key concepts to master:\n• Viewport navigation (rotate, zoom, pan)\n• Modes: Object, Edit, Sculpt\n• Basic tools: move, rotate, scale\n• Basic modeling: extrude, subdivide\n• UV mapping\n• Export/Import meshes";
                    break;
            }
            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenBlenderTutorials_Click(object sender, RoutedEventArgs e) => SwitchTab(2);
        #endregion

        #region Actions
        private void DownloadBlender_Click(object sender, RoutedEventArgs e) => OpenUrl("https://www.blender.org/download/");
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void OpenUrl(string url)
        {
            try { Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true }); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        #endregion
    }
}