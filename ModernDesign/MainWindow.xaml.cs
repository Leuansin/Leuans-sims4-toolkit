using ModernDesign.MVVM;
using ModernDesign.MVVM.View; // Para usar UnlockerService
using ModernDesign.MVVM.ViewModel;
using ModernDesign.Profile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ModernDesign.Localization;

namespace ModernDesign
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _cleanerTimer;
        private DispatcherTimer _ramMonitorTimer;
        private readonly Random _rng = new Random();
        private bool _ramWarningShown = false;

        private bool isChatbotOpen = false;
        private List<ChatbotResponse> chatbotResponses = new List<ChatbotResponse>();
        private static readonly HttpClient httpClient = new HttpClient();
        // Clase interna para respuestas del chatbot
        private class ChatbotResponse
        {
            public List<string> Keywords { get; set; } = new List<string>();
            public string ResponseES { get; set; }
            public string ResponseEN { get; set; }
            public string Action { get; set; } // acción a ejecutar

        }
        
        public MainWindow()
        {
            InitializeComponent();
            StartCleanerTimer();
            StartRamMonitorTimer();

            // Limpieza también cuando se cierre la ventana principal
            this.Closed += MainWindow_Closed;
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCustomBackground();
        }

        private void LoadCustomBackground()
        {
            var colors = UserSettingsManager.GetBackgroundColors();

            try
            {
                var gradient = new LinearGradientBrush();
                gradient.StartPoint = new Point(0, 0);
                gradient.EndPoint = new Point(1, 1);
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[0]), 0));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[1]), 0.45));
                gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(colors[2]), 1));

                MainBackgroundBorder.Background = gradient;
            }
            catch { }
        }

        private void StartCleanerTimer()
        {
            _cleanerTimer = new DispatcherTimer();
            _cleanerTimer.Tick += CleanerTimer_Tick;

            ScheduleNextClean(); // primera vez
            _cleanerTimer.Start();
        }

        private void StartRamMonitorTimer()
        {
            _ramMonitorTimer = new DispatcherTimer();
            _ramMonitorTimer.Interval = TimeSpan.FromSeconds(10); // Revisar cada 10 segundos
            _ramMonitorTimer.Tick += RamMonitorTimer_Tick;
            _ramMonitorTimer.Start();
        }

        private void RamMonitorTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Obtener el uso de RAM del proceso actual
                Process currentProcess = Process.GetCurrentProcess();
                long ramUsageBytes = currentProcess.WorkingSet64;
                double ramUsageMB = ramUsageBytes / (1024.0 * 1024.0);

                // Si supera 800MB y no hemos mostrado el warning
                if (ramUsageMB > 800 && !_ramWarningShown)
                {
                    _ramWarningShown = true;
                    ShowRamWarning(ramUsageMB);
                }
            }
            catch
            {
                // Ignorar errores en el monitoreo
            }
        }

        private void ShowRamWarning(double ramUsageMB)
        {
            var title = LanguageManager.Get("RamWarningTitle");
            var message = string.Format(
                LanguageManager.Get("RamWarningMessage"), 
                ramUsageMB.ToString("F0"));

            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private string GetLanguageCode()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string toolkitFolder = Path.Combine(appData, "Leuan's - Sims 4 ToolKit");
            string iniPath = Path.Combine(toolkitFolder, "language.ini");

            string languageCode = "en-US";

            try
            {
                if (File.Exists(iniPath))
                {
                    string[] lines = File.ReadAllLines(iniPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("Language = ", StringComparison.OrdinalIgnoreCase))
                        {
                            languageCode = line.Substring("Language = ".Length).Trim();
                            break;
                        }
                    }
                }
            }
            catch
            {
                // si falla lectura, nos quedamos con en-US
            }

            return languageCode;
        }

        private void ScheduleNextClean()
        {
            const double baseSeconds = 15.0;

            // jitter en segundos, por ejemplo entre -5 y +5
            double jitter = (_rng.NextDouble() * 10.0) - 5.0;

            double nextSeconds = baseSeconds + jitter;

            // nunca menos de 5 segundos, por seguridad
            if (nextSeconds < 5.0)
                nextSeconds = 5.0;

            _cleanerTimer.Interval = TimeSpan.FromSeconds(nextSeconds);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                //UnlockerService.CleanLocalUnlockerFiles();
            }
            catch
            {
                // ignorar errores aquí
            }

            // Detener timers
            _cleanerTimer?.Stop();
            _ramMonitorTimer?.Stop();
        }


        private void CleanerTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                //UnlockerService.CleanLocalUnlockerFiles();
            }
            catch
            {
                // no queremos romper la UI si algo falla al borrar
            }

            // programar el siguiente tick con nuevo jitter
            ScheduleNextClean();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ==================== CHATBOT ====================

        private async void ChatbotButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (!isChatbotOpen)
            {
                // ABRIR el chatbot
                ChatbotWindow.Visibility = Visibility.Visible;
                isChatbotOpen = true;

                if (chatbotResponses.Count == 0)
                {
                    await LoadChatbotResponsesAsync();
                }

                // Solo mostrar mensaje de bienvenida si no hay mensajes
                if (ChatMessagesPanel.Children.Count == 0)
                {
                    bool isSpanish = GetLanguageCode().StartsWith("es");
                    AddBotMessage(isSpanish
                        ? "¡Hola! 👋 Soy el asistente virtual que Leuan asignó para ti. Describe tu problema y te ayudaré a solucionarlo."
                        : "Hello! 👋 I'm the virtual assistant that Leuan assigned for you. Describe your problem and I'll help you solve it.");
                }
            }
            else
            {
                // CERRAR el chatbot
                ChatbotWindow.Visibility = Visibility.Collapsed;
                isChatbotOpen = false;
            }
        }

        private void CloseChatbot_Click(object sender, RoutedEventArgs e)
        {
            ChatbotWindow.Visibility = Visibility.Collapsed;
            isChatbotOpen = false;
            ChatMessagesPanel.Children.Clear();
        }

        private void ChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage_Click(sender, null);
            }
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = ChatInputBox.Text.Trim();
            if (string.IsNullOrEmpty(userMessage)) return;

            AddUserMessage(userMessage);
            ChatInputBox.Clear();
            ProcessUserMessage(userMessage);
        }

        private void AddUserMessage(string message)
        {
            Border messageBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                CornerRadius = new CornerRadius(12, 12, 0, 12),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(50, 5, 5, 5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 250 // Responsive
            };

            TextBlock textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                FontFamily = new FontFamily("Bahnschrift Light"),
                TextWrapping = TextWrapping.Wrap
            };

            messageBorder.Child = textBlock;
            ChatMessagesPanel.Children.Add(messageBorder);
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddBotMessage(string message)
        {
            Border messageBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                CornerRadius = new CornerRadius(12, 12, 12, 0),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(5, 5, 50, 5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 250, // Responsive
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")),
                BorderThickness = new Thickness(1)
            };

            TextBlock textBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 13,
                FontFamily = new FontFamily("Bahnschrift Light"),
                TextWrapping = TextWrapping.Wrap
            };

            messageBorder.Child = textBlock;
            ChatMessagesPanel.Children.Add(messageBorder);
            ChatScrollViewer.ScrollToBottom();
        }

        private async Task LoadChatbotResponsesAsync()
        {
            try
            {
                string url = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/Chatbot/chatbot.txt";
                string content = await httpClient.GetStringAsync(url);
                ParseChatbotResponses(content);
            }
            catch
            {
                chatbotResponses = GetDefaultResponses();
            }
        }

        private void ParseChatbotResponses(string content)
        {
            chatbotResponses.Clear();

            var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            ChatbotResponse currentResponse = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("[KEYWORDS]"))
                {
                    if (currentResponse != null)
                    {
                        chatbotResponses.Add(currentResponse);
                    }
                    currentResponse = new ChatbotResponse();
                }
                else if (trimmed.StartsWith("KEYWORDS=") && currentResponse != null)
                {
                    var keywordsStr = trimmed.Substring("KEYWORDS=".Length);
                    currentResponse.Keywords = keywordsStr.Split('|').Select(k => k.Trim().ToLower()).ToList();
                }
                else if (trimmed.StartsWith("RESPONSE_ES=") && currentResponse != null)
                {
                    currentResponse.ResponseES = trimmed.Substring("RESPONSE_ES=".Length).Replace("\\n", "\n");
                }
                else if (trimmed.StartsWith("RESPONSE_EN=") && currentResponse != null)
                {
                    currentResponse.ResponseEN = trimmed.Substring("RESPONSE_EN=".Length).Replace("\\n", "\n");
                }
                else if (trimmed.StartsWith("ACTION=") && currentResponse != null)
                {
                    currentResponse.Action = trimmed.Substring("ACTION=".Length).Trim();
                }
            }

            if (currentResponse != null)
            {
                chatbotResponses.Add(currentResponse);
            }
        }



        private List<ChatbotResponse> GetDefaultResponses()
        {
            return new List<ChatbotResponse>
    {
        new ChatbotResponse
        {
            Keywords = new List<string> { "dlc", "no aparece", "not showing", "unlock", "desbloquear" },
            ResponseES = "🔧 Solución: Asegúrate de instalar el EA DLC Unlocker.\n\n1. Ve a Home → Repair\n2. Haz clic en Install EA DLC Unlocker\n3. Reinicia el juego",
            ResponseEN = "🔧 Solution: Make sure to install the EA DLC Unlocker.\n\n1. Go to Home → Repair\n2. Click Install EA DLC Unlocker\n3. Restart the game"
        }
    };
        }

        private void ProcessUserMessage(string message)
        {
            string lowerMessage = message.ToLower();
            bool isSpanish = GetLanguageCode().StartsWith("es");

            foreach (var response in chatbotResponses)
            {
                if (response.Keywords.Any(keyword => lowerMessage.Contains(keyword)))
                {
                    string answer = isSpanish ? response.ResponseES : response.ResponseEN;
                    AddBotMessage(answer);

                    // ENVIAR TELEMETRÍA A DISCORD (async sin await para no bloquear UI)
                    //_ = SendChatbotTelemetryAsync(message, answer, response.Action ?? "");

                    // EJECUTAR ACCIÓN SI EXISTE
                    if (!string.IsNullOrEmpty(response.Action))
                    {
                        ExecuteChatbotAction(response.Action);
                    }

                    return;
                }
            }

            // Mensaje por defecto cuando no se encuentra respuesta
            string defaultMessage = isSpanish
                ? "🤔 No pude identificar tu problema específico.\n\n¿Necesitas ayuda personalizada?\n\nÚnete al Discord de Leuan:\n🔗 discord.gg/leuan"
                : "🤔 I couldn't identify your specific problem.\n\nNeed personalized help?\n\nJoin Leuan's Discord:\n🔗 discord.gg/leuan";

            AddBotMessage(defaultMessage);

            // ENVIAR TELEMETRÍA TAMBIÉN PARA PREGUNTAS SIN RESPUESTA
            //_ = SendChatbotTelemetryAsync(message, defaultMessage, "");
        }


        private async Task SendChatbotTelemetryAsync(string userQuestion, string botResponse, string action)
        {
            try
            {
                string webhookUrl = "https://discord.com/api/webhooks/1266183922107023360/GYpr3YE7anaS9tOHjnlqmakZgUpJ1HtK2mGmRsTl_zhyDOoVHwjZytcYdwSYPbuCAzud";

                // Obtener información del usuario (si existe)
                string username = "Usuario Anónimo";
                try
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string profilePath = Path.Combine(appData, "Leuan's - Sims 4 ToolKit", "profile.ini");
                    if (File.Exists(profilePath))
                    {
                        var lines = File.ReadAllLines(profilePath);
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("Username = ", StringComparison.OrdinalIgnoreCase))
                            {
                                username = line.Substring("Username = ".Length).Trim();
                                break;
                            }
                        }
                    }
                }
                catch { }

                // Determinar el emoji y descripción de la acción
                string actionEmoji = "💬";
                string actionDescription = "Sin acción específica";

                if (!string.IsNullOrEmpty(action))
                {
                    if (action.StartsWith("OPEN_URL:", StringComparison.OrdinalIgnoreCase))
                    {
                        actionEmoji = "🌐";
                        string url = action.Substring("OPEN_URL:".Length).Trim();
                        actionDescription = $"Abrir enlace: {url}";
                    }
                    else
                    {
                        switch (action.ToUpper())
                        {
                            case "OPEN_REPAIR_WINDOW":
                                actionEmoji = "🔧";
                                actionDescription = "Abrir ventana de Reparación (DLC Unlocker)";
                                break;
                            case "OPEN_UPDATER":
                                actionEmoji = "📥";
                                actionDescription = "Abrir Instalador de DLCs";
                                break;
                            case "OPEN_DISCOVERY":
                                actionEmoji = "🔍";
                                actionDescription = "Navegar a sección Discovery";
                                break;
                            case "OPEN_FPS_BOOSTER":
                                actionEmoji = "⚡";
                                actionDescription = "Navegar a FPS Booster";
                                break;
                            case "OPEN_SETTINGS":
                                actionEmoji = "⚙️";
                                actionDescription = "Navegar a Configuración";
                                break;
                            default:
                                actionDescription = $"Acción: {action}";
                                break;
                        }
                    }
                }

                // Escapar caracteres especiales para JSON
                string EscapeJson(string text)
                {
                    if (string.IsNullOrEmpty(text)) return "N/A";
                    return text.Replace("\\", "\\\\")
                               .Replace("\"", "\\\"")
                               .Replace("\n", "\\n")
                               .Replace("\r", "\\r")
                               .Replace("\t", "\\t");
                }

                // Crear el JSON manualmente
                string jsonPayload = $@"{{
            ""embeds"": [{{
                ""title"": ""🤖 Interacción del Chatbot"",
                ""description"": ""Un usuario ha interactuado con el asistente virtual del toolkit."",
                ""color"": 3447003,
                ""fields"": [
                    {{
                        ""name"": ""👤 Usuario"",
                        ""value"": ""```{EscapeJson(username)}```"",
                        ""inline"": true
                    }},
                    {{
                        ""name"": ""🕐 Fecha y Hora"",
                        ""value"": ""```{DateTime.Now:dd/MM/yyyy HH:mm:ss}```"",
                        ""inline"": true
                    }},
                    {{
                        ""name"": ""❓ Pregunta del Usuario"",
                        ""value"": ""```{EscapeJson(TruncateText(userQuestion, 1000))}```"",
                        ""inline"": false
                    }},
                    {{
                        ""name"": ""💡 Respuesta del Bot"",
                        ""value"": ""```{EscapeJson(TruncateText(botResponse, 1000))}```"",
                        ""inline"": false
                    }},
                    {{
                        ""name"": ""{actionEmoji} Acción Ejecutada"",
                        ""value"": ""```{EscapeJson(actionDescription)}```"",
                        ""inline"": false
                    }}
                ],
                ""footer"": {{
                    ""text"": ""Leuan's Sims 4 ToolKit - Chatbot Telemetry""
                }},
                ""timestamp"": ""{DateTime.UtcNow:o}""
            }}]
        }}";

                // Enviar al webhook
                var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                await httpClient.PostAsync(webhookUrl, content);
            }
            catch (Exception ex)
            {
                // Ignorar errores de telemetría para no afectar la experiencia del usuario
                System.Diagnostics.Debug.WriteLine($"Error enviando telemetría: {ex.Message}");
            }
        }

        // Método auxiliar para truncar texto largo
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "N/A";
            if (text.Length <= maxLength) return text;
            return text.Substring(0, maxLength - 3) + "...";
        }

        private void ExecuteChatbotAction(string action)
        {
            if (string.IsNullOrEmpty(action)) return;

            try
            {
                // DETECTAR SI ES UNA URL
                if (action.StartsWith("OPEN_URL:", StringComparison.OrdinalIgnoreCase))
                {
                    string url = action.Substring("OPEN_URL:".Length).Trim();

                    // Abrir URL en el navegador predeterminado
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    return;
                }

                switch (action.ToUpper())
                {
                    case "OPEN_DLC_UPDATER":
                        // Abrir DLC Updater
                        InstallModeSelector installmodeWindow = new InstallModeSelector();
                        installmodeWindow.Owner = this;
                        installmodeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        installmodeWindow.ShowDialog();
                        break;

                    case "OPEN_DLC_UNLOCKER":
                        // Abrir DLC Unlocker
                        DLCUnlockerWindow dlcunlockerWindow = new DLCUnlockerWindow();
                        dlcunlockerWindow.Owner = this;
                        dlcunlockerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        dlcunlockerWindow.ShowDialog();
                        break;

                    case "UPDATE_GAME":
                        // Abrir Main Selection View
                        MainSelectionWindow mainSelectionWindow = new MainSelectionWindow();
                        mainSelectionWindow.Owner = this;
                        mainSelectionWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        mainSelectionWindow.ShowDialog();
                        break;

                    case "CRACK_GAME":
                        // Abrir Cracking Tool Window
                        CrackingToolWindow crackingToolWindow = new CrackingToolWindow();
                        crackingToolWindow.Owner = this;
                        crackingToolWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        crackingToolWindow.ShowDialog();
                        break;

                    case "DOWNLOAD_BASEGAME":
                        // Abrir Install Method Selector Window
                        InstallMethodSelectorWindow installMethodWindow = new InstallMethodSelectorWindow();
                        installMethodWindow.Owner = this;
                        installMethodWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        installMethodWindow.ShowDialog();
                        break;

                    case "MOD_MANAGER":
                        // Abrir Organize Mods Window
                        OrganizeModsWindow organizeModsWindow = new OrganizeModsWindow();
                        organizeModsWindow.Owner = this;
                        organizeModsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        organizeModsWindow.ShowDialog();
                        break;

                    case "LOADING_SCREEN":
                        // Abrir Loading Screen Selector Window
                        LoadingScreenSelectorWindow loadingScreenWindow = new LoadingScreenSelectorWindow();
                        loadingScreenWindow.Owner = this;
                        loadingScreenWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        loadingScreenWindow.ShowDialog();
                        break;

                    case "CHEATS_GUIDE":
                        // Navegar a Cheats Guide View
                        if (DataContext is MainViewModel mainVM)
                        {
                            mainVM.CurrentView = new CheatsGuideView();
                        }
                        break;

                    case "GAMEPLAY_ENHANCER":
                        // Navegar a Gameplay Enhancer View
                        if (DataContext is MainViewModel mainVM2)
                        {
                            mainVM2.CurrentView = new GameplayEnhancerView();
                        }
                        break;

                    case "AUTOEXTRACT_DLCS":
                        // Abrir Semi Auto Installer Window
                        SemiAutoInstallerWindow semiAutoWindow = new SemiAutoInstallerWindow();
                        semiAutoWindow.Owner = this;
                        semiAutoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        semiAutoWindow.ShowDialog();
                        break;

                    case "REPAIR_GAME":
                        // Abrir Repair Logger Window
                        RepairLoggerWindow repairWindow = new RepairLoggerWindow();
                        repairWindow.Owner = this;
                        repairWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        repairWindow.ShowDialog();
                        break;

                    case "CHANGE_LANGUAGE":
                        // Abrir Language Selector Window
                        LanguageSelectorWindow languageWindow = new LanguageSelectorWindow();
                        languageWindow.Owner = this;
                        languageWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        languageWindow.ShowDialog();
                        break;

                    case "GAME_TWEAKER":
                        // Abrir Game Tweaker Window
                        GameTweakerWindow gameTweakerWindow = new GameTweakerWindow();
                        gameTweakerWindow.Owner = this;
                        gameTweakerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        gameTweakerWindow.ShowDialog();
                        break;

                    case "SCREENSHOT_MANAGER":
                        // Navegar a Gallery Manager View
                        if (DataContext is MainViewModel mainVM3)
                        {
                            mainVM3.CurrentView = new GalleryManagerWindow();
                        }
                        break;

                    case "MUSIC_MANAGER":
                        // Navegar a Music Manager View
                        if (DataContext is MainViewModel mainVM4)
                        {
                            mainVM4.CurrentView = new MusicManagerView();
                        }
                        break;

                    case "SAVEGAME_MANAGER":
                        // Navegar a Save Games View
                        if (DataContext is MainViewModel mainVM5)
                        {
                            mainVM5.CurrentView = new SaveGamesView();
                        }
                        break;

                    case "LEARN_MODDING":
                        // Abrir S4S Categories Window
                        S4SCategoriesWindow s4sWindow = new S4SCategoriesWindow();
                        s4sWindow.Owner = this;
                        s4sWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        s4sWindow.ShowDialog();
                        break;

                    case "FIX_COMMON":
                        // Abrir Fix Common Errores Window
                        FixCommonErrorsWindow fixCommonWindow = new FixCommonErrorsWindow();
                        fixCommonWindow.Owner = this;
                        fixCommonWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        fixCommonWindow.ShowDialog();
                        break;

                    case "METHOD50_50":
                        // Abrir Method 50/50 Window
                        Method5050Window method5050Window = new Method5050Window();
                        method5050Window.Owner = this;
                        method5050Window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        method5050Window.ShowDialog();
                        break;

                    default:
                        // Acción no reconocida
                        break;
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores opcional
                System.Diagnostics.Debug.WriteLine($"Error ejecutando acción del chatbot: {ex.Message}");
            }
        }

    }
}