using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class GameplayEnhancerView : Window
    {
        public GameplayEnhancerView()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        #region Language
        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            Title = es ? "Optimizador de Juego" : "Gameplay Enhancer";
            TitleText.Text = es ? "⚡ Optimizador de Juego" : "⚡ Gameplay Enhancer";
            SubtitleText.Text = es ? "Optimiza la configuración de tu juego para la mejor experiencia" : "Optimize your game settings for the best experience";

            PresetsTitle.Text = es ? "🎮 Presets de Gráficos" : "🎮 Graphics Presets";

            PrettyGraphicsTitle.Text = es ? "Gráficos Hermosos" : "Pretty Graphics";
            PrettyGraphicsDesc.Text = es ? "Máxima calidad visual" : "Maximum visual quality";

            BalancedTitle.Text = es ? "Hermoso y Rendimiento" : "Pretty & Performance";
            BalancedDesc.Text = es ? "Lo mejor de ambos mundos" : "Best of both worlds";

            PerformanceTitle.Text = es ? "Rendimiento" : "Performance";
            PerformanceDesc.Text = es ? "Máximo FPS" : "Maximum FPS";

            AdvancedTitle.Text = es ? "🔧 Configuración Avanzada" : "🔧 Advanced Settings";
            AdvancedDesc.Text = es ? "Ajusta cada aspecto de tu juego" : "Fine-tune every aspect of your game";

            GameTweakerTitle.Text = "Game Tweaker";
            GameTweakerSubtitle.Text = es ? "Edita Options.ini con controles avanzados" : "Edit Options.ini with advanced controls";

            CloseButton.Content = es ? "Cerrar" : "Close";
        }
        #endregion

        #region Preset Cards
        private void PrettyGraphicsCard_Click(object sender, MouseButtonEventArgs e)
        {
            ApplyPrettyGraphicsPreset();
        }

        private void BalancedCard_Click(object sender, MouseButtonEventArgs e)
        {
            ApplyBalancedPreset();
        }

        private void PerformanceCard_Click(object sender, MouseButtonEventArgs e)
        {
            ApplyPerformancePreset();
        }

        private void ApplyPrettyGraphicsPreset()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string title = es ? "Gráficos Hermosos" : "Pretty Graphics";
            string message = es
                ? "Este preset aplicará:\n\n• Todas las configuraciones en ULTRA\n• Texturas sin comprimir\n• Reflejos al máximo\n• Distancia de visión alta\n• Post-procesamiento activado\n\n¿Deseas continuar?"
                : "This preset will apply:\n\n• All settings on ULTRA\n• Uncompressed textures\n• Maximum reflections\n• High view distance\n• Post-processing enabled\n\n¿Do you want to continue?";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ApplyPreset("PrettyGraphics");
            }
        }

        private void ApplyBalancedPreset()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string title = es ? "Hermoso y Rendimiento" : "Pretty & Performance";
            string message = es
                ? "Este preset aplicará:\n\n• Sims y Objetos: Medio\n• Iluminación: Media\n• Reflejos: Alto\n• Partículas: Bajo\n• Suavizado de bordes: Medio\n• Texturas 3D: No\n• Distancia de visión: Alta\n• Texturas sin comprimir: Sí\n• Post-procesamiento: No\n• Modo portátil: No\n\n¿Deseas continuar?"
                : "This preset will apply:\n\n• Sims & Objects: Medium\n• Lightning: Medium\n• Reflections: High\n• Particles: Low\n• Edge Smoothing: Medium\n• 3D Textures: No\n• View Distance: High\n• Uncompressed Textures: Yes\n• Post-processing: No\n• Laptop Mode: No\n\nDo you want to continue?";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ApplyPreset("Balanced");
            }
        }

        private void ApplyPerformancePreset()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string title = es ? "Rendimiento" : "Performance";
            string message = es
                ? "Este preset aplicará:\n\n• Todas las configuraciones en BAJO\n• Efectos visuales desactivados\n• Texturas comprimidas\n• Modo portátil activado\n• Optimizado para máximo FPS\n\n¿Deseas continuar?"
                : "This preset will apply:\n\n• All settings on LOW\n• Visual effects disabled\n• Compressed textures\n• Laptop mode enabled\n• Optimized for maximum FPS\n\nDo you want to continue?";

            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ApplyPreset("Performance");
            }
        }

        private void ApplyPreset(string presetName)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string optionsPath = GetOptionsIniPath();

            if (string.IsNullOrEmpty(optionsPath) || !File.Exists(optionsPath))
            {
                MessageBox.Show(
                    es ? "No se pudo encontrar Options.ini. Por favor, usa Game Tweaker para seleccionar la ubicación manualmente."
                       : "Could not find Options.ini. Please use Game Tweaker to select the location manually.",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lines = File.ReadAllLines(optionsPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    switch (presetName)
                    {
                        case "PrettyGraphics":
                            lines[i] = ApplyPrettyGraphicsSettings(line);
                            break;
                        case "Balanced":
                            lines[i] = ApplyBalancedSettings(line);
                            break;
                        case "Performance":
                            lines[i] = ApplyPerformanceSettings(line);
                            break;
                    }
                }

                File.WriteAllLines(optionsPath, lines);

                MessageBox.Show(
                    es ? "¡Preset aplicado exitosamente! Reinicia el juego para ver los cambios."
                       : "Preset applied successfully! Restart the game to see changes.",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"{(es ? "Error al aplicar preset: " : "Error applying preset: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string ApplyPrettyGraphicsSettings(string line)
        {
            if (line.StartsWith("simquality")) return "simquality = 4";
            if (line.StartsWith("objectquality")) return "objectquality = 4";
            if (line.StartsWith("lightingquality")) return "lightingquality = 4";
            if (line.StartsWith("terrainquality")) return "terrainquality = 4";
            if (line.StartsWith("generalreflections")) return "generalreflections = 4";
            if (line.StartsWith("viewdistance")) return "viewdistance = 4";
            if (line.StartsWith("edgesmoothing")) return "edgesmoothing = 2";
            if (line.StartsWith("visualeffects")) return "visualeffects = 4";
            if (line.StartsWith("postprocessing")) return "postprocessing = 1";
            if (line.StartsWith("useuncompressedtextures")) return "useuncompressedtextures = 1";
            if (line.StartsWith("advancedrendering")) return "advancedrendering = 1";

            return line;
        }

        private string ApplyBalancedSettings(string line)
        {
            if (line.StartsWith("simquality")) return "simquality = 2";
            if (line.StartsWith("objectquality")) return "objectquality = 2";
            if (line.StartsWith("lightingquality")) return "lightingquality = 2";
            if (line.StartsWith("terrainquality")) return "terrainquality = 2";
            if (line.StartsWith("generalreflections")) return "generalreflections = 3";
            if (line.StartsWith("viewdistance")) return "viewdistance = 3";
            if (line.StartsWith("edgesmoothing")) return "edgesmoothing = 1";
            if (line.StartsWith("visualeffects")) return "visualeffects = 1";
            if (line.StartsWith("postprocessing")) return "postprocessing = 0";
            if (line.StartsWith("useuncompressedtextures")) return "useuncompressedtextures = 1";
            if (line.StartsWith("advancedrendering")) return "advancedrendering = 0";

            return line;
        }

        private string ApplyPerformanceSettings(string line)
        {
            if (line.StartsWith("simquality")) return "simquality = 0";
            if (line.StartsWith("objectquality")) return "objectquality = 0";
            if (line.StartsWith("lightingquality")) return "lightingquality = 0";
            if (line.StartsWith("terrainquality")) return "terrainquality = 0";
            if (line.StartsWith("generalreflections")) return "generalreflections = 0";
            if (line.StartsWith("viewdistance")) return "viewdistance = 0";
            if (line.StartsWith("edgesmoothing")) return "edgesmoothing = 0";
            if (line.StartsWith("visualeffects")) return "visualeffects = 0";
            if (line.StartsWith("postprocessing")) return "postprocessing = 0";
            if (line.StartsWith("useuncompressedtextures")) return "useuncompressedtextures = 0";
            if (line.StartsWith("advancedrendering")) return "advancedrendering = 0";

            return line;
        }
        #endregion

        #region Game Tweaker
        private void GameTweakerButton_Click(object sender, RoutedEventArgs e)
        {
            var tweakerWindow = new GameTweakerWindow();
            tweakerWindow.Owner = this;
            tweakerWindow.ShowDialog();
        }
        #endregion

        #region Helpers
        private string GetOptionsIniPath()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Try "The Sims 4" first
            string path1 = Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Options.ini");
            if (File.Exists(path1)) return path1;

            // Try "Los Sims 4"
            string path2 = Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Options.ini");
            if (File.Exists(path2)) return path2;

            return null;
        }
        #endregion

        #region Actions
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}