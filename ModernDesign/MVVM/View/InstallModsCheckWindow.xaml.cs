using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class InstallModsCheckWindow : Window
    {
        private string _modsFolderPath;

        public InstallModsCheckWindow()
        {
            InitializeComponent();
            ApplyLanguage();
            CheckDefaultPaths();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            Title = es ? "Instalar mods – Comprobación" : "Install Mods – Checker";
            WindowTitleText.Text = es ? "Instalar mods – Comprobación" : "Install Mods – Checker";
            CheckerIntroText.Text = es ? "Voy a buscar automáticamente la carpeta Mods en tus documentos de The Sims 4." : "I'll automatically search for the Mods folder in your The Sims 4 documents.";
            ManualGroupBox.Header = es ? "¿No la encontré? Dime dónde están tus documentos de The Sims 4" : "Didn't find it? Tell me where your The Sims 4 documents are";
            ManualExampleText.Text = es ? "Ejemplo: C:\\Users\\TuUsuario\\Documents\\Electronic Arts\\The Sims 4" : "Example: C:\\Users\\YourUser\\Documents\\Electronic Arts\\The Sims 4";
            BrowseButton.Content = es ? "Buscar..." : "Browse...";
            VerifyButton.Content = es ? "Verificar y crear carpeta Mods" : "Verify & create Mods folder";
            CloseMainButton.Content = es ? "Cerrar" : "Close";
            SelectDifferentFolderButton.Content = es ? "📂 ¿Esta no es tu carpeta? Selecciona otra" : "📂 Not your folder? Select different one";
            OpenModsFolderButton.Content = es ? "📁 Abrir carpeta Mods" : "📁 Open Mods Folder";
        }

        private void CheckDefaultPaths()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] basePaths = {
                    Path.Combine(docs, "Electronic Arts", "The Sims 4"),
                    Path.Combine(docs, "Electronic Arts", "Los Sims 4"),
                    Path.Combine(docs, "Origin", "The Sims 4"),
                    Path.Combine(docs, "Origin", "Los Sims 4")
                };

                _modsFolderPath = null;
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string header = es ? "Buscando carpeta Mods en:\n" : "Searching Mods folder in:\n";
                string infoText = header;

                foreach (var basePath in basePaths)
                {
                    infoText += $"• {basePath}\n";
                    string mods = Path.Combine(basePath, "Mods");
                    if (Directory.Exists(mods)) { _modsFolderPath = mods; break; }
                }

                AutoPathText.Text = infoText.TrimEnd();

                if (_modsFolderPath != null)
                {
                    ShowSuccess(es ? $"✅ Se encontró una carpeta Mods válida:\n{_modsFolderPath}\n\nTu juego soporta mods correctamente." : $"✅ A valid Mods folder was found:\n{_modsFolderPath}\n\nYour game supports mods correctly.");
                    OpenModsFolderButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowError(es ? "❌ No se encontró la carpeta Mods en las rutas típicas.\n\nNo se pueden instalar mods todavía. Si conoces la carpeta de documentos de tu juego, indícala abajo para intentar arreglarlo." : "❌ Mods folder was not found in the typical paths.\n\nYou can't install mods yet. If you know your game documents folder, set it below to fix this.");
                    OpenModsFolderButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ShowError(ServiceLocator.Get<ILanguageManager>().IsSpanish ? $"Ocurrió un error al revisar las rutas:\n{ex.Message}" : $"An error occurred while checking paths:\n{ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));
            StatusText.Text = message;
        }

        private void ShowSuccess(string message)
        {
            StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#064E3B"));
            StatusText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6EE7B7"));
            StatusText.Text = message;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = ServiceLocator.Get<ILanguageManager>().IsSpanish ? "Selecciona la carpeta de documentos de The Sims 4" : "Select The Sims 4 documents folder";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    ManualPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void SelectDifferentFolder_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = es ? "Selecciona la carpeta de documentos de The Sims 4 (donde está o debería estar la carpeta Mods)" : "Select The Sims 4 documents folder (where the Mods folder is or should be)";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selected = dialog.SelectedPath;
                    string modsPath = Path.Combine(selected, "Mods");

                    if (Directory.Exists(modsPath))
                    {
                        _modsFolderPath = modsPath;
                        ShowSuccess(es ? $"✅ Se encontró una carpeta Mods válida:\n{_modsFolderPath}\n\nTu juego soporta mods correctamente." : $"✅ A valid Mods folder was found:\n{_modsFolderPath}\n\nYour game supports mods correctly.");
                        OpenModsFolderButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        var result = System.Windows.MessageBox.Show(
                            es ? $"No se encontró una carpeta 'Mods' en:\n{selected}\n\n¿Deseas crearla ahora?" : $"No 'Mods' folder found in:\n{selected}\n\nDo you want to create it now?",
                            es ? "Crear carpeta Mods" : "Create Mods folder",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                Directory.CreateDirectory(modsPath);
                                _modsFolderPath = modsPath;
                                ShowSuccess(es ? $"✅ Se creó la carpeta Mods:\n{_modsFolderPath}\n\nTu juego ahora soporta mods correctamente." : $"✅ Mods folder was created:\n{_modsFolderPath}\n\nYour game now supports mods correctly.");
                                OpenModsFolderButton.Visibility = Visibility.Visible;
                            }
                            catch (Exception ex)
                            {
                                ShowError(es ? $"Error al crear la carpeta:\n{ex.Message}" : $"Error creating folder:\n{ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void VerifyManualPath_Click(object sender, RoutedEventArgs e)
        {
            string basePath = ManualPathTextBox.Text?.Trim();
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            if (string.IsNullOrWhiteSpace(basePath)) { ShowError(es ? "Por favor ingresa una ruta válida." : "Please enter a valid path."); return; }

            try
            {
                if (!Directory.Exists(basePath)) { ShowError(es ? "La ruta indicada no existe. Revisa que la hayas escrito correctamente." : "The specified path does not exist. Please check the path."); return; }

                string modsPath = Path.Combine(basePath, "Mods");
                if (!Directory.Exists(modsPath)) Directory.CreateDirectory(modsPath);

                _modsFolderPath = modsPath;
                ShowSuccess(es ? $"✅ Se ha validado la carpeta de documentos:\n{basePath}\n\nSe creó/confirmó la carpeta Mods:\n{modsPath}\n\nTu juego ahora soporta mods correctamente." : $"✅ Documents folder was validated:\n{basePath}\n\nMods folder was created/confirmed:\n{modsPath}\n\nYour game now supports mods correctly.");
                OpenModsFolderButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ShowError(es ? $"No se pudo verificar o crear la carpeta Mods:\n{ex.Message}" : $"Could not verify or create Mods folder:\n{ex.Message}");
            }
        }

        private void OpenModsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_modsFolderPath) && Directory.Exists(_modsFolderPath))
            {
                try { Process.Start(new ProcessStartInfo { FileName = _modsFolderPath, UseShellExecute = true }); }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ServiceLocator.Get<ILanguageManager>().IsSpanish ? $"No se pudo abrir la carpeta:\n{ex.Message}" : $"Could not open folder:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}