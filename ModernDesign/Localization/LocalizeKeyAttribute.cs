using System;

namespace ModernDesign.Localization
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LocalizeKeyAttribute : Attribute
    {
        public string Key { get; }
        public LocalizeKeyAttribute(string key) => Key = key;
    }
}