using ModernDesign.Localization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class IntermediateCategoryWindow : Window
    {
        private string _currentCategory = "xml";

        public IntermediateCategoryWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            SelectCategory("xml");
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            TitleText.Text = es
                ? "🛠️ Categorías de Modding Intermedio"
                : "🛠️ Intermediate Modding Categories";

            SubtitleText.Text = es
                ? "Da el salto desde lo básico con lecciones enfocadas en ajustes y creación intermedia."
                : "Go beyond beginner level with focused intermediate modding lessons.";

            CloseButton.Content = es ? "Cerrar" : "Close";

            // Tarjetas
            CardXmlTitle.Text = es ? "XML Tuning avanzado" : "Advanced XML Tuning";
            CardXmlDesc.Text = es
                ? "Override, inyecciones y ajustes finos sin romper el juego."
                : "Overrides, injections and fine-tuning without breaking the game.";

            CardGameplayTitle.Text = es ? "Gameplay intermedio" : "Intermediate Gameplay";
            CardGameplayDesc.Text = es
                ? "Autonomía, necesidades y progresión equilibrada."
                : "Autonomy, needs and balanced progression.";

            CardCasObjectsTitle.Text = es ? "CAS y Objetos" : "CAS & Objects";
            CardCasObjectsDesc.Text = es
                ? "Recolors intermedios y ajustes de catálogo."
                : "Intermediate recolors and catalog tuning.";

            CardPerformanceTitle.Text = es ? "Rendimiento y QA" : "Performance & QA";
            CardPerformanceDesc.Text = es
                ? "Organización, merges y pruebas controladas."
                : "Organization, merges and controlled testing.";

            UpdateCategoryUI();
        }

        private void CategoryCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is string key)
            {
                SelectCategory(key);
            }
        }

        private void SelectCategory(string key)
        {
            _currentCategory = key;
            UpdateCategoryUI();
        }

        private void UpdateCategoryUI()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            LessonsPanel.Children.Clear();

            switch (_currentCategory)
            {
                case "xml":
                    CategoryTitleText.Text = es ? "XML Tuning avanzado" : "Advanced XML Tuning";
                    CategoryIntroText.Text = es
                        ? "Aquí se trabaja directamente con archivos XML del juego para ajustar comportamientos, tiempos, costes y más."
                        : "Here you work directly with game XML tuning files to adjust behaviours, timings, costs and more.";

                    AddLesson(
                        es ? "Tipos de tuning" : "Types of tuning",
                        es
                            ? "Aprende a identificar tunings de interacciones, objetos, rasgos, buffs y situaciones antes de editarlos."
                            : "Learn to identify tuning for interactions, objects, traits, buffs and situations before editing them.",
                        "#22C55E",
                        es ? "Fundamentos" : "Fundamentals");

                    AddLesson(
                        es ? "Overrides seguros" : "Safe overrides",
                        es
                            ? "Crea copias limpias en tu propio paquete y evita editar archivos originales del juego o de otros creadores."
                            : "Create clean copies in your own package and avoid editing original game or creators' files.",
                        "#38BDF8",
                        es ? "Buenas prácticas" : "Best practice");

                    AddLesson(
                        es ? "Conflictos y prioridades" : "Conflicts & priorities",
                        es
                            ? "Entiende cómo el juego decide qué tuning usar cuando dos mods editan lo mismo, y cómo minimizar conflictos."
                            : "Understand how the game decides which tuning to use when two mods change the same resource, and how to minimize conflicts.",
                        "#F97316",
                        es ? "Estrategia" : "Strategy");
                    break;

                case "gameplay":
                    CategoryTitleText.Text = es ? "Gameplay intermedio" : "Intermediate Gameplay";
                    CategoryIntroText.Text = es
                        ? "Enfocado en modificar cómo viven y se comportan tus Sims sin llegar aún a scripting complejo."
                        : "Focused on changing how Sims live and behave, without going into complex scripting yet.";

                    AddLesson(
                        es ? "Curvas de necesidad" : "Need curves",
                        es
                            ? "Modifica la velocidad a la que bajan las necesidades y cómo impacta en la jugabilidad diaria."
                            : "Adjust how fast needs decay and how that impacts day-to-day gameplay.",
                        "#22C55E",
                        es ? "Balanceo" : "Balancing");

                    AddLesson(
                        es ? "Autonomía" : "Autonomy",
                        es
                            ? "Controla qué interacciones se usan en autonomía, con qué probabilidad y en qué contexto."
                            : "Control which interactions are used autonomously, with what chance and in which context.",
                        "#A78BFA",
                        es ? "Comportamiento" : "Behaviour");

                    AddLesson(
                        es ? "Progresión y recompensa" : "Progression & rewards",
                        es
                            ? "Crea experiencias más largas o más rápidas ajustando requisitos, recompensas y cooldowns."
                            : "Create longer or faster experiences by tuning requirements, rewards and cooldowns.",
                        "#F97316",
                        es ? "Diseño" : "Design");
                    break;

                case "cas_objects":
                    CategoryTitleText.Text = es ? "CAS y Objetos intermedios" : "Intermediate CAS & Objects";
                    CategoryIntroText.Text = es
                        ? "Aquí trabajas con recolors limpios, tags, categorías y variaciones para ropa y objetos."
                        : "Here you work with clean recolors, tags, categories and variations for clothing and objects.";

                    AddLesson(
                        es ? "Recolors organizados" : "Organised recolors",
                        es
                            ? "Crea recolors que no saturen el catálogo y respeten categorías, edad y género."
                            : "Create recolors that don't flood the catalog and respect category, age and gender.",
                        "#22C55E",
                        es ? "Catálogo limpio" : "Clean catalog");

                    AddLesson(
                        es ? "Tags y filtros" : "Tags & filters",
                        es
                            ? "Ajusta tags para que tu CC aparezca en los filtros correctos dentro de CAS y Modo Construir."
                            : "Edit tags so your CC appears in the correct filters in CAS and Build Mode.",
                        "#38BDF8",
                        es ? "Accesibilidad" : "Accessibility");

                    AddLesson(
                        es ? "Variantes y swatches" : "Variants & swatches",
                        es
                            ? "Aprende a ordenar paletas de color, miniaturas y variaciones sin duplicar contenido."
                            : "Learn to order colour palettes, thumbnails and variations without duplicating content.",
                        "#A78BFA",
                        es ? "Presentación" : "Presentation");
                    break;

                case "performance":
                    CategoryTitleText.Text = es ? "Rendimiento y QA" : "Performance & QA";
                    CategoryIntroText.Text = es
                        ? "Tu mod puede ser bueno, pero si no está optimizado o probado, romperá partidas. Aquí se pule todo."
                        : "Your mod can be good, but if it's not optimized or tested, it will break saves. This is where you polish it.";

                    AddLesson(
                        es ? "Estructura de carpetas" : "Folder structure",
                        es
                            ? "Define una estructura clara por tipo de mod, creador y versión para encontrar errores rápido."
                            : "Define a clear structure by mod type, creator and version to find issues quickly.",
                        "#22C55E",
                        es ? "Organización" : "Organization");

                    AddLesson(
                        es ? "Merges responsables" : "Responsible merges",
                        es
                            ? "Cuándo conviene mergear, cuándo NO, y cómo documentar qué hay dentro de cada paquete."
                            : "When merging is useful, when it's NOT, and how to document what's inside each package.",
                        "#F97316",
                        es ? "Buenas prácticas" : "Best practice");

                    AddLesson(
                        es ? "Ciclos de prueba" : "Testing cycles",
                        es
                            ? "Crea partidas de prueba específicas para probar cada mod sin arriesgar tu save principal."
                            : "Create dedicated test saves for each mod without risking your main save.",
                        "#EF4444",
                        es ? "Testing" : "Testing");
                    break;
            }
        }

        private void AddLesson(string title, string description, string colorHex, string badgeText)
        {
            var border = new Border
            {
                Style = (Style)Resources["LessonCard"]
            };

            var stack = new StackPanel();

            if (!string.IsNullOrEmpty(badgeText))
            {
                var badgeBorder = new Border
                {
                    Style = (Style)Resources["BadgeStyle"],
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
                };
                badgeBorder.Child = new TextBlock
                {
                    Text = badgeText,
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                stack.Children.Add(badgeBorder);
            }

            stack.Children.Add(new TextBlock
            {
                Text = title,
                Style = (Style)Resources["TitleStyle"],
                FontSize = 13
            });

            stack.Children.Add(new TextBlock
            {
                Text = description,
                Style = (Style)Resources["BodyStyle"],
                FontSize = 11,
                Margin = new Thickness(0, 3, 0, 0)
            });

            border.Child = stack;
            LessonsPanel.Children.Add(border);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
