using ModernDesign.Core;
using ModernDesign.MVVM.View;
using System.Windows.Input;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private object _currentView;

        public string SettingsText => LanguageManager.Get("MainWindowSettingsRadio");
        
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand HomeViewCommand { get; set; }
        public ICommand DiscoveryViewCommand { get; set; }
        public ICommand FPSBoosterViewCommand { get; set; }
        public ICommand SocialViewCommand { get; set; }
        public ICommand SettingsViewCommand { get; set; }

        public MainViewModel()
        {
            CurrentView = new HomeView();

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = new HomeView();
            });

            DiscoveryViewCommand = new RelayCommand(o =>
            {
                CurrentView = new DiscoveryView();
            });

            FPSBoosterViewCommand = new RelayCommand(o =>
            {
                CurrentView = new FPSBoosterView();
            });

            SocialViewCommand = new RelayCommand(o =>
            {
                CurrentView = new SocialView();
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = new SettingsView();
            });
        }
    }
}