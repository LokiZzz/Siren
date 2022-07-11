using Siren.Messaging;
using Siren.Models;
using Siren.Services;
using Siren.ViewModels.Players;
using Siren.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
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
            Task.Run(async () => { await IntializeCollections(); }).Wait();
        }

        #region Settings
        private ObservableCollection<SettingViewModel> _settings;
        public ObservableCollection<SettingViewModel> Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        private SettingViewModel _selectedSetting;
        public SettingViewModel SelectedSetting
        {
            get => _selectedSetting;
            set => SetProperty(ref _selectedSetting, value);
        }

        private async Task GoToAddSetting()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Add}&component={EComponentType.Setting}"
            );
        }

        private async Task AddSetting(SceneManager manager)
        {
            SettingViewModel newSetting = new SettingViewModel
            {
                Name = SceneManager.SettingToAdd.Name,
                ImagePath = SceneManager.SettingToAdd.ImagePath,
            };
            Settings.Add(newSetting);
            await SelectSetting(newSetting);
        }

        private async Task GoToEditSetting(SettingViewModel setting)
        {
            SceneManager.IllustratedCardToEdit = setting;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Setting}"
            );
        }

        private Task SelectSetting(SettingViewModel setting)
        {
            return Task.Run(() =>
            {
                IsBusy = true;

                SelectedSetting = setting;

                if (SelectedSetting != null)
                {
                    foreach (SettingViewModel item in Settings)
                    {
                        item.IsSelected = false;
                    }

                    SelectedSetting.IsSelected = true;
                    MusicPlayer.Tracks = SelectedSetting.Music;
                }

                ShowSettingEditTools = SelectedSetting != null;
                OnPropertyChanged(nameof(CurrentElementsCountString));
                OnPropertyChanged(nameof(CurrentEffectsCountString));


                IsBusy = false;
            });
        }

        private async Task DeleteSetting(SettingViewModel setting)
        {
            bool wantToDelete = await Application.Current.MainPage.DisplayAlert(
                "Warning",
                $"Are you really want to delete «{setting.Name}» setting?",
                "Yes", "No"
            );

            if (wantToDelete)
            {
                RemoveEvents();
                setting.Scenes.Clear();
                setting.Elements.ForEach(x => x.Dispose());
                setting.Elements.Clear();
                setting.Effects.ForEach(x => x.Dispose());
                setting.Effects.Clear();
                AddEvents();
                Settings.Remove(setting);

                if (!Settings.Any())
                {
                    await SelectScene(null);
                    await SelectSetting(null);
                }
            }
        }

        private bool _showSettingEditTools;
        public bool ShowSettingEditTools
        {
            get => _showSettingEditTools;
            set => SetProperty(ref _showSettingEditTools, value);
        }
        #endregion

        #region Scene
        private SceneViewModel _selectedScene;
        public SceneViewModel SelectedScene
        {
            get => _selectedScene;
            set
            {
                SetProperty(ref _selectedScene, value);

                if (SelectedSetting != null)
                {
                    foreach (SceneViewModel item in SelectedSetting.Scenes)
                    {
                        item.IsSelected = false;
                    }
                    if (SelectedScene != null)
                    {
                        SelectedScene.IsSelected = true;
                    }
                }
            }
        }

        private async Task GoToAddScene()
        {
            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Add}&component={EComponentType.Scene}"
            );
        }

        private void AddScene(SceneManager manager)
        {
            SceneViewModel newScene = new SceneViewModel
            {
                Name = SceneManager.SceneToAdd.Name,
                ImagePath = SceneManager.SceneToAdd.ImagePath,
            };
            SelectedSetting.Scenes.Add(newScene);
        }

        private async Task GoToEditScene(SceneViewModel scene)
        {
            SceneManager.IllustratedCardToEdit = scene;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Scene}"
            );
        }

        private async Task SelectScene(SceneViewModel scene)
        {
            Settings.SelectMany(x => x.Scenes).ForEach(x => x.IsSelected = false);
            SelectedScene = scene;

            foreach (SceneComponentViewModel element in Settings.SelectMany(x => x.Elements))
            {
                TrackSetupViewModel existingElement = SelectedScene?.Elements?
                    .FirstOrDefault(x => x.FilePath.Equals(element.FilePath));

                if (existingElement != null)
                {
                    if (!element.IsPlaying || element.Volume != existingElement.Volume)
                    {
                        await element.SmoothPlay(targetVolume: existingElement.Volume);
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

            await MusicPlayer.AdjustPlayer(
                SelectedScene.IsMusicEnabled, 
                SelectedScene.IsMusicShuffled, 
                SelectedScene.MusicVolume
            );

            OnPropertyChanged(nameof(IsSomethingPlaying));
            OnPropertyChanged(nameof(CurrentSceneText));
            Settings.ForEach(x => x.UpdateHasSelectedScene());

            ShowSceneEditTools = SelectedScene != null;

            GlobalPlayActivityIndicatorIsVisible = false;
        }

        private async Task DeleteScene(SceneViewModel scene)
        {
            bool wantToDelete = await App.Current.MainPage.DisplayAlert(
                "Warning",
                $"Are you really want to delete «{scene.Name}» scene?",
                "Yes", "No"
            );

            if (wantToDelete)
            {
                SelectedSetting.Scenes.Remove(scene);
            }
        }

        private async Task SaveScene(SceneViewModel scene)
        {
            List<TrackSetupViewModel> playingElements = SelectedSetting.Elements
                .Where(x => x.IsPlaying)
                .Select(x => new TrackSetupViewModel
                {
                    FilePath = x.FilePath,
                    Volume = x.Volume,
                })
                .ToList();

            scene.Elements = new ObservableCollection<TrackSetupViewModel>(playingElements);
            scene.IsMusicEnabled = MusicPlayer.IsOn;
            scene.IsMusicShuffled = MusicPlayer.Shuffle;
            scene.MusicVolume = MusicPlayer.Volume;

            await SaveCurrentEnvironment();
        }

        private bool _showSceneEditTools;
        public bool ShowSceneEditTools
        {
            get => _showSceneEditTools;
            set => SetProperty(ref _showSceneEditTools, value);
        }
        #endregion

        #region Elements
        private int _maxElementsCount = 30;
        private async Task AddElements()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            if (result.Count() + SelectedSetting.Elements.Count() > _maxElementsCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (FileResult element in result)
            {
                if (!SelectedSetting.Elements.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { Loop = true, FilePath = element.FullPath };
                    SelectedSetting.Elements.Add(player);
                }
            }

            OnPropertyChanged(nameof(CurrentElementsCountString));
        }

        private async Task GoToEditElement(SceneComponentViewModel element)
        {
            SceneManager.ComponentToEdit = element;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Element}"
            );
        }

        private void DeleteElement(SceneComponentViewModel component)
        {
            SelectedSetting.Elements.Remove(component);
            component.Dispose();
            OnPropertyChanged(nameof(CurrentElementsCountString));
        }

        public string CurrentElementsCountString
        {
            get
            {
                int current = SelectedSetting?.Elements?.Count ?? 0;
                int max = _maxElementsCount;

                OnPropertyChanged(nameof(FreeElementsSpace));

                return $"{current}/{max}";
            }
        }

        public double FreeElementsSpace => ((double?)SelectedSetting?.Elements?.Count ?? 0) / _maxElementsCount;
        #endregion

        #region Effects
        private int _maxEffectsCount = 30;
        private async Task AddEffects()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            if (result.Count() + SelectedSetting.Effects.Count() > _maxEffectsCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (FileResult element in result)
            {
                if (!SelectedSetting.Effects.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { FilePath = element.FullPath };
                    SelectedSetting.Effects.Add(player);
                }
            }

            OnPropertyChanged(nameof(CurrentEffectsCountString));
        }

        private async Task GoToEditEffect(SceneComponentViewModel effect)
        {
            SceneManager.ComponentToEdit = effect;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.Effect}"
            );
        }

        private void DeleteEffect(SceneComponentViewModel component)
        {
            SelectedSetting.Effects.Remove(component);
            component.Dispose();
            OnPropertyChanged(nameof(CurrentEffectsCountString));
        }

        public string CurrentEffectsCountString
        {
            get
            {
                int current = SelectedSetting?.Effects?.Count ?? 0;
                int max = _maxElementsCount;

                OnPropertyChanged(nameof(FreeEffectsSpace));

                return $"{current}/{max}";
            }
        }

        public double FreeEffectsSpace => ((double?)SelectedSetting?.Effects?.Count ?? 0) / _maxElementsCount;
        #endregion

        #region Music
        public SceneMusicPlayerViewModel MusicPlayer { get; set; } = new SceneMusicPlayerViewModel();

        private int _maxMusicTracksCount = 100;
        private async Task AddMusic()
        {
            IEnumerable<FileResult> result = await FilePicker.PickMultipleAsync(PickOptions.Default);

            if (result.Count() + SelectedSetting.Music.Count() > _maxMusicTracksCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (FileResult element in result)
            {
                if (!SelectedSetting.Music.Any(x => x.FilePath.Equals(element.FullPath)))
                {
                    SceneComponentViewModel track = new SceneComponentViewModel { FilePath = element.FullPath };
                    SelectedSetting.Music.Add(track);
                }
            }

            OnPropertyChanged(nameof(CurrentMusicTracksCountString));
        }

        private async Task GoToEditMusicTrack(SceneComponentViewModel musicTrack)
        {
            SceneManager.ComponentToEdit = musicTrack;

            await Shell.Current.GoToAsync(
                $"{nameof(AddOrEditComponentPage)}?intent={EAddOrEditIntent.Edit}&component={EComponentType.MusicTrack}"
            );
        }

        private void DeleteMusicTrack(SceneComponentViewModel component)
        {
            SelectedSetting.Music.Remove(component);
            component.Dispose();
            OnPropertyChanged(nameof(CurrentMusicTracksCountString));
        }

        public double FreeMusicTracksSpace => ((double?)SelectedSetting?.Music?.Count ?? 0) / _maxMusicTracksCount;
        public string CurrentMusicTracksCountString
        {
            get
            {
                int current = SelectedSetting?.Music?.Count ?? 0;
                int max = _maxMusicTracksCount;

                OnPropertyChanged(nameof(FreeMusicTracksSpace));

                return $"{current}/{max}";
            }
        }

        private async Task PlayMusic()
        {
            if(MusicPlayer.IsMusicPlaying)
            {
                MusicPlayer.StopMusic();
            }
            else
            {
                await MusicPlayer.PlayMusic();
            }

            OnPropertyChanged(nameof(IsSomethingPlaying));
        }
        #endregion

        #region Environment
        private async void SaveCurrentEnvironment(object sender, NotifyCollectionChangedEventArgs e) => await SaveCurrentEnvironment();

        private async Task SaveCurrentEnvironment()
        {
            await SceneManager.SaveCurrentEnvironment(Settings);
        }

        private async Task IntializeCollections()
        {
            Settings = await SceneManager.GetVMFromCurrentEnvironment();
            await SelectSetting(Settings.FirstOrDefault());
            AddEvents();

            if (!Settings.Any())
            {
                await SelectScene(null);
                await SelectSetting(null);
            }
        }
        #endregion

        #region Events
        private void AddEvents()
        {
            Settings.CollectionChanged += BindNewSettingEvents;
            Settings.CollectionChanged += SaveCurrentEnvironment;
            Settings.ForEach(x => x.SettingChanged += SaveCurrentEnvironment);
        }

        private void RemoveEvents()
        {
            Settings.CollectionChanged -= BindNewSettingEvents;
            Settings.CollectionChanged -= SaveCurrentEnvironment;
            Settings.ForEach(x => x.SettingChanged -= SaveCurrentEnvironment);
        }

        private void BindNewSettingEvents(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (SettingViewModel item in e.NewItems)
                {
                    item.SettingChanged += SaveCurrentEnvironment;
                }
            }
        }

        private void InitializeMessagingCenter()
        {
            MessagingCenter.Subscribe<SceneComponentViewModel>(this, Messages.ElementPlayingStatusChanged, vm => OnPropertyChanged(nameof(IsSomethingPlaying)));
            MessagingCenter.Subscribe<SceneMusicPlayerViewModel>(this, Messages.MusicTrackPlayingStatusChanged, vm => OnPropertyChanged(nameof(IsSomethingPlaying)));

            MessagingCenter.Subscribe<SceneManager>(this, Messages.SettingAdded, async (setting) => await AddSetting(setting));
            MessagingCenter.Subscribe<SceneManager>(this, Messages.SceneAdded, AddScene);
            MessagingCenter.Subscribe<AddOrEditComponentViewModel>(this, Messages.IllustratedCardEdited, async (manager) => await SaveCurrentEnvironment());
            MessagingCenter.Subscribe<BundlePageViewModel>(this, Messages.NeedToUpdateEnvironment, async (manager) => await IntializeCollections());
        }
        #endregion

        #region Commands
        public Command AddSettingCommand { get => new Command(async () => await GoToAddSetting()); }
        public Command EditSettingCommand { get => new Command<SettingViewModel>(async (setting) => await GoToEditSetting(setting)); }
        public Command SelectSettingCommand { get => new Command<SettingViewModel>(async (setting) => await SelectSetting(setting)); }
        public Command DeleteSettingCommand { get => new Command<SettingViewModel>(async (scetting) => await DeleteSetting(scetting)); }
        public Command AddSceneCommand { get => new Command(async () => await GoToAddScene()); }
        public Command SaveSceneCommand { get => new Command<SceneViewModel>(async (scene) => await SaveScene(scene)); }
        public Command EditSceneCommand { get => new Command<SceneViewModel>(async (scene) => await GoToEditScene(scene)); }
        public Command SelectSceneCommand { get => new Command<SceneViewModel>(async (scene) => await SelectScene(scene)); }
        public Command DeleteSceneCommand { get => new Command<SceneViewModel>(async (scene) => await DeleteScene(scene)); }
        public Command AddElementsCommand { get => new Command(async () => await AddElements()); }
        public Command EditElementCommand { get => new Command<SceneComponentViewModel>(async (element) => await GoToEditElement(element)); }
        public Command DeleteElementCommand { get => new Command<SceneComponentViewModel>(DeleteElement); }
        public Command AddEffectsCommand { get => new Command(async () => await AddEffects()); }
        public Command EditEffectCommand { get => new Command<SceneComponentViewModel>(async (effect) => await GoToEditEffect(effect)); }
        public Command DeleteEffectCommand { get => new Command<SceneComponentViewModel>(DeleteEffect); }
        public Command AddMusicCommand { get => new Command(async () => await AddMusic()); }
        public Command EditMusicCommand { get => new Command<SceneComponentViewModel>(async (music) => await GoToEditMusicTrack(music)); }
        public Command DeleteMusicCommand { get => new Command<SceneComponentViewModel>(DeleteMusicTrack); }
        public Command PlayMusicCommand { get => new Command(async () => await PlayMusic()); }
        public Command GlobalPlayCommand { get => new Command(async () => await GlobalPlayStop()); }
        #endregion

        #region Global Play/Stop
        private bool _globalPlayActivityIndicatorIsVisible = false;
        public bool GlobalPlayActivityIndicatorIsVisible
        {
            get => _globalPlayActivityIndicatorIsVisible;
            set => SetProperty(ref _globalPlayActivityIndicatorIsVisible, value);
        }

        private async Task GlobalPlayStop()
        {
            if (IsSomethingPlaying)
            {
                GlobalPlayActivityIndicatorIsVisible = true;

                Settings.SelectMany(x => x.Elements)
                    .Where(x => x.IsPlaying)
                    .ForEach(x => x.SmoothStop());

                MusicPlayer.StopMusic();
            }
            else
            {
                if (SelectedScene != null)
                {
                    await SelectScene(SelectedScene);
                }
            }
            OnPropertyChanged(nameof(IsSomethingPlaying));
        }

        public bool IsSomethingPlaying
        {
            get
            {
                bool isScenePlaying = Settings.SelectMany(x => x.Elements).Any(x => x.IsPlaying) || MusicPlayer.IsPlaying;
                
                if(GlobalPlayActivityIndicatorIsVisible && !isScenePlaying)
                {
                    GlobalPlayActivityIndicatorIsVisible = false;
                }

                return isScenePlaying;
            }
        }

        public string CurrentSceneText
        {
            get
            {
                string current;

                if(SelectedSetting == null || SelectedScene == null)
                {
                    current = "still does not selected...";
                }
                else
                {
                    string setting = Settings.FirstOrDefault(x => x.Scenes.Contains(SelectedScene)).Name;
                    current = $"{setting} — { SelectedScene.Name}";
                }

                return $"Current scene: {current}";
            }
        }
        #endregion

        #region Private utility
        private async Task AlertTooManyFiles()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Too many files!",
                $"You have selected too many files, split them into groups and put them in different settings.",
                "Ok :("
            );
        }
        #endregion
    }
}