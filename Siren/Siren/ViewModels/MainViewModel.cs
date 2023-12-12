using Siren.Messaging;
using Siren.Services;
using Siren.Utility;
using Siren.ViewModels.Help;
using Siren.ViewModels.Players;
using Siren.Views;
using Siren.Views.Help;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Siren.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private SceneManager SceneManager { get; }
        private IFileManager FileManager { get; }

        public MainViewModel()
        {
            InitializeMessagingCenter();
            SceneManager = DependencyService.Get<SceneManager>();
            FileManager = DependencyService.Get<IFileManager>();
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
            IsBusy = true;

            SelectedSetting = setting;

            if (SelectedSetting != null)
            {
                foreach (SettingViewModel item in Settings)
                {
                    item.IsSelected = false;
                }

                SelectedSetting.IsSelected = true;
                SelectedSetting.MusicPlayer.Tracks = SelectedSetting.Music;
            }

            ShowSettingEditTools = SelectedSetting != null;
            OnPropertyChanged(nameof(CurrentElementsCountString));
            OnPropertyChanged(nameof(CurrentEffectsCountString));
            OnPropertyChanged(nameof(CurrentMusicTracksCountString));
            OnPropertyChanged(nameof(SelectedSetting.Music));
            OnPropertyChanged(nameof(SelectedSetting.MusicPlayer.IsOn));
            OnPropertyChanged(nameof(SelectedSetting.MusicPlayer.Shuffle));

            IsBusy = false;

            return Task.CompletedTask;
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
                    await SelectSetting(null);
                    await SelectScene(null);
                }
                else
                {
                    if(SelectedSetting == setting)
                    {
                        await SelectSetting(Settings.First());
                    }
                }

                OnPropertyChanged(nameof(CurrentElementsCountString));
                OnPropertyChanged(nameof(CurrentEffectsCountString));
                OnPropertyChanged(nameof(CurrentMusicTracksCountString));
                OnPropertyChanged(nameof(SelectedSetting.Music));
            }
        }

        private async Task MoveSetting(SettingViewModel setting, bool up)
        {
            int currentIndex = Settings.IndexOf(setting);

            bool cantMove = currentIndex == 0 && up
                || currentIndex == Settings.Count() - 1 && !up;

            if (cantMove) return;

            int neighborIndex = up ? currentIndex - 1 : currentIndex + 1;
            SettingViewModel temp = Settings[neighborIndex];
            Settings[neighborIndex] = Settings[currentIndex];
            Settings[currentIndex] = temp;

            await SaveCurrentEnvironment();
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

            if (SelectedSetting != null)
            {
                Settings.Where(x => x != SelectedSetting).ForEach(x => x.MusicPlayer.SmoothStop());

                await SelectedSetting.MusicPlayer.AdjustPlayer(SelectedScene);
            }

            OnPropertyChanged(nameof(IsSomethingPlaying));
            OnPropertyChanged(nameof(CurrentSceneText));

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
            List<TrackSetupViewModel> playingElements = Settings.SelectMany(x => x.Elements)
                .Where(x => x.IsPlaying)
                .Select(x => new TrackSetupViewModel
                {
                    FilePath = x.FilePath,
                    Volume = x.Volume,
                })
                .ToList();

            scene.Elements = new ObservableCollection<TrackSetupViewModel>(playingElements);
            scene.IsMusicEnabled = SelectedSetting.MusicPlayer.IsOn;
            scene.IsMusicShuffled = SelectedSetting.MusicPlayer.Shuffle;
            scene.IsOneMusicTrackRepeatEnabled = SelectedSetting.MusicPlayer.IsRepeat;
            scene.MusicVolume = SelectedSetting.MusicPlayer.Volume;

            await SaveCurrentEnvironment();

            await Application.Current.MainPage.DisplayAlert(
                "Nice!",
                $"Scene «{scene.Name}» successfully saved!",
                "Pretty!"
            );
        }

        private async Task MoveScene(SceneViewModel scene, bool toTheLeft)
        {
            int currentIndex = SelectedSetting.Scenes.IndexOf(scene);

            bool cantMove = currentIndex == 0 && toTheLeft 
                || currentIndex == SelectedSetting.Scenes.Count() - 1 && !toTheLeft;

            if (cantMove) return;

            int neighborIndex = toTheLeft ? currentIndex - 1 : currentIndex + 1;
            SceneViewModel temp = SelectedSetting.Scenes[neighborIndex];
            SelectedSetting.Scenes[neighborIndex] = SelectedSetting.Scenes[currentIndex];
            SelectedSetting.Scenes[currentIndex] = temp;

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
            IEnumerable<string> result = await FileManager.ChooseAndCopySoundsToAppData();

            if (result.Count() + SelectedSetting.Elements.Count() > _maxElementsCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (string element in result)
            {
                if (!SelectedSetting.Elements.Any(x => x.FilePath.Equals(element)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { Loop = true, FilePath = element };
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

        private async Task DeleteElement(SceneComponentViewModel component)
        {
            SelectedSetting.Elements.Remove(component);
            component.Dispose();

            if (!SelectedSetting.Effects.Any(x => x.FilePath == component.FilePath)
                && !SelectedSetting.Music.Any(x => x.FilePath == component.FilePath))
            {
                await FileManager.DeleteFromAppData(Path.GetFileName(component.FilePath));
            }

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
            IEnumerable<string> result = await FileManager.ChooseAndCopySoundsToAppData();

            if (result.Count() + SelectedSetting.Effects.Count() > _maxEffectsCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (string element in result)
            {
                if (!SelectedSetting.Effects.Any(x => x.FilePath.Equals(element)))
                {
                    SceneComponentViewModel player = new SceneComponentViewModel { FilePath = element };
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

        private async Task DeleteEffect(SceneComponentViewModel component)
        {
            SelectedSetting.Effects.Remove(component);
            component.Dispose();

            if (!SelectedSetting.Elements.Any(x => x.FilePath == component.FilePath)
                && !SelectedSetting.Music.Any(x => x.FilePath == component.FilePath))
            {
                await FileManager.DeleteFromAppData(Path.GetFileName(component.FilePath));
            }

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

        private double _effectsVolume = 100;
        public double EffectsVolume
        {
            get => _effectsVolume;
            set
            {
                SetProperty(ref _effectsVolume, value);
                Settings.SelectMany(s => s.Effects).ForEach(x => x.Volume = value);
            }
        }
        #endregion

        #region Music
        private int _maxMusicTracksCount = 100;
        private async Task AddMusic()
        {
            IEnumerable<string> result = await FileManager.ChooseAndCopySoundsToAppData();

            if (result.Count() + SelectedSetting.Music.Count() > _maxMusicTracksCount)
            {
                await AlertTooManyFiles();

                return;
            }

            foreach (string element in result)
            {
                if (!SelectedSetting.Music.Any(x => x.FilePath.Equals(element)))
                {
                    SceneComponentViewModel track = new SceneComponentViewModel { FilePath = element };
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

        private async Task DeleteMusicTrack(SceneComponentViewModel component)
        {
            SelectedSetting.Music.Remove(component);
            component.Dispose();

            if (!SelectedSetting.Elements.Any(x => x.FilePath == component.FilePath)
                && !SelectedSetting.Effects.Any(x => x.FilePath == component.FilePath))
            {
                await FileManager.DeleteFromAppData(Path.GetFileName(component.FilePath));
            }

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
            if (SelectedSetting.MusicPlayer.IsMusicPlaying)
            {
                SelectedSetting.MusicPlayer.StopMusic();
            }
            else
            {
                await SelectedSetting.MusicPlayer.PlayMusic();
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
            else
            {
                //Cosmetics
                SelectedSetting.MusicPlayer.Shuffle = true;
                SelectedSetting.MusicPlayer.IsOn = true;
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
        public Command MoveSettingUpCommand { get => new Command<SettingViewModel>(async (scetting) => await MoveSetting(scetting, true)); }
        public Command MoveSettingDownCommand { get => new Command<SettingViewModel>(async (scetting) => await MoveSetting(scetting, false)); }
        public Command AddSceneCommand { get => new Command(async () => await GoToAddScene()); }
        public Command SaveSceneCommand { get => new Command<SceneViewModel>(async (scene) => await SaveScene(scene)); }
        public Command EditSceneCommand { get => new Command<SceneViewModel>(async (scene) => await GoToEditScene(scene)); }
        public Command SelectSceneCommand { get => new Command<SceneViewModel>(async (scene) => await SelectScene(scene)); }
        public Command DeleteSceneCommand { get => new Command<SceneViewModel>(async (scene) => await DeleteScene(scene)); }
        public Command MoveSceneLeftCommand { get => new Command<SceneViewModel>(async (scene) => await MoveScene(scene, true)); }
        public Command MoveSceneRightCommand { get => new Command<SceneViewModel>(async (scene) => await MoveScene(scene, false)); }
        public Command AddElementsCommand { get => new Command(async () => await AddElements()); }
        public Command EditElementCommand { get => new Command<SceneComponentViewModel>(async (element) => await GoToEditElement(element)); }
        public Command DeleteElementCommand { get => new Command<SceneComponentViewModel>(async (element) => await DeleteElement(element)); }
        public Command AddEffectsCommand { get => new Command(async () => await AddEffects()); }
        public Command EditEffectCommand { get => new Command<SceneComponentViewModel>(async (effect) => await GoToEditEffect(effect)); }
        public Command DeleteEffectCommand { get => new Command<SceneComponentViewModel>(async (effect) => await DeleteEffect(effect)); }
        public Command AddMusicCommand { get => new Command(async () => await AddMusic()); }
        public Command EditMusicCommand { get => new Command<SceneComponentViewModel>(async (music) => await GoToEditMusicTrack(music)); }
        public Command DeleteMusicCommand { get => new Command<SceneComponentViewModel>(async (music) => await DeleteMusicTrack(music)); }
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

                Settings.SelectMany(x => x.Effects)
                    .Where(x => x.IsPlaying)
                    .ForEach(x => x.SmoothStop());

                Settings.Select(x => x.MusicPlayer)
                    .Where(x => x.IsMusicPlaying)
                    .ForEach(x => x.StopMusic());
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
                bool isScenePlaying = Settings.SelectMany(x => x.Elements).Any(x => x.IsPlaying);
                bool isEffectPlaying = Settings.SelectMany(x => x.Effects).Any(x => x.IsPlaying);
                bool isMusicPlaying = Settings.Select(x => x.MusicPlayer).Any(x => x.IsMusicPlaying);
                bool isSomethingPlaying = isScenePlaying || isEffectPlaying || isMusicPlaying;

                if (GlobalPlayActivityIndicatorIsVisible && !isSomethingPlaying)
                {
                    GlobalPlayActivityIndicatorIsVisible = false;
                }

                Settings.ForEach(x => x.UpdateHasSelectedScene());

                return isSomethingPlaying;
            }
        }

        public string CurrentSceneText
        {
            get
            {
                string current;

                if (SelectedSetting == null || SelectedScene == null)
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

        #region Utility
        public bool IsPlayerSettingsExpanded
        {
            get => Preferences.Get(nameof(IsPlayerSettingsExpanded), true);
            set => Preferences.Set(nameof(IsPlayerSettingsExpanded), value);
        }

        private int _elementsSpan;
        public int ElementsSpan
        {
            get => Preferences.Get(nameof(ElementsSpan), 3);
            set
            {
                Preferences.Set(nameof(ElementsSpan), value);
                SetProperty(ref _elementsSpan, value);
            }
        }

        private int _effectsSpan;
        public int EffectsSpan
        {
            get
            {
                return Preferences.Get(nameof(EffectsSpan), 4);
            }
            set
            {
                Preferences.Set(nameof(EffectsSpan), value);
                SetProperty(ref _effectsSpan, value);
            }
        }

        private async Task AlertTooManyFiles()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Too many files!",
                $"You have selected too many files, split them into groups and put them in different settings.",
                "Ok :("
            );
        }
        #endregion

        #region Help
        public Command GoToHelpCommand 
        { 
            get => new Command<EHelpTopic>(async (topic) => 
                await Shell.Current.GoToAsync($"{nameof(HelpPage)}?topic={topic}")
            ); 
        }
        #endregion

        #region Other

        public Command OpenAppDataCommand { get => new Command(async () => {
            await LocalDataHelper.OpenAppDataFolder();
        });}

        #endregion
    }
}