using ModernDesign.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LeuanS4ToolKit.Core;
using WpfMessageBox = System.Windows.MessageBox;
using WpfDragEventArgs = System.Windows.DragEventArgs;
using WpfDragDropEffects = System.Windows.DragDropEffects;

namespace ModernDesign.MVVM.View
{
    public partial class OrganizeMusicView : Window
    {
        private string customMusicPath;
        private List<MusicFile> musicFiles = new List<MusicFile>();
        private List<MusicFolder> musicFolders = new List<MusicFolder>();
        private MusicFile draggedFile = null;

        public class MusicFile
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string CurrentFolder { get; set; }
        }

        public class MusicFolder
        {
            public string FolderName { get; set; }
            public string FolderPath { get; set; }
            public int FileCount { get; set; }
        }

        public OrganizeMusicView(string customMusicPath)
        {
            InitializeComponent();
            this.customMusicPath = customMusicPath;
            ApplyLanguage();
            LoadMusicData();
        }

        private void ApplyLanguage()
        {
            bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;

            this.Title = es ? "Organizar Música" : "Organize Music";
            TitleText.Text = es ? "📂 Organizar Música" : "📂 Organize Music";
            SubtitleText.Text = es ? "Arrastra canciones a las carpetas para organizarlas" : "Drag songs to folders to organize them";
        }

        private void LoadMusicData()
        {
            try
            {
                // Cargar carpetas
                var folders = Directory.GetDirectories(customMusicPath);
                foreach (var folder in folders)
                {
                    string folderName = Path.GetFileName(folder);
                    int fileCount = Directory.GetFiles(folder, "*.mp3").Length;

                    musicFolders.Add(new MusicFolder
                    {
                        FolderName = folderName,
                        FolderPath = folder,
                        FileCount = fileCount
                    });
                }

                // Cargar archivos MP3
                var allMp3Files = Directory.GetFiles(customMusicPath, "*.mp3", SearchOption.AllDirectories);
                foreach (var mp3 in allMp3Files)
                {
                    string fileName = Path.GetFileName(mp3);
                    string currentFolder = Path.GetFileName(Path.GetDirectoryName(mp3));

                    musicFiles.Add(new MusicFile
                    {
                        FileName = fileName,
                        FilePath = mp3,
                        CurrentFolder = currentFolder
                    });
                }

                // Mostrar en UI
                DisplayFolders();
                DisplayFiles();
            }
            catch (Exception ex)
            {
                bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                WpfMessageBox.Show(
                    $"{(es ? "Error cargando datos: " : "Error loading data: ")}{ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DisplayFolders()
        {
            FoldersPanel.Children.Clear();

            foreach (var folder in musicFolders)
            {
                Border folderCard = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                    CornerRadius = new CornerRadius(12),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(15),
                    AllowDrop = true,
                    Tag = folder
                };

                folderCard.Drop += FolderCard_Drop;
                folderCard.DragEnter += FolderCard_DragEnter;
                folderCard.DragLeave += FolderCard_DragLeave;

                StackPanel stack = new StackPanel();

                TextBlock folderName = new TextBlock
                {
                    Text = $"📁 {folder.FolderName}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                TextBlock fileCount = new TextBlock
                {
                    Text = $"{folder.FileCount} archivos",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontSize = 11
                };

                stack.Children.Add(folderName);
                stack.Children.Add(fileCount);
                folderCard.Child = stack;

                FoldersPanel.Children.Add(folderCard);
            }
        }

        private void DisplayFiles()
        {
            FilesPanel.Children.Clear();

            foreach (var file in musicFiles)
            {
                Border fileCard = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748")),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 0, 0, 8),
                    Padding = new Thickness(12),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = file
                };

                fileCard.MouseDown += FileCard_MouseDown;

                StackPanel stack = new StackPanel();

                TextBlock fileName = new TextBlock
                {
                    Text = $"🎵 {file.FileName}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Margin = new Thickness(0, 0, 0, 3)
                };

                TextBlock currentFolder = new TextBlock
                {
                    Text = $"📂 {file.CurrentFolder}",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontSize = 10
                };

                stack.Children.Add(fileName);
                stack.Children.Add(currentFolder);
                fileCard.Child = stack;

                FilesPanel.Children.Add(fileCard);
            }
        }

        private void FileCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Border card = sender as Border;
                draggedFile = card.Tag as MusicFile;

                if (draggedFile != null)
                {
                    DragDrop.DoDragDrop(card, draggedFile, WpfDragDropEffects.Move);
                }
            }
        }

        private void FolderCard_DragEnter(object sender, WpfDragEventArgs e)
        {
            Border card = sender as Border;
            card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F46E5"));
        }

        private void FolderCard_DragLeave(object sender, WpfDragEventArgs e)
        {
            Border card = sender as Border;
            card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
        }

        private void FolderCard_Drop(object sender, WpfDragEventArgs e)
        {
            Border card = sender as Border;
            card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));

            MusicFolder targetFolder = card.Tag as MusicFolder;

            if (draggedFile != null && targetFolder != null)
            {
                try
                {
                    string newPath = Path.Combine(targetFolder.FolderPath, draggedFile.FileName);

                    if (File.Exists(newPath))
                    {
                        bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                        WpfMessageBox.Show(
                            es ? "Ya existe un archivo con ese nombre en la carpeta destino." : "A file with that name already exists in the destination folder.",
                            es ? "Error" : "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    File.Move(draggedFile.FilePath, newPath);

                    // Mover también el .webp si existe
                    string oldWebp = Path.ChangeExtension(draggedFile.FilePath, ".webp");
                    if (File.Exists(oldWebp))
                    {
                        string newWebp = Path.ChangeExtension(newPath, ".webp");
                        File.Move(oldWebp, newWebp);
                    }

                    // Actualizar datos
                    musicFiles.Clear();
                    musicFolders.Clear();
                    LoadMusicData();

                    draggedFile = null;
                }
                catch (Exception ex)
                {
                    bool es = ServiceLocator.Get<ILanguageManager>().IsSpanish;
                    WpfMessageBox.Show(
                        $"{(es ? "Error moviendo archivo: " : "Error moving file: ")}{ex.Message}",
                        es ? "Error" : "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}