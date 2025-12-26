using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ModernDesign.Localization
{
    public static class LanguageManager
    {
        public static bool IsSpanish { get; private set; }

        private static readonly Dictionary<string, Dictionary<string, string>> Translations =
            new Dictionary<string, Dictionary<string, string>>();
        private static string _currentLocale = "en"; // default
        static LanguageManager()
        {
            LoadLanguage();
            Initialize();
        }

        public static string CurrentLocale 
        { 
            get => _currentLocale; 
            set => _currentLocale = value; 
        }
        
        public static void Initialize()
        {
            var allResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            Console.WriteLine("ALL RESOURCES: " + string.Join(" | ", allResources));
            
            
            LoadEmbeddedTranslations("en-US");
            LoadEmbeddedTranslations("es-ES");
            LoadEmbeddedTranslations("ru-RU");
        }
        
        private static void LoadEmbeddedTranslations(string locale)
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

                    using( var reader = new StreamReader(stream))
                    {
                        var json = reader.ReadToEnd();
                        var dict = JsonSerializer.Deserialize<Dictionary<string, string >> (json);

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
        
        public static string Get(string key)
        {
            if (Translations.TryGetValue(_currentLocale, out var localeDict) && 
                localeDict.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to English
            if (Translations.TryGetValue("en", out var enDict) && 
                enDict.TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return $"[{key}]";
        }
        
        public static void LoadLanguage()
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
                            _currentLocale = value;
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
