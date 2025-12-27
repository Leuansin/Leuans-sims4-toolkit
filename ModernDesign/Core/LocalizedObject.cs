using System;
using System.Collections.Generic;
using System.Linq;
using ModernDesign.Localization;
using ModernDesign.MVVM.ViewModel;
using System.Reflection;

namespace LeuanS4ToolKit.Core
{
    public class LocalizedObservableObject : ObservableObject
    {
        internal readonly ILanguageManager _lm;
        private readonly List<string> _localizedProperties = new List<string>();

        protected LocalizedObservableObject()
        {
            InitLocalization();
            _lm = ServiceLocator.Get<ILanguageManager>();
            _lm.LanguageChanged += OnLanguageChanged;
        }

        internal string GetPropertyKey()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var propertyFrame = stackTrace.GetFrame(1);
            var propertyMethod = propertyFrame.GetMethod();

            var property = GetType().GetProperties()
                .FirstOrDefault(p => p.GetMethod == propertyMethod);

            return property?.GetCustomAttribute<LocalizeKeyAttribute>()?.Key
                   ?? throw new InvalidOperationException("LocalizeKey attribute not found");
        }

        private void InitLocalization()
        {
            _localizedProperties.Clear();
            var props = GetType().GetProperties()
                .Where(p => p.GetCustomAttribute<LocalizeKeyAttribute>() != null);

            foreach (var prop in props)
                _localizedProperties.Add(prop.Name);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            foreach (var propName in _localizedProperties)
            {
                OnPropertyChanged(propName);
            }
        }
    }
}