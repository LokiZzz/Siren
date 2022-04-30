using Siren.Messaging;
using Siren.Services;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }

        public MainViewModel()
        {
            InitializeMessagingCenter();
            SceneManager = DependencyService.Get<SceneManager>();
            IntializeCollections();
        }

        private async Task GoToAddSetting() => await Shell.Current.GoToAsync(nameof(AddOrEditSettingPage));

        private void AddSetting(SceneManager manager)
        {
            Settings.Add(new SettingViewModel
            {
                Name = SceneManager.SettingToAdd.Name,
                ImagePath = SceneManager.SettingToAdd.ImagePath,
            });
        }

        private void SelectSetting(string name)
        {
            SelectedSetting = Settings.FirstOrDefault(x => x.Name.Equals(name));
            SceneManager.SelectedSetting = SelectedSetting;
        }

        private async Task GoToAddScene() => await Shell.Current.GoToAsync(nameof(AddOrEditScenePage));

        private void AddScene(SceneManager manager)
        {
            SelectedSetting.Scenes.Add(new SceneViewModel
            {
                Name = SceneManager.SceneToAdd.Name,
                ImagePath = SceneManager.SceneToAdd.ImagePath,
            });
        }

        private void SelectScene(string name)
        {
            SelectedScene = SelectedSetting.Scenes.FirstOrDefault(x => x.Name.Equals(name));

            foreach (SceneComponentViewModel element in SelectedSetting.Elements)
            {
                TrackSetupViewModel existingElement = SelectedScene.Elements
                    .FirstOrDefault(x => x.FilePath.Equals(element.FilePath));

                if (existingElement != null)
                {
                    if (!element.IsPlaying || element.Volume != existingElement.Volume)
                    {
                        element.SmoothPlay(targetVolume: existingElement.Volume);
                    }
                }
                else
                {
                    if (element.IsPlaying)
                    {
                        element.SmoothStop();
                    }
                }
            }

            OnPropertyChanged(nameof(IsScenePlaying));
        }

        private void SaveScene(string name)
        {
            var playingElements = SelectedSetting.Elements
                .Where(x => x.IsPlaying)
                .Select(x => new TrackSetupViewModel
                {
                    FilePath = x.FilePath,
                    Volume = x.Volume,
                })
                .ToList();

            SelectedScene.Elements = new ObservableCollection<TrackSetupViewModel>(playingElements);

            SaveCurrentBundle();
        }

        private async Task AddElements()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach(FileResult element in result)
            {
                if (!SelectedSetting.Elements.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { Loop = true };
                    await player.Load(element.FullPath);
                    SelectedSetting.Elements.Add(player);
                }
            }
        }

        private void DeleteElement(string name)
        {
            SceneComponentViewModel element = SelectedSetting.Elements.FirstOrDefault(x => x.Name.Equals(name));
            SelectedSetting.Elements.Remove(element);
            element.Dispose();
        }

        private async Task AddEffects()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            foreach (FileResult element in result)
            {
                if (!SelectedSetting.Effects.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel();
                    await player.Load(element.FullPath);
                    SelectedSetting.Effects.Add(player);
                }
            }
        }

        private void GlobalPlayStop()
        {
            if(IsScenePlaying)
            {
                SelectedSetting.Elements.ForEach(x => x.SmoothStop());
            }
            else
            {
                SelectScene(SelectedScene.Name);
            }
            OnPropertyChanged(nameof(IsScenePlaying));
        }

        private void SaveCurrentBundle()
        {
            SceneManager.SaveCurrentBundle(Settings);
        }

        private void IntializeCollections()
        {
            Settings = SceneManager.GetCurrentBundle();

            Settings.CollectionChanged += BindNewSettingEvents;
            Settings.CollectionChanged += SaveCurrentBundle;
            Settings.ForEach(x => x.SettingChanged += SaveCurrentBundle);
        }

        private void BindNewSettingEvents(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach(SettingViewModel item in e.NewItems)
                {
                    item.SettingChanged += SaveCurrentBundle;
                }
            }
        }

        private void SaveCurrentBundle(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveCurrentBundle();
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneComponentViewModel>(this, Messages.ElementPlayingStatusChanged, vm => OnPropertyChanged(nameof(IsScenePlaying)));
            MessagingCenter.Subscribe<SceneManager>(this, Messages.SettingAdded, AddSetting);
            MessagingCenter.Subscribe<SceneManager>(this, Messages.SceneAdded, AddScene);
        }

        public Command AddSettingCommand { get => new Command(async () => await GoToAddSetting()); }
        public Command AddSceneCommand { get => new Command(async () => await GoToAddScene()); }
        public Command AddElementsCommand { get => new Command(async () => await AddElements()); }
        public Command AddEffectsCommand { get => new Command(async () => await AddEffects()); }
        public Command SelectSettingCommand { get => new Command<string>(SelectSetting); }
        public Command SelectSceneCommand { get => new Command<string>(SelectScene); }
        public Command DeleteElementCommand { get => new Command<string>(DeleteElement); }
        public Command SaveSceneCommand { get => new Command<string>(SaveScene); }
        public Command GlobalPlayCommand { get => new Command(GlobalPlayStop); }

        public ObservableCollection<SettingViewModel> Settings { get; set; } = new ObservableCollection<SettingViewModel>();

        private SettingViewModel _selectedSetting;
        public SettingViewModel SelectedSetting
        {
            get => _selectedSetting;
            set
            {
                SetProperty(ref _selectedSetting, value);
                foreach (SettingViewModel item in Settings)
                {
                    item.IsSelected = false;
                }
                SelectedSetting.IsSelected = true;
            }
        }

        private SceneViewModel _selectedScene;
        public SceneViewModel SelectedScene
        {
            get => _selectedScene;
            set
            {
                SetProperty(ref _selectedScene, value);
                foreach (SceneViewModel item in SelectedSetting.Scenes)
                {
                    item.IsSelected = false;
                }
                SelectedScene.IsSelected = true;
            }
        }

        public bool IsScenePlaying => SelectedSetting?.Elements.Any(x => x.IsPlaying) == true;
    }
}