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
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class GalleryManagerWindow : Window
    {
        private string screenshotsPath = "";
        private List<string> photoFiles = new List<string>();
        private List<string> albumFolders = new List<string>();
        private const string COMPRESSOR_URL = "https://zeroauno.blob.core.windows.net/leuan/TheSims4/Utility/leuan-compressor.exe";
        private string compressorPath = "";
        private string currentAlbum = null;

        public GalleryManagerWindow()
        {
            InitializeComponent();
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            this.Title = es ? "Gestor de Galería - Capturas de Los Sims 4" : "Gallery Manager - Sims 4 Screenshots";

            TitleText.Text = es ? "🖼️ Gestor de Galería" : "🖼️ Gallery Manager";
            SubtitleText.Text = es ? "Administra tus capturas de pantalla de Los Sims 4" : "Manage your Sims 4 screenshots";

            ChangeFolderButton.Content = es ? "Cambiar Carpeta" : "Change Folder";
            RefreshButton.Content = es ? "🔄 Actualizar" : "🔄 Refresh";
            CompressAllButton.Content = es ? "📦 Comprimir Todas" : "📦 Compress All";
            CreateAlbumButton.Content = es ? "📁 Crear Álbum" : "📁 Create Album";
            BackToRootButton.Content = es ? "⬅️ Volver" : "⬅️ Back";

            LoadingText.Text = es ? "Cargando capturas..." : "Loading screenshots...";
            EmptyText.Text = es ? "No se encontraron capturas" : "No screenshots found";
            EmptySubText.Text = es ? "Toma algunas capturas en Los Sims 4 con la tecla 'C'" : "Take some screenshots in The Sims 4 with 'C' key";
            HowToTakeScreenshotsText.Text = es ? "Cómo tomar Capturas de Pantalla" : "How to take Screenshots";

            RepositoryLink.Text = es
            ? "Ver Repositorio y código del Compresor"
            : "See Compressor Source and Repository";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await DetectScreenshotsFolder();
            await LoadPhotos();
        }

        private string AbbreviatePath(string path, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
                return path;

            int charsToShow = (maxLength - 3) / 2;
            return path.Substring(0, charsToShow) + "..." + path.Substring(path.Length - charsToShow);
        }

        private void UpdateFolderPathText()
        {
            string displayPath = currentAlbum == null
                ? screenshotsPath
                : Path.Combine(screenshotsPath, currentAlbum);

            string abbreviatedPath = AbbreviatePath(displayPath, 50);
            FolderPathText.Text = $"📁 {abbreviatedPath}";

            BackToRootButton.Visibility = currentAlbum == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task DetectScreenshotsFolder()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string[] possiblePaths = new string[]
                {
                    Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Capturas de pantalla"),
                    Path.Combine(documentsPath, "Electronic Arts", "Los Sims 4", "Screenshots"),
                    Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Capturas de pantalla"),
                    Path.Combine(documentsPath, "Electronic Arts", "The Sims 4", "Screenshots")
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        screenshotsPath = path;
                        UpdateFolderPathText();
                        return;
                    }
                }

                FolderPathText.Text = es ? "📁 Carpeta de capturas no encontrada" : "📁 Screenshots folder not found";
                MessageBox.Show(
                    es ? "No se pudo encontrar la carpeta de capturas de Los Sims 4. Por favor selecciónala manualmente."
                       : "Could not find The Sims 4 screenshots folder. Please select it manually.",
                    es ? "Carpeta No Encontrada" : "Folder Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error detectando carpeta: " : "Error detecting folder: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void HowToTakeScreenshots_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            string message = es
                ? "Para tomar capturas de pantalla en Los Sims 4:\n\n" +
                  "1. Presiona la tecla 'C' durante el juego\n" +
                  "2. Las capturas se guardarán automáticamente en:\n" +
                  "   Documents/Electronic Arts/The Sims 4/Screenshots\n\n" +
                  "Tip: Usa 'Tab' para ocultar la interfaz antes de capturar.\n\n" +
                  "¿Quieres ver un video tutorial?"
                : "To take screenshots in The Sims 4:\n\n" +
                  "1. Press the 'C' key during gameplay\n" +
                  "2. Screenshots will be automatically saved to:\n" +
                  "   Documents/Electronic Arts/The Sims 4/Screenshots\n\n" +
                  "Tip: Use 'Tab' to hide the UI before capturing.\n\n" +
                  "Do you want to watch a video tutorial?";

            var result = MessageBox.Show(message,
                es ? "Cómo tomar Capturas" : "How to take Screenshots",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.youtube.com/watch?v=9DPKt9lZPbg&t=24s",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        es ? $"Error abriendo el navegador: {ex.Message}" : $"Error opening browser: {ex.Message}",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadPhotos()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                LoadingPanel.Visibility = Visibility.Visible;
                EmptyPanel.Visibility = Visibility.Collapsed;
                PhotosGrid.Visibility = Visibility.Collapsed;
                PhotosGrid.Children.Clear();
                photoFiles.Clear();
                albumFolders.Clear();

                if (string.IsNullOrEmpty(screenshotsPath) || !Directory.Exists(screenshotsPath))
                {
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    EmptyPanel.Visibility = Visibility.Visible;
                    return;
                }

                string currentPath = currentAlbum == null
                    ? screenshotsPath
                    : Path.Combine(screenshotsPath, currentAlbum);

                await Task.Run(() =>
                {
                    if (currentAlbum == null)
                    {
                        albumFolders = Directory.GetDirectories(currentPath).ToList();
                    }

                    // Cargar archivos .png, .jpg y .jpeg
                    var pngFiles = Directory.GetFiles(currentPath, "*.png").ToList();
                    var jpgFiles = Directory.GetFiles(currentPath, "*.jpg").ToList();
                    var jpegFiles = Directory.GetFiles(currentPath, "*.jpeg").ToList();

                    photoFiles = pngFiles.Concat(jpgFiles).Concat(jpegFiles).ToList();
                });

                foreach (string albumPath in albumFolders)
                {
                    CreateAlbumCard(albumPath);
                }

                foreach (string photoPath in photoFiles)
                {
                    CreatePhotoCard(photoPath);
                }

                LoadingPanel.Visibility = Visibility.Collapsed;

                if (photoFiles.Count == 0 && albumFolders.Count == 0)
                {
                    EmptyPanel.Visibility = Visibility.Visible;
                    PhotoCountText.Text = es ? "0 elementos" : "0 items";
                }
                else
                {
                    PhotosGrid.Visibility = Visibility.Visible;
                    PhotoCountText.Text = es
                        ? $"{photoFiles.Count} foto{(photoFiles.Count != 1 ? "s" : "")}, {albumFolders.Count} álbum{(albumFolders.Count != 1 ? "es" : "")}"
                        : $"{photoFiles.Count} photo{(photoFiles.Count != 1 ? "s" : "")}, {albumFolders.Count} album{(albumFolders.Count != 1 ? "s" : "")}";
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error cargando fotos: " : "Error loading photos: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateAlbumCard(string albumPath)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            string folderName = Path.GetFileName(albumPath);

            string albumName = folderName;
            if (folderName.StartsWith("Leuan's ToolKit - Album "))
            {
                albumName = folderName.Substring("Leuan's ToolKit - Album ".Length);
            }

            // Contar archivos .png, .jpg y .jpeg
            int photoCount = Directory.GetFiles(albumPath, "*.png").Length +
                             Directory.GetFiles(albumPath, "*.jpg").Length +
                             Directory.GetFiles(albumPath, "*.jpeg").Length;

            Border card = new Border
            {
                Style = (Style)FindResource("PhotoCard"),
                Width = 220,
                Height = 280,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748"))
            };

            Grid cardGrid = new Grid();
            card.Child = cardGrid;

            Border iconBorder = new Border
            {
                Height = 160,
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new LinearGradientBrush(
                    (Color)ColorConverter.ConvertFromString("#4F46E5"),
                    (Color)ColorConverter.ConvertFromString("#7C3AED"),
                    90)
            };

            TextBlock albumIcon = new TextBlock
            {
                Text = "📁",
                FontSize = 64,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBorder.Child = albumIcon;
            cardGrid.Children.Add(iconBorder);

            StackPanel infoPanel = new StackPanel
            {
                Margin = new Thickness(12, 170, 12, 12),
                VerticalAlignment = VerticalAlignment.Top
            };

            TextBlock albumNameText = new TextBlock
            {
                Text = albumName,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock photoCountText = new TextBlock
            {
                Text = es ? $"{photoCount} foto{(photoCount != 1 ? "s" : "")}" : $"{photoCount} photo{(photoCount != 1 ? "s" : "")}",
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            };

            infoPanel.Children.Add(albumNameText);
            infoPanel.Children.Add(photoCountText);

            UniformGrid buttonsGrid = new UniformGrid
            {
                Rows = 1,
                Columns = 2,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Button openBtn = CreateActionButton("📂", "#22C55E", "#16A34A");
            openBtn.Click += (s, e) => OpenAlbum(folderName);
            buttonsGrid.Children.Add(openBtn);

            Button deleteBtn = CreateActionButton("🗑️", "#EF4444", "#DC2626");
            deleteBtn.Click += (s, e) => DeleteAlbum(albumPath);
            buttonsGrid.Children.Add(deleteBtn);

            infoPanel.Children.Add(buttonsGrid);
            cardGrid.Children.Add(infoPanel);

            PhotosGrid.Children.Add(card);
        }

        private void CreatePhotoCard(string photoPath)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("PhotoCard"),
                Width = 220,
                Height = 280
            };

            Grid cardGrid = new Grid();
            card.Child = cardGrid;

            Border imageBorder = new Border
            {
                Height = 160,
                CornerRadius = new CornerRadius(12, 12, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"))
            };

            Image thumbnail = new Image
            {
                Stretch = Stretch.UniformToFill,
                Source = LoadThumbnail(photoPath)
            };
            imageBorder.Child = thumbnail;
            cardGrid.Children.Add(imageBorder);

            StackPanel infoPanel = new StackPanel
            {
                Margin = new Thickness(12, 170, 12, 12),
                VerticalAlignment = VerticalAlignment.Top
            };

            FileInfo fileInfo = new FileInfo(photoPath);

            TextBlock fileName = new TextBlock
            {
                Text = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock fileSize = new TextBlock
            {
                Text = FormatFileSize(fileInfo.Length),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontSize = 9,
                Margin = new Thickness(0, 0, 0, 10)
            };

            infoPanel.Children.Add(fileName);
            infoPanel.Children.Add(fileSize);

            UniformGrid buttonsGrid = new UniformGrid
            {
                Rows = 2,
                Columns = 2,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            Button viewBtn = CreateActionButton("👁️", "#6366F1", "#4F46E5");
            viewBtn.Click += (s, e) => ViewPhoto(photoPath);
            buttonsGrid.Children.Add(viewBtn);

            Button renameBtn = CreateActionButton("✏️", "#F59E0B", "#D97706");
            renameBtn.Click += (s, e) => RenamePhoto(photoPath);
            buttonsGrid.Children.Add(renameBtn);

            Button deleteBtn = CreateActionButton("🗑️", "#EF4444", "#DC2626");
            deleteBtn.Click += (s, e) => DeletePhoto(photoPath);
            buttonsGrid.Children.Add(deleteBtn);

            Button emailBtn = CreateActionButton("📧", "#8B5CF6", "#7C3AED");
            emailBtn.Click += (s, e) => SendByEmail(photoPath);
            buttonsGrid.Children.Add(emailBtn);

            infoPanel.Children.Add(buttonsGrid);
            cardGrid.Children.Add(infoPanel);

            PhotosGrid.Children.Add(card);
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

            btn.MouseEnter += (s, e) =>
            {
                btn.Background = new SolidColorBrush(hover);
                var transform = new System.Windows.Media.ScaleTransform(1, 1);
                btn.RenderTransform = transform;
                btn.RenderTransformOrigin = new Point(0.5, 0.5);

                var anim = new System.Windows.Media.Animation.DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(150));
                transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, anim);
                transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, anim);
            };

            btn.MouseLeave += (s, e) =>
            {
                btn.Background = new SolidColorBrush(normalColor);
                if (btn.RenderTransform is System.Windows.Media.ScaleTransform transform)
                {
                    var anim = new System.Windows.Media.Animation.DoubleAnimation(1.1, 1, TimeSpan.FromMilliseconds(150));
                    transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, anim);
                    transform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, anim);
                }
            };

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

        private BitmapImage LoadThumbnail(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 220;
                bitmap.UriSource = new Uri(path);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
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

        private void OpenAlbum(string albumName)
        {
            currentAlbum = albumName;
            UpdateFolderPathText();
            _ = LoadPhotos();
        }

        private async void DeleteAlbum(string albumPath)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string albumName = Path.GetFileName(albumPath);

                string displayName = albumName;
                if (albumName.StartsWith("Leuan's ToolKit - Album "))
                {
                    displayName = albumName.Substring("Leuan's ToolKit - Album ".Length);
                }

                var result = MessageBox.Show(
                    es ? $"¿Estás seguro de que quieres eliminar el álbum '{displayName}'?\n\nLas fotos dentro del álbum se moverán de vuelta a la carpeta principal."
                       : $"Are you sure you want to delete the album '{displayName}'?\n\nPhotos inside the album will be moved back to the main folder.",
                    es ? "Confirmar Eliminación" : "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Mover todas las fotos del álbum de vuelta a la carpeta principal (.png, .jpg, .jpeg)
                    var pngPhotos = Directory.GetFiles(albumPath, "*.png");
                    var jpgPhotos = Directory.GetFiles(albumPath, "*.jpg");
                    var jpegPhotos = Directory.GetFiles(albumPath, "*.jpeg");

                    var photosInAlbum = pngPhotos.Concat(jpgPhotos).Concat(jpegPhotos).ToArray();

                    foreach (string photoPath in photosInAlbum)
                    {
                        string fileName = Path.GetFileName(photoPath);
                        string destPath = Path.Combine(screenshotsPath, fileName);

                        // Si ya existe un archivo con el mismo nombre, agregar un sufijo
                        int counter = 1;
                        while (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            destPath = Path.Combine(screenshotsPath, $"{nameWithoutExt}_{counter}{extension}");
                            counter++;
                        }

                        File.Move(photoPath, destPath);
                    }

                    // Ahora eliminar la carpeta vacía
                    Directory.Delete(albumPath, false);

                    await LoadPhotos();

                    MessageBox.Show(
                        es ? $"¡Álbum '{displayName}' eliminado exitosamente!\n\n{photosInAlbum.Length} foto{(photosInAlbum.Length != 1 ? "s" : "")} movida{(photosInAlbum.Length != 1 ? "s" : "")} a la carpeta principal."
                           : $"Album '{displayName}' deleted successfully!\n\n{photosInAlbum.Length} photo{(photosInAlbum.Length != 1 ? "s" : "")} moved to the main folder.",
                        es ? "Éxito" : "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error eliminando álbum: " : "Error deleting album: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void RenamePhoto(string photoPath)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                string currentName = Path.GetFileNameWithoutExtension(photoPath);

                var inputDialog = new Window
                {
                    Title = es ? "Renombrar Foto" : "Rename Photo",
                    Width = 400,
                    Height = 180,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    WindowStyle = WindowStyle.ToolWindow,
                    ResizeMode = ResizeMode.NoResize,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"))
                };

                var stack = new StackPanel { Margin = new Thickness(20) };
                stack.Children.Add(new TextBlock
                {
                    Text = es ? "Nuevo nombre:" : "New name:",
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = Brushes.White,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold
                });

                var textBox = new TextBox
                {
                    Text = currentName,
                    Margin = new Thickness(0, 0, 0, 20),
                    Padding = new Thickness(8),
                    FontSize = 13,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A")),
                    Foreground = Brushes.White,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                    BorderThickness = new Thickness(2)
                };
                stack.Children.Add(textBox);

                var buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                var okButton = new Button
                {
                    Content = "OK",
                    Width = 80,
                    Height = 32,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsDefault = true,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold
                };
                okButton.Click += (s, e) => inputDialog.DialogResult = true;

                var cancelButton = new Button
                {
                    Content = es ? "Cancelar" : "Cancel",
                    Width = 80,
                    Height = 32,
                    IsCancel = true,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold
                };

                buttonsPanel.Children.Add(okButton);
                buttonsPanel.Children.Add(cancelButton);
                stack.Children.Add(buttonsPanel);

                inputDialog.Content = stack;

                if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    string newName = textBox.Text.Trim();
                    string directory = Path.GetDirectoryName(photoPath);
                    string extension = Path.GetExtension(photoPath);
                    string newPath = Path.Combine(directory, newName + extension);

                    if (File.Exists(newPath))
                    {
                        MessageBox.Show(
                            es ? "Ya existe un archivo con ese nombre." : "A file with that name already exists.",
                            es ? "Error" : "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    File.Move(photoPath, newPath);
                    await LoadPhotos();
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error renombrando foto: " : "Error renaming photo: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ViewPhoto(string photoPath)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(photoPath));
                ViewerImage.Source = bitmap;

                FileInfo fileInfo = new FileInfo(photoPath);
                ViewerFileName.Text = fileInfo.Name;
                ViewerFileSize.Text = FormatFileSize(fileInfo.Length);

                PhotoViewerOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error viendo foto: " : "Error viewing photo: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void DeletePhoto(string photoPath)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                var result = MessageBox.Show(
                    es ? $"¿Estás seguro de que quieres eliminar esta foto?\n\n{Path.GetFileName(photoPath)}"
                       : $"Are you sure you want to delete this photo?\n\n{Path.GetFileName(photoPath)}",
                    es ? "Confirmar Eliminación" : "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    File.Delete(photoPath);
                    await LoadPhotos();
                    MessageBox.Show(
                        es ? "¡Foto eliminada exitosamente!" : "Photo deleted successfully!",
                        es ? "Éxito" : "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error eliminando foto: " : "Error deleting photo: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task DownloadCompressor()
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (!string.IsNullOrEmpty(compressorPath) && File.Exists(compressorPath))
                    return;

                compressorPath = Path.Combine(screenshotsPath, "leuan-compressor.exe");

                if (File.Exists(compressorPath))
                    return;

                ShowProgress(es ? "Descargando compresor..." : "Downloading compressor...", 0);

                using (HttpClient client = new HttpClient())
                {
                    byte[] data = await client.GetByteArrayAsync(COMPRESSOR_URL);
                    File.WriteAllBytes(compressorPath, data);
                }

                HideProgress();
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                HideProgress();
                MessageBox.Show(
                    $"{(es ? "Error descargando compresor: " : "Error downloading compressor: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SendByEmail(string photoPath)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:?subject={(es ? "Captura de Los Sims 4" : "Sims 4 Screenshot")}&body={(es ? "¡Mira esta captura!" : "Check out this screenshot!")}",
                    UseShellExecute = true
                });

                MessageBox.Show(
                    es ? $"Por favor adjunta este archivo manualmente:\n\n{photoPath}"
                       : $"Please attach this file manually:\n\n{photoPath}",
                    es ? "Cliente de Correo Abierto" : "Email Client Opened",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    es ? $"Error abriendo cliente de correo: {ex.Message}\n\nPuedes adjuntar el archivo manualmente:\n{photoPath}"
                       : $"Error opening email client: {ex.Message}\n\nYou can manually attach the file:\n{photoPath}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private async void CompressAllButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (photoFiles.Count == 0)
                {
                    MessageBox.Show(
                        es ? "No hay fotos para comprimir." : "No photos to compress.",
                        es ? "Info" : "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    es ? $"¿Comprimir todas las {photoFiles.Count} fotos a JPG?\n\nEsto puede tomar un tiempo."
                       : $"Compress all {photoFiles.Count} photos to JPG?\n\nThis may take a while.",
                    es ? "Confirmar" : "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // Preguntar si quiere eliminar los originales
                var deleteOriginals = MessageBox.Show(
                    es ? "¿Te gustaría eliminar permanentemente las fotos originales después de comprimirlas?\n\n" +
                         "• SÍ: Las fotos PNG originales serán eliminadas después de la compresión\n" +
                         "• NO: Las fotos PNG originales se mantendrán como respaldo"
                       : "Would you like to permanently delete the original photos after compressing them?\n\n" +
                         "• YES: Original PNG photos will be deleted after compression\n" +
                         "• NO: Original PNG photos will be kept as backup",
                    es ? "Eliminar Originales" : "Delete Originals",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                await DownloadCompressor();

                if (string.IsNullOrEmpty(compressorPath) || !File.Exists(compressorPath))
                {
                    MessageBox.Show(
                        es ? "Compresor no disponible." : "Compressor not available.",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                ShowProgress(es ? "Comprimiendo todas las fotos..." : "Compressing all photos...", 0);

                string currentPath = currentAlbum == null
                    ? screenshotsPath
                    : Path.Combine(screenshotsPath, currentAlbum);

                // Construir argumentos según la elección del usuario
                string arguments = deleteOriginals == MessageBoxResult.Yes
                    ? "-in ./ -out ./ -q 80 -deletefinal"
                    : "-in ./ -out ./ -q 80";

                bool compressionSuccess = false;

                await Task.Run(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = compressorPath,
                        Arguments = arguments,
                        WorkingDirectory = currentPath,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process process = Process.Start(psi);
                    process.WaitForExit();

                    // Si el proceso terminó exitosamente (código de salida 0)
                    compressionSuccess = process.ExitCode == 0;
                });

                HideProgress();

                // Eliminar el compressor.exe después de usarlo exitosamente
                if (compressionSuccess && !string.IsNullOrEmpty(compressorPath) && File.Exists(compressorPath))
                {
                    try
                    {
                        File.Delete(compressorPath);
                        compressorPath = ""; // Resetear la ruta
                    }
                    catch
                    {
                        // Si no se puede eliminar, no es crítico, simplemente lo ignoramos
                    }
                }

                await LoadPhotos();

                string successMessage = deleteOriginals == MessageBoxResult.Yes
                    ? (es ? "¡Todas las fotos comprimidas exitosamente!\n\nLas fotos originales han sido eliminadas."
                          : "All photos compressed successfully!\n\nOriginal photos have been deleted.")
                    : (es ? "¡Todas las fotos comprimidas exitosamente!\n\nLas fotos originales se mantuvieron como respaldo."
                          : "All photos compressed successfully!\n\nOriginal photos were kept as backup.");

                MessageBox.Show(
                    successMessage,
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                HideProgress();
                MessageBox.Show(
                    $"{(es ? "Error comprimiendo fotos: " : "Error compressing photos: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RepositoryLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Leuansin/PNG-to-JPG-Bulk-Compresser",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    es ? $"Error abriendo el navegador: {ex.Message}" : $"Error opening browser: {ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void CreateAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

                if (currentAlbum != null)
                {
                    MessageBox.Show(
                        es ? "Solo puedes crear álbumes desde la carpeta principal." : "You can only create albums from the main folder.",
                        es ? "Info" : "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                if (photoFiles.Count == 0)
                {
                    MessageBox.Show(
                        es ? "No hay fotos disponibles para agregar al álbum." : "No photos available to add to the album.",
                        es ? "Info" : "Info",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                var createAlbumView = new CreateAlbumView(photoFiles)
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                if (createAlbumView.ShowDialog() != true)
                    return;

                string albumName = createAlbumView.AlbumName;
                List<string> selectedPhotos = createAlbumView.SelectedPhotos;

                string folderName = $"Leuan's ToolKit - Album {albumName}";
                string albumPath = Path.Combine(screenshotsPath, folderName);

                if (Directory.Exists(albumPath))
                {
                    MessageBox.Show(
                        es ? "Ya existe un álbum con ese nombre." : "An album with that name already exists.",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Directory.CreateDirectory(albumPath);

                foreach (string photoPath in selectedPhotos)
                {
                    string fileName = Path.GetFileName(photoPath);
                    string destPath = Path.Combine(albumPath, fileName);
                    File.Move(photoPath, destPath);
                }

                await LoadPhotos();

                MessageBox.Show(
                    es ? $"¡Álbum '{albumName}' creado exitosamente con {selectedPhotos.Count} foto{(selectedPhotos.Count != 1 ? "s" : "")}!"
                       : $"Album '{albumName}' created successfully with {selectedPhotos.Count} photo{(selectedPhotos.Count != 1 ? "s" : "")}!",
                    es ? "Éxito" : "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                MessageBox.Show(
                    $"{(es ? "Error creando álbum: " : "Error creating album: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BackToRootButton_Click(object sender, RoutedEventArgs e)
        {
            currentAlbum = null;
            UpdateFolderPathText();
            await LoadPhotos();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPhotos();
        }

        private async void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = es ? "Selecciona la carpeta de capturas de Los Sims 4" : "Select The Sims 4 Screenshots folder",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                screenshotsPath = dialog.SelectedPath;
                currentAlbum = null;
                UpdateFolderPathText();
                await LoadPhotos();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClosePhotoViewer_Click(object sender, RoutedEventArgs e)
        {
            PhotoViewerOverlay.Visibility = Visibility.Collapsed;
            ViewerImage.Source = null;
        }

        private void PhotoViewerOverlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource == PhotoViewerOverlay)
            {
                ClosePhotoViewer_Click(sender, e);
            }
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
    }
}