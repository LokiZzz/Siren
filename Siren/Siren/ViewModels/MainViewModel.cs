using Siren.Models;
using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public MainViewModel()
        {
            InitializeMessagingCenter();
            InitializeCommands();
            InitializeAudioEnvironment();

            SceneManager = DependencyService.Get<SceneManager>();

            var setting = new Setting { Name = "Tavern" };
            Settings.Add(setting);
            SelectedSetting = setting;
            //Delete this:
            for (int i = 0; i < 7; i++)
            {
                SelectedSetting.Scenes.Add(new Scene { Name = $"Scene of the selected setting here {i}" });
            }
            var setting2 = new Setting { Name = "Ship" };
            Settings.Add(setting2);
            SelectedSetting = setting2;
            //Delete this:
            for (int i = 0; i < 7; i++)
            {
                SelectedSetting.Scenes.Add(new Scene { Name = $"Scene of the selected setting here {i}" });
            }
        }

        private void InitializeAudioEnvironment()
        {
            if (SceneManager?.Settings != null)
            {
                Settings = new ObservableCollection<Setting>(SceneManager.Settings);
                SelectedSetting = Settings.Any() ? Settings.First() : null;
            }
        }

        private void UpdateSettings(SceneManager manager)
        {
            foreach (Setting setting in manager.Settings)
            {
                if (!Settings.Any(x => x.Name == setting.Name))
                {
                    Settings.Add(setting);
                    SelectedSetting = setting;
                }
            }
        }

        private async Task AddSetting()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditSettingPage));
        }

        private async Task AddScene()
        {
            await Shell.Current.GoToAsync(nameof(AddOrEditScenePage));
        }

        private async Task AddElements()
        {
            //List<Task> loadingWork = new List<Task>();
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach(FileResult element in result)
            {
                PlayerViewModel player = new PlayerViewModel();
                //loadingWork.Add(player.Load(element.FullPath));
                await player.Load(element.FullPath);
                Elements.Add(player);
            }

            //Task.WaitAll(loadingWork.ToArray());
        }

        private async Task AddEffects()
        {
            
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneManager>(this, SceneManagerMessages.SettingAdded, UpdateSettings);
        }

        private void InitializeCommands()
        {
            AddSettingCommand = new Command(async () => await AddSetting());
            AddSceneCommand = new Command(async () => await AddScene());
            AddElementsCommand = new Command(async () => await AddElements());
            AddEffectsCommand = new Command(async () => await AddEffects());
        }

        public Command AddSettingCommand { get; set; }
        public Command AddSceneCommand { get; set; }
        public Command AddElementsCommand { get; set; }
        public Command AddEffectsCommand { get; set; }

        private Setting _selectedSetting;
        public Setting SelectedSetting
        {
            get
            {
                return SceneManager.SelectedSetting;
            }
            set
            {
                SceneManager.SelectedSetting = value;
                SetProperty(ref _selectedSetting, value);
            }
        }

        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        private Scene _selectedScene;
        public Scene SelectedScene
        {
            get => _selectedScene;
            set => SetProperty(ref _selectedScene, value);
        }

        public ObservableCollection<PlayerViewModel> Elements { get; set; } = new ObservableCollection<PlayerViewModel>();
    }
}