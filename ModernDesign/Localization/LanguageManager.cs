using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ModernDesign.Localization
{
    public class LanguageManager : ILanguageManager
    {
        public event EventHandler<EventArgs> LanguageChanged;

        public bool IsSpanish { get; set; }

        public string CurrentLocale
        {
            get => _currentLocale;
            set
            {
                _currentLocale = value;
                LanguageChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static string _currentLocale =
            System.Threading.Thread.CurrentThread.CurrentCulture.Name; // default

        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>();

        private static IEnumerable<string> _supportedLanguages;

        public LanguageManager()
        {
            LoadLanguage();
            Initialize();
        }

        public bool LanguageSupported(string languageCode)
        {
            return _supportedLanguages.Contains(languageCode);
        }

        public string Get(string key)
        {
            if (Translations.TryGetValue(_currentLocale, out var localeDict) &&
                localeDict.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to English
            if (Translations.TryGetValue("en-US", out var enDict) &&
                enDict.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return $"[{key}]";
        }

        private void Initialize()
        {
            _supportedLanguages = Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(r => r.StartsWith("LeuanS4ToolKit.Localization."))
                .Select(r => r.Split('.')[2]);

            foreach (var supportedLanguage in _supportedLanguages)
            {
                LoadEmbeddedTranslations(supportedLanguage);
            }
        }

        private void LoadEmbeddedTranslations(string locale)
        {
            try
            {
                var resourceName = $"LeuanS4ToolKit.Localization.{locale}.json";

                Console.WriteLine($"Loading: {resourceName}");

                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Console.WriteLine($"Locale resource not found: {resourceName}");
                        return;
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        if (dict != null && dict.Count > 0)
                        {
                            Translations[locale] = dict;
                            Console.WriteLine($"Locale resource {locale}: {dict.Count} strings");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed load locale resource {locale}: {ex.Message}");
            }
        }

        private void LoadLanguage()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string iniPath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "language.ini");

                // Si no existe, por defecto inglés
                if (!File.Exists(iniPath))
                {
                    IsSpanish = false;
                    return;
                }

                var lines = File.ReadAllLines(iniPath);
                foreach (var raw in lines)
                {
                    var line = raw.Trim();
                    if (line.StartsWith("Language", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = line.Split('=');
                        if (parts.Length >= 2)
                        {
                            var value = parts[1].Trim();
                            CurrentLocale = value;
                            return;
                        }
                    }
                }

                // Si no se encontró la línea, inglés
                IsSpanish = false;
            }
            catch
            {
                // En caso de error leyendo el archivo: default inglés
                IsSpanish = false;
            }
        }
    }
}