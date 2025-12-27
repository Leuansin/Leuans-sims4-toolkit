using System.Windows;
using System.Windows.Controls;
using ModernDesign.Localization;

namespace LeuanS4ToolKit.Core
{
    public static class LocalizationProperties
    {
        public static readonly DependencyProperty LocalizeTextProperty =
            DependencyProperty.RegisterAttached(
                "LocalizeText", typeof(string), typeof(LocalizationProperties),
                new PropertyMetadata(null, OnLocalizeTextChanged));

        public static string GetLocalizeText(DependencyObject obj) =>
            (string)obj.GetValue(LocalizeTextProperty);

        public static void SetLocalizeText(DependencyObject obj, string value) =>
            obj.SetValue(LocalizeTextProperty, value);

        private static void OnLocalizeTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && ServiceLocator.Get<ILanguageManager>() is ILanguageManager lm)
            {
                var key = (string)e.NewValue;
                var localizedText = lm.Get(key);

                if (!string.IsNullOrEmpty(localizedText))
                {
                    if (fe is TextBlock tb) tb.Text = localizedText;
                    if (fe is Button btn) btn.Content = localizedText;
                }
            }
        }

        public static readonly DependencyProperty LocalizeTitleProperty =
            DependencyProperty.RegisterAttached(
                "LocalizeTitle", typeof(string), typeof(LocalizationProperties),
                new PropertyMetadata(null, OnLocalizeTitleChanged));

        public static string GetLocalizeTitle(DependencyObject obj) =>
            (string)obj.GetValue(LocalizeTitleProperty);

        public static void SetLocalizeTitle(DependencyObject obj, string value) =>
            obj.SetValue(LocalizeTitleProperty, value);

        private static void OnLocalizeTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && ServiceLocator.Get<ILanguageManager>() is ILanguageManager lm)
            {
                var key = (string)e.NewValue;
                var localizedTitle = lm.Get(key);

                if (!string.IsNullOrEmpty(localizedTitle))
                    window.Title = localizedTitle;

                lm.LanguageChanged += (s, args) =>
                    window.Title = lm.Get(key);
            }
        }

        public static readonly DependencyProperty LocalizeTooltipProperty =
            DependencyProperty.RegisterAttached(
                "LocalizeTooltip", typeof(string), typeof(LocalizationProperties),
                new PropertyMetadata(null, OnLocalizeTooltipChanged));

        public static string GetLocalizeTooltip(DependencyObject obj) =>
            (string)obj.GetValue(LocalizeTooltipProperty);

        public static void SetLocalizeTooltip(DependencyObject obj, string value) =>
            obj.SetValue(LocalizeTooltipProperty, value);

        private static void OnLocalizeTooltipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe && ServiceLocator.Get<ILanguageManager>() is ILanguageManager lm)
            {
                var key = (string)e.NewValue;
                var localizedTooltip = lm.Get(key);

                if (!string.IsNullOrEmpty(localizedTooltip))
                    fe.ToolTip = localizedTooltip;

                // Подписка на смену языка
                lm.LanguageChanged += (s, args) =>
                    fe.ToolTip = lm.Get(key);
            }
        }
    }
}