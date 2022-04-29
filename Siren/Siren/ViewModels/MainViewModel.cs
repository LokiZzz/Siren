using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
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
            //Delete this:
            for (int i = 0; i < 4; i++)
            {
                setting.Scenes.Add(new Scene { Name = $"Really fun scene in the Tavern #{i}" });
            }
            var setting2 = new Setting { Name = "Ship" };
            //Delete this:
            for (int i = 0; i < 4; i++)
            {
                setting2.Scenes.Add(new Scene { Name = $"Action pirate style scene #{i}" });
            }

            Settings.Add(setting);
            Settings.Add(setting2);
            SelectedSetting = setting;
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
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach(FileResult element in result)
            {
                PlayerViewModel player = new PlayerViewModel { Loop = true };
                await player.Load(element.FullPath);
                SelectedSetting.Elements.Add(player);
            }
        }

        private async Task AddEffects()
        {
            
        }

        private void SelectSetting(string name)
        {
            SelectedSetting = Settings.FirstOrDefault(x => x.Name.Equals(name));
        }

        private void SelectScene(string name)
        {
            SelectedScene = SelectedSetting.Scenes.FirstOrDefault(x => x.Name.Equals(name));

            foreach(PlayerViewModel element in SelectedSetting.Elements)
            {
                TrackSetup existingElement = SelectedScene.Elements
                    .FirstOrDefault(x => x.Path.Equals(element.FilePath));

                if (existingElement != null)
                {
                    if(!element.IsPlaying || element.Volume != existingElement.Volume)
                    {
                        element.SmoothPlay(targetVolume: existingElement.Volume);
                    }
                    //element.Volume = existingElement.Volume;
                }
                else
                {
                    if (element.IsPlaying)
                    {
                        element.SmoothStop();
                    }
                }
            }
        }

        private void SaveScene(string name)
        {
            var playingElements = SelectedSetting.Elements
                .Where(x => x.IsPlaying)
                .Select(x => new TrackSetup
                {
                    Path = x.FilePath,
                    Volume = x.Volume,
                })
                .ToList();

            SelectedScene.Elements = new ObservableCollection<TrackSetup>(playingElements);
        }

        private void DeleteElement(string name)
        {
            PlayerViewModel element = SelectedSetting.Elements.FirstOrDefault(x => x.Name.Equals(name));
            SelectedSetting.Elements.Remove(element);
            element.Dispose();
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
            SelectSettingCommand = new Command<string>(SelectSetting);
            SelectSceneCommand = new Command<string>(SelectScene);
            DeleteElementCommand = new Command<string>(DeleteElement);
            SaveSceneCommand = new Command<string>(SaveScene);
        }

        public Command AddSettingCommand { get; set; }
        public Command AddSceneCommand { get; set; }
        public Command AddElementsCommand { get; set; }
        public Command AddEffectsCommand { get; set; }
        public Command SelectSettingCommand { get; set; }
        public Command SelectSceneCommand { get; set; }
        public Command DeleteElementCommand { get; set; }
        public Command SaveSceneCommand { get; set; }

        public ObservableCollection<Setting> Settings { get; set; } = new ObservableCollection<Setting>();

        private Setting _selectedSetting;
        public Setting SelectedSetting
        {
            get => _selectedSetting;
            set
            {
                SetProperty(ref _selectedSetting, value);
                foreach (Setting item in Settings)
                {
                    item.IsSelected = false;
                }
                SelectedSetting.IsSelected = true;
            }
        }

        private Scene _selectedScene;
        public Scene SelectedScene
        {
            get => _selectedScene;
            set
            {
                SetProperty(ref _selectedScene, value);
                foreach (Scene item in SelectedSetting.Scenes)
                {
                    item.IsSelected = false;
                }
                SelectedScene.IsSelected = true;
            }
        }
    }
}