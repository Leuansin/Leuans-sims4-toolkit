using ModernDesign.Core;
using ModernDesign.MVVM.View;
using System.Windows.Input;
using LeuanS4ToolKit.Core;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.ViewModel
{
    public class MainViewModel : LocalizedObservableObject
    {
        private object _currentView;

        [LocalizeKey("MainWindowSettingsText")]
        public string SettingsText => _lm.Get(GetPropertyKey());

        [LocalizeKey("MainWindowHomeText")]
        public string HomeText => _lm.Get(GetPropertyKey());
        
        [LocalizeKey("MainWindowDiscoveryText")]
        public string DiscoveryText => _lm.Get(GetPropertyKey());
        
         [LocalizeKey("MainWindowFpsBoosterText")]
         public string FpsBoosterText => _lm.Get(GetPropertyKey());
        
         [LocalizeKey("MainWindowSocialText")]
         public string SocialText => _lm.Get(GetPropertyKey());

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

            HomeViewCommand = new RelayCommand(o => { CurrentView = new HomeView(); });

            DiscoveryViewCommand = new RelayCommand(o => { CurrentView = new DiscoveryView(); });

            FPSBoosterViewCommand = new RelayCommand(o => { CurrentView = new FPSBoosterView(); });

            SocialViewCommand = new RelayCommand(o => { CurrentView = new SocialView(); });

            SettingsViewCommand = new RelayCommand(o => { CurrentView = new SettingsView(); });
        }
    }
}