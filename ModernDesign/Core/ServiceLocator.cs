using System;
using Microsoft.Extensions.DependencyInjection;

namespace LeuanS4ToolKit.Core
{
    public static class ServiceLocator
    {
        public static IServiceProvider Current { get; set; }
    
        public static T Get<T>() => Current.GetRequiredService<T>();
        public static object Get(Type type) => Current.GetRequiredService(type);
    }
}