using ModernDesign.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.IO.Compression;
using LeuanS4ToolKit.Core;
using WpfMessageBox = System.Windows.MessageBox;

namespace ModernDesign.MVVM.View
{
    public partial class MusicManagerView : Window
    {
        private string musicDownloaderPath = "";
        private string downloaderExePath = "";
        private string customMusicPath = "";
        private List<DownloadedSong> downloadedSongs = new List<DownloadedSong>();
        private const string DOWNLOADER_URL = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/LeuMusic.zip";

        // ✅ 5 API KEYS CON ROTACIÓN AUTOMÁTICA
        private static readonly string[] YOUTUBE_API_KEYS = new string[]
        {
            "AIzaSyCDcHpWQGH0KL4TvtulBf13tbcAK3Whl-A",
            "AIzaSyAoQal6zNkkyeZCGDEQWDtasMT0AM2ibQU",
            "AIzaSyABm_GA5zlZpcbIdD97tQSQ2nb7blMQbpg",
            "AIzaSyBU0SDniSX5KeYaSll36YXdMDDprruZV2A",
            "AIzaSyBllvxLxKrs3lD5PRbtVHuV-wfs7qITca4"
        };

        private static int currentApiKeyIndex = 0;
        private static int apiCallsToday = 0;
        private static DateTime lastResetDate = DateTime.Today;

        private bool isSearching = false;

        public class DownloadedSong
        {
            public string Title { get; set; }
            public string FilePath { get; set; }
            public string ThumbnailPath { get; set; }
            public long FileSize { get; set; }
            public DateTime DownloadDate { get; set; }
        }

        public MusicManagerView()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            this.Title = es ? "Gestor de Música - Descargas Personalizadas" : "Music Manager - Custom Downloads";

            TitleText.Text = es ? "🎵 Gestor de Música" : "🎵 Music Manager";
            SubtitleText.Text = es ? "Descarga y administra tu música personalizada" : "Download and manage your custom music";

            SearchPlaceholder.Text = es ? "Buscar en YouTube..." : "Search on YouTube...";
            MyDownloadsButton.Content = es ? "📁 Mis Descargas" : "📁 My Downloads";
            BackToSearchButton.Content = es ? "⬅️ Volver a Búsqueda" : "⬅️ Back to Search";
            RefreshButton.Content = es ? "🔄 Actualizar" : "🔄 Refresh";
            OrganizeButton.Content = es ? "📂 Organizar" : "📂 Organize";
            ChangeFolderButton.Content = es ? "📁 Cambiar Carpeta" : "📁 Change Folder";

            QualityLabel.Text = es ? "Calidad:" : "Quality:";
            TurboModeCheck.Content = es ? "Modo Turbo (Rápido)" : "Turbo Mode (Fast)";
            QualityModeCheck.Content = es ? "Modo Calidad (Alta)" : "Quality Mode (High)";
            UseCookiesCheck.Content = es ? "Usar Cookies" : "Use Cookies";

            EmptySearchText.Text = es ? "Busca cualquier video de YouTube" : "Search any YouTube video";
            EmptySearchSubText.Text = es ? "Escribe el nombre de una canción, artista o video" : "Type a song name, artist or video";

            EmptyDownloadsText.Text = es ? "No tienes descargas aún" : "You don't have downloads yet";
            EmptyDownloadsSubText.Text = es ? "Busca y descarga música desde YouTube" : "Search and download music from YouTube";

            RepositoryLink.Text = es
                ? "Ver Repositorio del Descargador"
                : "See Downloader Repository";
        }

        private string GetNextApiKey()
        {
            // Resetear contador diario
            if (DateTime.Today > lastResetDate)
            {
                apiCallsToday = 0;
                lastResetDate = DateTime.Today;
                currentApiKeyIndex = 0;
                Debug.WriteLine("🔄 Contador de API reseteado para nuevo día");
            }

            // Rotar cada 90 búsquedas (90% del límite de 100)
            if (apiCallsToday > 0 && apiCallsToday % 90 == 0)
            {
                currentApiKeyIndex = (currentApiKeyIndex + 1) % YOUTUBE_API_KEYS.Length;
                Debug.WriteLine($"🔑 Rotando a API Key #{currentApiKeyIndex + 1}");
            }

            apiCallsToday++;
            Debug.WriteLine($"📊 Búsquedas hoy: {apiCallsToday} | API Key actual: #{currentApiKeyIndex + 1}");

            return YOUTUBE_API_KEYS[currentApiKeyIndex];
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeMusicDownloader();
            await DetectCustomMusicFolder();
            await LoadMyDownloads();
        }

        private async Task DetectCustomMusicFolder()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string[] possiblePaths = new string[]
                {
                    Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Custom Music", "Pop"),
                    Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Custom Music", "Pop"),
                    Path.Combine(documentsPath, "Origin", "Los Sims 4", "Custom Music", "Pop"),
                    Path.Combine(documentsPath, "Origin", "The Sims 4", "Custom Music", "Pop")
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        customMusicPath = Path.GetDirectoryName(path);
                        FolderPathText.Text = $"📁 {AbbreviatePath(customMusicPath, 50)}";
                        return;
                    }
                }

                FolderPathText.Text = es ? "📁 Carpeta no detectada - Click en 'Cambiar Carpeta'" : "📁 Folder not detected - Click 'Change Folder'";

                WpfMessageBox.Show(
                    es ? "No se pudo detectar la carpeta 'Custom Music' de Los Sims 4.\n\nPor favor selecciónala manualmente."
                       : "Could not detect The Sims 4 'Custom Music' folder.\n\nPlease select it manually.",
                    es ? "Carpeta No Encontrada" : "Folder Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error detectando carpeta: " : "Error detecting folder: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string AbbreviatePath(string path, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            int charsToShow = (maxLength - 3) / 2;
            return path.Substring(0, charsToShow) + "..." + path.Substring(path.Length - charsToShow);
        }

        private async Task InitializeMusicDownloader()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                musicDownloaderPath = Path.Combine(appDataPath, "Leuan's - Sims 4 ToolKit", "music_downloader");
                downloaderExePath = Path.Combine(musicDownloaderPath, "LeuMusic.exe");

                if (!File.Exists(downloaderExePath))
                {
                    var result = WpfMessageBox.Show(
                        es ? "El descargador de música no está instalado.\n\n¿Deseas descargarlo ahora?"
                           : "The music downloader is not installed.\n\nDo you want to download it now?",
                        es ? "Descargador No Encontrado" : "Downloader Not Found",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await DownloadAndExtractDownloader();
                    }
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error inicializando descargador: " : "Error initializing downloader: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DownloadAndExtractDownloader()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                ShowProgress(es ? "Descargando LeuMusic..." : "Downloading LeuMusic...", 0);

                string tempPath = Path.Combine(Path.GetTempPath(), "LeuMusic_temp");
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                Directory.CreateDirectory(tempPath);

                string zipPath = Path.Combine(tempPath, "LeuMusic.zip");

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);

                    var response = await client.GetAsync(DOWNLOADER_URL, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        var isMoreToRead = true;

                        do
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                            {
                                isMoreToRead = false;
                            }
                            else
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;

                                if (canReportProgress)
                                {
                                    var progress = (double)totalRead / totalBytes * 100;
                                    ShowProgress(es ? $"Descargando... {progress:0}%" : $"Downloading... {progress:0}%", progress);
                                }
                            }
                        }
                        while (isMoreToRead);
                    }
                }

                ShowProgress(es ? "Extrayendo archivos..." : "Extracting files...", 100);

                if (!Directory.Exists(musicDownloaderPath))
                    Directory.CreateDirectory(musicDownloaderPath);

                // Extraer usando .NET nativo
                ZipFile.ExtractToDirectory(zipPath, musicDownloaderPath);

                Directory.Delete(tempPath, true);

                HideProgress();

                WpfMessageBox.Show(
                    es ? "¡Descargador instalado exitosamente!" : "Downloader installed successfully!",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                HideProgress();
                WpfMessageBox.Show(
                    $"{(es ? "Error descargando/extrayendo: " : "Error downloading/extracting: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void SearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await PerformSearch();
            }
        }

        private async Task PerformSearch()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string query = SearchBox.Text.Trim();

                if (string.IsNullOrEmpty(query))
                {
                    WpfMessageBox.Show(
                        es ? "Por favor ingresa un término de búsqueda" : "Please enter a search term",
                        es ? "Búsqueda Vacía" : "Empty Search",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                isSearching = true;
                SearchResultsGrid.Children.Clear();
                LoadingSearchPanel.Visibility = Visibility.Visible;
                EmptySearchPanel.Visibility = Visibility.Collapsed;
                SearchResultsGrid.Visibility = Visibility.Collapsed;

                var results = await SearchYouTube(query);

                LoadingSearchPanel.Visibility = Visibility.Collapsed;

                if (results.Count == 0)
                {
                    EmptySearchPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    SearchResultsGrid.Visibility = Visibility.Visible;
                    foreach (var result in results)
                    {
                        CreateSearchResultCard(result);
                    }
                }

                isSearching = false;
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                isSearching = false;
                LoadingSearchPanel.Visibility = Visibility.Collapsed;
                WpfMessageBox.Show(
                    $"{(es ? "Error en búsqueda: " : "Search error: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task<List<YouTubeSearchResult>> SearchYouTube(string query)
        {
            var results = new List<YouTubeSearchResult>();
            int maxRetries = YOUTUBE_API_KEYS.Length;
            int currentRetry = 0;
            bool allApiKeysExhausted = false;

            // ✅ PASO 1: Intentar con YouTube Data API
            while (currentRetry < maxRetries)
            {
                try
                {
                    string apiKey = GetNextApiKey();

                    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = apiKey,
                        ApplicationName = "LeuMusic Manager"
                    });

                    var searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.Q = query;
                    searchListRequest.MaxResults = 5;
                    searchListRequest.Type = "video";
                    searchListRequest.VideoCategoryId = "10";
                    searchListRequest.Order = Google.Apis.YouTube.v3.SearchResource.ListRequest.OrderEnum.Relevance;

                    var searchListResponse = await searchListRequest.ExecuteAsync();

                    foreach (var searchResult in searchListResponse.Items)
                    {
                        if (searchResult.Id.Kind == "youtube#video")
                        {
                            results.Add(new YouTubeSearchResult
                            {
                                VideoId = searchResult.Id.VideoId,
                                Title = searchResult.Snippet.Title,
                                Duration = 0,
                                Thumbnail = searchResult.Snippet.Thumbnails?.Medium?.Url ??
                                           searchResult.Snippet.Thumbnails?.Default__?.Url ??
                                           $"https://img.youtube.com/vi/{searchResult.Id.VideoId}/mqdefault.jpg"
                            });
                        }
                    }

                    Debug.WriteLine($"✅ Búsqueda exitosa con API Key #{currentApiKeyIndex + 1}");
                    return results;
                }
                catch (Google.GoogleApiException ex)
                {
                    if (ex.HttpStatusCode == System.Net.HttpStatusCode.Forbidden &&
                        ex.Message.Contains("quotaExceeded"))
                    {
                        currentRetry++;
                        currentApiKeyIndex = (currentApiKeyIndex + 1) % YOUTUBE_API_KEYS.Length;
                        Debug.WriteLine($"⚠️ API Key #{currentApiKeyIndex} agotada, rotando...");

                        if (currentRetry >= maxRetries)
                        {
                            allApiKeysExhausted = true;
                            Debug.WriteLine("❌ Todas las API Keys agotadas, cambiando a scraping...");
                            break;
                        }
                        continue;
                    }
                    else
                    {
                        Debug.WriteLine($"❌ Error de API: {ex.Message}");
                        allApiKeysExhausted = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error general: {ex.Message}");
                    allApiKeysExhausted = true;
                    break;
                }
            }

            // ✅ PASO 2: Si todas las APIs fallaron, usar scraping con yt-dlp
            if (allApiKeysExhausted || results.Count == 0)
            {
                Debug.WriteLine("🔄 Intentando búsqueda con yt-dlp (scraping)...");
                results = await SearchYouTubeWithScraping(query);
            }

            return results;
        }

        private async Task<List<YouTubeSearchResult>> SearchYouTubeWithScraping(string query)
        {
            var results = new List<YouTubeSearchResult>();

            await Task.Run(() =>
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "yt-dlp",
                        Arguments = $"--get-id --get-title --flat-playlist \"ytsearch5:{query}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = System.Text.Encoding.UTF8
                    };

                    Process process = Process.Start(psi);
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        Debug.WriteLine($"⚠️ yt-dlp error: {errors}");
                        return;
                    }

                    // Parsear resultados (vienen en pares: título, ID, título, ID, ...)
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        if (i + 1 < lines.Length)
                        {
                            string title = lines[i].Trim();
                            string videoId = lines[i + 1].Trim();

                            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(videoId))
                            {
                                results.Add(new YouTubeSearchResult
                                {
                                    VideoId = videoId,
                                    Title = title,
                                    Duration = 0,
                                    Thumbnail = $"https://img.youtube.com/vi/{videoId}/mqdefault.jpg"
                                });
                            }
                        }
                    }

                    if (results.Count > 0)
                    {
                        Debug.WriteLine($"✅ Scraping exitoso: {results.Count} resultados");
                    }
                    else
                    {
                        Debug.WriteLine("⚠️ Scraping no devolvió resultados");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error en scraping: {ex.Message}");
                }
            });

            return results;
        }

        private void CreateSearchResultCard(YouTubeSearchResult result)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("SearchResultCard"),
                Width = 280,
                Height = 200
            };

            Grid cardGrid = new Grid();
            card.Child = cardGrid;

            Border thumbnailBorder = new Border
            {
                Height = 120,
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
            };

            if (!string.IsNullOrEmpty(result.Thumbnail))
            {
                Image thumbnail = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    Source = new BitmapImage(new Uri(result.Thumbnail))
                };
                thumbnailBorder.Child = thumbnail;
            }
            else
            {
                TextBlock placeholder = new TextBlock
                {
                    Text = "🎵",
                    FontSize = 48,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White
                };
                thumbnailBorder.Child = placeholder;
            }

            cardGrid.Children.Add(thumbnailBorder);

            StackPanel infoPanel = new StackPanel
            {
                Margin = new Thickness(12, 130, 12, 12),
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock titleText = new TextBlock
            {
                Text = result.Title,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxHeight = 32,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };

            Button downloadBtn = new Button
            {
                Content = "⬇️ Descargar",
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Padding = new Thickness(8, 6, 8, 6),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            downloadBtn.Click += async (s, e) => await DownloadVideo(result);

            infoPanel.Children.Add(titleText);
            infoPanel.Children.Add(downloadBtn);
            cardGrid.Children.Add(infoPanel);

            SearchResultsGrid.Children.Add(card);
        }

        private async Task DownloadVideo(YouTubeSearchResult result)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (string.IsNullOrEmpty(customMusicPath) || !Directory.Exists(customMusicPath))
                {
                    WpfMessageBox.Show(
                        es ? "Por favor selecciona la carpeta 'Custom Music' primero." : "Please select the 'Custom Music' folder first.",
                        es ? "Carpeta No Configurada" : "Folder Not Configured",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!File.Exists(downloaderExePath))
                {
                    WpfMessageBox.Show(
                        es ? "El descargador no está instalado." : "Downloader is not installed.",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ShowProgress(es ? "Descargando..." : "Downloading...", 0);

                string popFolder = Path.Combine(customMusicPath, "Pop");
                if (!Directory.Exists(popFolder))
                    Directory.CreateDirectory(popFolder);

                string arguments = $"-video=\"{result.Title}\" -dir=\"{popFolder}\" -silent";

                if (TurboModeCheck.IsChecked == true)
                    arguments += " -turbo";
                else if (QualityModeCheck.IsChecked == true)
                    arguments += " -quality";

                if (UseCookiesCheck.IsChecked == true)
                    arguments += " -cookies";

                bool downloadSuccess = false;
                string errorMessage = "";

                await Task.Run(() =>
                {
                    try
                    {
                        string originalPath = Environment.GetEnvironmentVariable("PATH");
                        string newPath = musicDownloaderPath + ";" + originalPath;

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = downloaderExePath,
                            Arguments = arguments,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            WorkingDirectory = musicDownloaderPath
                        };

                        psi.EnvironmentVariables["PATH"] = newPath;

                        Process process = Process.Start(psi);

                        string output = process.StandardOutput.ReadToEnd();
                        string errors = process.StandardError.ReadToEnd();

                        bool finished = process.WaitForExit(600000);

                        if (finished && process.ExitCode == 0)
                        {
                            downloadSuccess = true;
                        }
                        else if (!finished)
                        {
                            process.Kill();
                            errorMessage = es ? "Descarga cancelada por timeout (10 min)" : "Download cancelled by timeout (10 min)";
                        }
                        else
                        {
                            errorMessage = $"Exit code: {process.ExitCode}.\nOutput: {output}\nErrors: {errors}";
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Error ejecutando descargador: {ex.Message}";
                    }
                });

                HideProgress();

                if (downloadSuccess)
                {
                    var result2 = WpfMessageBox.Show(
                        es ? "¡Descarga completada!\n\n¿Deseas ir a 'Mis Descargas' para organizar tu música?\n\n(Es importante organizarla para que funcione correctamente en el juego)"
                           : "Download completed!\n\nDo you want to go to 'My Downloads' to organize your music?\n\n(It's important to organize it for it to work correctly in the game)",
                        es ? "Éxito" : "Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result2 == MessageBoxResult.Yes)
                    {
                        await LoadMyDownloads();
                        SearchPanel.Visibility = Visibility.Collapsed;
                        DownloadsPanel.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    WpfMessageBox.Show(
                        $"{(es ? "La descarga no se completó correctamente.\n\nError: " : "Download did not complete successfully.\n\nError: ")}{errorMessage}",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                HideProgress();
                WpfMessageBox.Show(
                    $"{(es ? "Error descargando: " : "Error downloading: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadMyDownloads()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                DownloadsGrid.Children.Clear();
                downloadedSongs.Clear();

                if (string.IsNullOrEmpty(customMusicPath) || !Directory.Exists(customMusicPath))
                {
                    EmptyDownloadsPanel.Visibility = Visibility.Visible;
                    DownloadsGrid.Visibility = Visibility.Collapsed;
                    DownloadCountText.Text = es ? "0 descargas" : "0 downloads";
                    return;
                }

                var allMp3Files = Directory.GetFiles(customMusicPath, "*.mp3", SearchOption.AllDirectories);

                if (allMp3Files.Length == 0)
                {
                    EmptyDownloadsPanel.Visibility = Visibility.Visible;
                    DownloadsGrid.Visibility = Visibility.Collapsed;
                    DownloadCountText.Text = es ? "0 descargas" : "0 downloads";
                    return;
                }

                foreach (var mp3 in allMp3Files)
                {
                    FileInfo fileInfo = new FileInfo(mp3);
                    string thumbnailPath = Path.ChangeExtension(mp3, ".webp");

                    downloadedSongs.Add(new DownloadedSong
                    {
                        Title = Path.GetFileNameWithoutExtension(mp3),
                        FilePath = mp3,
                        ThumbnailPath = File.Exists(thumbnailPath) ? thumbnailPath : null,
                        FileSize = fileInfo.Length,
                        DownloadDate = fileInfo.CreationTime
                    });
                }

                EmptyDownloadsPanel.Visibility = Visibility.Collapsed;
                DownloadsGrid.Visibility = Visibility.Visible;

                foreach (var song in downloadedSongs)
                {
                    CreateDownloadedSongCard(song);
                }

                DownloadCountText.Text = es
                    ? $"{downloadedSongs.Count} descarga{(downloadedSongs.Count != 1 ? "s" : "")}"
                    : $"{downloadedSongs.Count} download{(downloadedSongs.Count != 1 ? "s" : "")}";
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error cargando descargas: " : "Error loading downloads: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CreateDownloadedSongCard(DownloadedSong song)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("SearchResultCard"),
                Width = 220,
                Height = 260
            };

            Grid cardGrid = new Grid();
            card.Child = cardGrid;

            Border thumbnailBorder = new Border
            {
                Height = 160,
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
            };

            if (!string.IsNullOrEmpty(song.ThumbnailPath) && File.Exists(song.ThumbnailPath))
            {
                Image thumbnail = new Image
                {
                    Stretch = Stretch.UniformToFill,
                    Source = new BitmapImage(new Uri(song.ThumbnailPath))
                };
                thumbnailBorder.Child = thumbnail;
            }
            else
            {
                TextBlock placeholder = new TextBlock
                {
                    Text = "🎵",
                    FontSize = 64,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White
                };
                thumbnailBorder.Child = placeholder;
            }

            cardGrid.Children.Add(thumbnailBorder);

            StackPanel infoPanel = new StackPanel
            {
                Margin = new Thickness(12, 170, 12, 12),
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock titleText = new TextBlock
            {
                Text = song.Title,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock sizeText = new TextBlock
            {
                Text = FormatFileSize(song.FileSize),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontSize = 9,
                Margin = new Thickness(0, 0, 0, 10)
            };

            UniformGrid buttonsGrid = new UniformGrid
            {
                Rows = 1,
                Columns = 2,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Button playBtn = CreateActionButton("▶️", "#6366F1", "#4F46E5");
            playBtn.Click += (s, e) => PlaySong(song.FilePath);
            buttonsGrid.Children.Add(playBtn);

            Button deleteBtn = CreateActionButton("🗑️", "#EF4444", "#DC2626");
            deleteBtn.Click += async (s, e) => await DeleteSong(song);
            buttonsGrid.Children.Add(deleteBtn);

            infoPanel.Children.Add(titleText);
            infoPanel.Children.Add(sizeText);
            infoPanel.Children.Add(buttonsGrid);
            cardGrid.Children.Add(infoPanel);

            DownloadsGrid.Children.Add(card);
        }

        private Button CreateActionButton(string icon, string bgColor, string hoverColor)
        {
            Button btn = new Button
            {
                Content = icon,
                FontSize = 11,
                Margin = new Thickness(2),
                Cursor = System.Windows.Input.Cursors.Hand,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.White,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor))
            };

            btn.Template = new ControlTemplate(typeof(Button))
            {
                VisualTree = CreateButtonTemplate()
            };

            Color normalColor = (Color)ColorConverter.ConvertFromString(bgColor);
            Color hover = (Color)ColorConverter.ConvertFromString(hoverColor);

            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(hover);
            btn.MouseLeave += (s, e) => btn.Background = new SolidColorBrush(normalColor);

            return btn;
        }

        private FrameworkElementFactory CreateButtonTemplate()
        {
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.PaddingProperty, new Thickness(8, 6, 8, 6));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            border.AppendChild(content);
            return border;
        }

        private void PlaySong(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error reproduciendo: " : "Error playing: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DeleteSong(DownloadedSong song)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                var result = WpfMessageBox.Show(
                    es ? $"¿Eliminar '{song.Title}'?" : $"Delete '{song.Title}'?",
                    es ? "Confirmar" : "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(song.FilePath);
                    if (!string.IsNullOrEmpty(song.ThumbnailPath) && File.Exists(song.ThumbnailPath))
                    {
                        File.Delete(song.ThumbnailPath);
                    }

                    await LoadMyDownloads();
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error eliminando: " : "Error deleting: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void MyDownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Collapsed;
            DownloadsPanel.Visibility = Visibility.Visible;
        }

        private void BackToSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchPanel.Visibility = Visibility.Visible;
            DownloadsPanel.Visibility = Visibility.Collapsed;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadMyDownloads();
        }

        private void OrganizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(customMusicPath) || !Directory.Exists(customMusicPath))
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    es ? "Por favor selecciona la carpeta 'Custom Music' primero." : "Please select the 'Custom Music' folder first.",
                    es ? "Carpeta No Configurada" : "Folder Not Configured",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var organizeView = new OrganizeMusicView(customMusicPath)
            {
                Owner = this
            };

            if (organizeView.ShowDialog() == true)
            {
                _ = LoadMyDownloads();
            }
        }

        private void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = es ? "Selecciona la carpeta 'Custom Music' de Los Sims 4" : "Select The Sims 4 'Custom Music' folder",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                customMusicPath = dialog.SelectedPath;
                FolderPathText.Text = $"📁 {AbbreviatePath(customMusicPath, 50)}";
                _ = LoadMyDownloads();
            }
        }

        private void RepositoryLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Leuansin/LeuMusic-Downloader",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowProgress(string title, double value)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressTitle.Text = title;
                ProgressBar.Value = value;
                ProgressText.Text = $"{value:0}%";
                ProgressOverlay.Visibility = Visibility.Visible;
            });
        }

        private void HideProgress()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressOverlay.Visibility = Visibility.Collapsed;
            });
        }

        private void TurboModeCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (QualityModeCheck != null)
                QualityModeCheck.IsChecked = false;
        }

        private void QualityModeCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (TurboModeCheck != null)
                TurboModeCheck.IsChecked = true;
        }
    }

    public class YouTubeSearchResult
    {
        public string VideoId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public string Thumbnail { get; set; }
    }
}