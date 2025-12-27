using ModernDesign.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LeuanS4ToolKit.Core;

namespace ModernDesign.MVVM.View
{
    public partial class CreateAlbumView : Window
    {
        public string AlbumName { get; private set; }
        public List<string> SelectedPhotos { get; private set; }
        private List<string> allPhotos;
        private Dictionary<Border, string> photoCardMap = new Dictionary<Border, string>();
        private Dictionary<Border, CheckBox> cardCheckBoxMap = new Dictionary<Border, CheckBox>();

        public CreateAlbumView(List<string> photos)
        {
            InitializeComponent();
            allPhotos = photos;
            SelectedPhotos = new List<string>();
            ApplyLanguage();
            LoadPhotos();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            this.Title = es ? "Crear Álbum" : "Create Album";
            TitleText.Text = es ? "📁 Crear Álbum" : "📁 Create Album";
            SubtitleText.Text = es ? "Selecciona las fotos para agregar a tu nuevo álbum" : "Select photos to add to your new album";
            AlbumNameLabel.Text = es ? "Nombre del Álbum:" : "Album Name:";
            SelectAllCheckBox.Content = es ? "Seleccionar Todas" : "Select All";
            CancelButton.Content = es ? "Cancelar" : "Cancel";
            CreateButton.Content = es ? "Crear Álbum" : "Create Album";
            UpdateSelectedCount();
        }

        private void LoadPhotos()
        {
            PhotosWrapPanel.Children.Clear();
            photoCardMap.Clear();
            cardCheckBoxMap.Clear();

            foreach (string photoPath in allPhotos)
            {
                var card = CreatePhotoCard(photoPath);
                PhotosWrapPanel.Children.Add(card);
            }
        }

        private Border CreatePhotoCard(string photoPath)
        {
            Border card = new Border
            {
                Style = (Style)FindResource("PhotoCheckCard")
            };

            Grid cardGrid = new Grid();
            card.Child = cardGrid;

            // Imagen thumbnail
            Border imageBorder = new Border
            {
                Height = 130,
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

            // Info panel
            StackPanel infoPanel = new StackPanel
            {
                Margin = new Thickness(10, 140, 10, 10),
                VerticalAlignment = VerticalAlignment.Top
            };

            FileInfo fileInfo = new FileInfo(photoPath);

            TextBlock fileName = new TextBlock
            {
                Text = Path.GetFileNameWithoutExtension(fileInfo.Name),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 5)
            };

            infoPanel.Children.Add(fileName);

            // CheckBox
            var checkBox = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                IsChecked = false
            };
            checkBox.Checked += (s, e) => UpdateSelection();
            checkBox.Unchecked += (s, e) => UpdateSelection();
            infoPanel.Children.Add(checkBox);

            cardGrid.Children.Add(infoPanel);

            // Guardar referencias
            photoCardMap[card] = photoPath;
            cardCheckBoxMap[card] = checkBox;

            // Click en la tarjeta para toggle checkbox
            card.MouseLeftButtonUp += (s, e) =>
            {
                checkBox.IsChecked = !checkBox.IsChecked;
            };

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            };

            card.MouseLeave += (s, e) =>
            {
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            };

            return card;
        }

        private BitmapImage LoadThumbnail(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = 180;
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

        private void UpdateSelection()
        {
            SelectedPhotos.Clear();

            foreach (var kvp in cardCheckBoxMap)
            {
                if (kvp.Value.IsChecked == true)
                {
                    SelectedPhotos.Add(photoCardMap[kvp.Key]);
                }
            }

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
            SelectedCountText.Text = es
                ? $"{SelectedPhotos.Count} seleccionada{(SelectedPhotos.Count != 1 ? "s" : "")}"
                : $"{SelectedPhotos.Count} selected";
        }

        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in cardCheckBoxMap.Values)
            {
                checkBox.IsChecked = true;
            }
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in cardCheckBoxMap.Values)
            {
                checkBox.IsChecked = false;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            if (string.IsNullOrWhiteSpace(AlbumNameTextBox.Text))
            {
                MessageBox.Show(
                    es ? "Por favor ingresa un nombre para el álbum." : "Please enter a name for the album.",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (SelectedPhotos.Count == 0)
            {
                MessageBox.Show(
                    es ? "Por favor selecciona al menos una foto." : "Please select at least one photo.",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            AlbumName = AlbumNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}