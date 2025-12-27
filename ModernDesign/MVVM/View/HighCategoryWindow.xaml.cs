using ModernDesign.Localization;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class HighCategoryWindow : Window
    {
        private string _currentCategory = "frameworks";

        public HighCategoryWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            SelectCategory("frameworks");
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            TitleText.Text = es
                ? "🧠 Categorías de Modding Avanzado"
                : "🧠 High-Level Modding Categories";

            SubtitleText.Text = es
                ? "Sistemas complejos, frameworks reutilizables y flujos de trabajo profesionales."
                : "Complex systems, reusable frameworks and more professional workflows.";

            CloseButton.Content = es ? "Cerrar" : "Close";

            CardFrameworksTitle.Text = es ? "Frameworks de gameplay" : "Gameplay Frameworks";
            CardFrameworksDesc.Text = es
                ? "Sistemas base que otros mods pueden usar como dependencia."
                : "Base systems that other mods can use as a dependency.";

            CardEventsTitle.Text = es ? "Eventos y situaciones" : "Events & Situations";
            CardEventsDesc.Text = es
                ? "Estructuras temporales avanzadas que reaccionan al jugador."
                : "Advanced temporal structures that react to the player.";

            CardPythonTitle.Text = es ? "Integración con Python" : "Python Integration";
            CardPythonDesc.Text = es
                ? "Comunica tuning, scripts y datos persistentes."
                : "Connect tuning, scripts and persistent data.";

            CardProjectsTitle.Text = es ? "Proyectos grandes" : "Large-Scale Projects";
            CardProjectsDesc.Text = es
                ? "Planifica, versiona y publica mods grandes con orden."
                : "Plan, version and ship large mods in an organized way.";

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
                case "frameworks":
                    CategoryTitleText.Text = es ? "Frameworks de gameplay" : "Gameplay Frameworks";
                    CategoryIntroText.Text = es
                        ? "Diseña sistemas que no son un único mod, sino una base para muchos otros (tuyos o de la comunidad)."
                        : "Design systems that are not just a single mod, but a base that many others (yours or the community's) can use.";

                    AddLesson(
                        es ? "Arquitectura modular" : "Modular architecture",
                        es
                            ? "Separa tu lógica en módulos: utilidades, datos, eventos y UI. Así puedes reutilizar piezas sin duplicar código."
                            : "Split your logic into modules: utilities, data, events and UI. That way you can reuse pieces without duplicating code.",
                        "#22C55E",
                        es ? "Diseño" : "Design");

                    AddLesson(
                        es ? "APIs de creador" : "Creator APIs",
                        es
                            ? "Define funciones y tunings pensados para que otros creadores los usen como \"API\" estable."
                            : "Expose functions and tunings that other creators can treat as a stable \"API\".",
                        "#38BDF8",
                        es ? "Colaboración" : "Collaboration");

                    AddLesson(
                        es ? "Compatibilidad futura" : "Future compatibility",
                        es
                            ? "Piensa desde ya en cómo actualizar sin romper mods que dependan de tu framework."
                            : "Think from the start about how to update without breaking mods that depend on your framework.",
                        "#F97316",
                        es ? "Largo plazo" : "Long term");
                    break;

                case "events":
                    CategoryTitleText.Text = es ? "Eventos y situaciones avanzadas" : "Advanced events & situations";
                    CategoryIntroText.Text = es
                        ? "Aquí conectas XML y scripts para crear experiencias temporales complejas (festivales, historias, mini sistemas)."
                        : "Here you connect XML and scripts to build complex temporal experiences (festivals, stories, mini systems).";

                    AddLesson(
                        es ? "Diseño del ciclo de vida" : "Lifecycle design",
                        es
                            ? "Define etapas claras: inicio, desarrollo, resolución y limpieza de datos."
                            : "Define clear phases: start, progression, resolution and cleanup.",
                        "#22C55E",
                        es ? "Blueprint" : "Blueprint");

                    AddLesson(
                        es ? "Triggers y condiciones" : "Triggers & conditions",
                        es
                            ? "Usa tests, loot y scripts para decidir cuándo se activa cada parte de tu evento."
                            : "Use tests, loot and scripts to decide when each part of your event fires.",
                        "#A78BFA",
                        es ? "Lógica" : "Logic");

                    AddLesson(
                        es ? "Persistencia entre partidas" : "Cross-save persistence",
                        es
                            ? "Guarda estados críticos en datos persistentes para que la historia continúe aunque cierres el juego."
                            : "Store critical states in persistent data so the story continues even across sessions.",
                        "#F97316",
                        es ? "Experiencia" : "Experience");
                    break;

                case "python":
                    CategoryTitleText.Text = es ? "Integración avanzada con Python" : "Advanced Python integration";
                    CategoryIntroText.Text = es
                        ? "Cuando tuning ya no basta, Python te permite crear lógica propia, estados complejos y sistemas dinámicos."
                        : "When tuning is not enough, Python lets you create your own logic, complex states and dynamic systems.";

                    AddLesson(
                        es ? "Estructura de un mod script" : "Script mod structure",
                        es
                            ? "Organiza paquetes, módulos y nombres para evitar conflictos con otros mods."
                            : "Organize packages, modules and names to avoid conflicts with other mods.",
                        "#22C55E",
                        es ? "Base sólida" : "Solid base");

                    AddLesson(
                        es ? "Comunicación tuning ⇄ script" : "Tuning ⇄ script communication",
                        es
                            ? "Aprende a pasar datos desde XML a Python y devolver resultados sin romper la carga."
                            : "Learn to pass data from XML into Python and back without breaking load order.",
                        "#38BDF8",
                        es ? "Integración" : "Integration");

                    AddLesson(
                        es ? "Debug avanzado" : "Advanced debugging",
                        es
                            ? "Usa logs, asserts y comandos de consola para entender qué hace tu código en tiempo real."
                            : "Use logs, asserts and console commands to understand what your code does at runtime.",
                        "#EF4444",
                        es ? "Debug" : "Debug");
                    break;

                case "projects":
                    CategoryTitleText.Text = es ? "Proyectos de gran escala" : "Large-scale projects";
                    CategoryIntroText.Text = es
                        ? "Piensa en tu mod como un proyecto real: tareas, versiones, changelogs y feedback de usuarios."
                        : "Treat your mod like a real project: tasks, versions, changelogs and user feedback.";

                    AddLesson(
                        es ? "Planificación por hitos" : "Milestone planning",
                        es
                            ? "Divide tu idea gigante en versiones pequeñas que puedan salir sin esperar meses."
                            : "Split your giant idea into small versions that can ship without waiting months.",
                        "#22C55E",
                        es ? "Gestión" : "Management");

                    AddLesson(
                        es ? "Control de versiones" : "Version control",
                        es
                            ? "Aprende a etiquetar versiones, mantener backups y anotar qué cambia en cada actualización."
                            : "Tag versions, keep backups and note what changes in each update.",
                        "#38BDF8",
                        es ? "Historia" : "History");

                    AddLesson(
                        es ? "Soporte y comunidad" : "Support & community",
                        es
                            ? "Define cómo recibir feedback, reportes de bugs y sugerencias sin ahogarte."
                            : "Define how to receive feedback, bug reports and suggestions without drowning.",
                        "#F97316",
                        es ? "Comunidad" : "Community");
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
