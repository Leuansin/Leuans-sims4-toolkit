using System;

namespace ModernDesign.Localization
{
    public interface ILanguageManager
    {
        event EventHandler<EventArgs> LanguageChanged;
        bool IsSpanish { get; set; }
        string CurrentLocale { get; set; }

        bool LanguageSupported(string languageCode);
        
        string Get(string key);
    }
}