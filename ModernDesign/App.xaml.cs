using System;
using System.IO;
using System.Windows;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace ModernDesign
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        { 
            var services = new ServiceCollection();
            services.AddSingleton<ILanguageManager, LanguageManager>();
            // add viemodels as transient here
            // services.AddTransient<ChildViewModel>();
            ServiceLocator.Current = services.BuildServiceProvider();
            
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string toolkitFolder = Path.Combine(appData, "Leuan's - Sims 4 ToolKit");
            string iniPath = Path.Combine(toolkitFolder, "language.ini");

            if (File.Exists(iniPath))
            {
                // Ya hay config: ir directo al SplashScreen
                SplashScreen splash = new SplashScreen();
                splash.Show();
            }
            else
            {
                // Primera vez: seleccionar idioma
                LanguageSelector lang = new LanguageSelector();
                lang.Show();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                //UnlockerService.CleanLocalUnlockerFiles();
            }
            catch
            {
                // no romper el cierre de la app
            }

            base.OnExit(e);
        }
    }
}
