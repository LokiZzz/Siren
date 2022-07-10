using Siren.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace Siren.ViewModels.Players
{
    public class SceneMusicPlayerViewModel : PlayerViewModel
    {
        public SceneMusicPlayerViewModel()
        {
            OnIsPlayingChangedEvent += OnIsPlayingChanged;
        }

        private ObservableCollection<SceneComponentViewModel> _tracks = new ObservableCollection<SceneComponentViewModel>();
        public ObservableCollection<SceneComponentViewModel> Tracks
        {
            get => _tracks;
            set => SetProperty(ref _tracks, value);
        }

        private bool _activityIndicatorIsVisible = false;
        public bool ActivityIndicatorIsVisible
        {
            get => _activityIndicatorIsVisible;
            set => SetProperty(ref _activityIndicatorIsVisible, value);
        }

        private bool _isMusicPlaying;
        public bool IsMusicPlaying
        {
            get => _isMusicPlaying;
            set 
            {
                if (ActivityIndicatorIsVisible && !value)
                {
                    ActivityIndicatorIsVisible = false;
                }

                SetProperty(ref _isMusicPlaying, value); 
            }
        }

        private bool _shuffle = true;
        public bool Shuffle
        {
            get => _shuffle;
            set => SetProperty(ref _shuffle, value);
        }

        private double _targetVolume = 100;
        public double TargetVolume
        {
            get => _targetVolume;
            set => SetProperty(ref _targetVolume, value);
        }

        private bool _nextTrackIsFirst = true;
        private int _currentMusicTrackIndex = -1;
        private List<int> _stillNotPlayedMusicTracks = new List<int>();
        private Stack<int> _history = new Stack<int>();

        public async Task PlayMusic()
        {
            if(!IsMusicPlaying && Tracks.Any())
            {
                RegisterNextTrackHandler();

                await ChooseNextTrackAndPlayIt();

                IsMusicPlaying = true;
                _nextTrackIsFirst = false;
            }
        }

        public void StopMusic()
        {
            if (IsMusicPlaying)
            {
                ResetState();

                SmoothStop();
            }
        }

        private SemaphoreSlim _playNextSemaphore = new SemaphoreSlim(1, 1);

        private async Task ChooseNextTrackAndPlayIt()
        {
            await _playNextSemaphore.WaitAsync();

            try
            {
                if (Shuffle)
                {
                    if (!_stillNotPlayedMusicTracks.Any())
                    {
                        _stillNotPlayedMusicTracks = Tracks.Select(x => Tracks.IndexOf(x)).ToList();
                    }

                    int trackIndex = _stillNotPlayedMusicTracks[new Random().Next(0, _stillNotPlayedMusicTracks.Count())];
                    _currentMusicTrackIndex = trackIndex;
                    _stillNotPlayedMusicTracks.Remove(trackIndex);
                }
                else
                {
                    if (_currentMusicTrackIndex == Tracks.Count() - 1)
                    {
                        _currentMusicTrackIndex = 0;
                    }
                    else
                    {
                        _currentMusicTrackIndex++;
                    }
                }

                await PlayCurrentIndexTrack();
            }
            finally
            {
                _playNextSemaphore.Release();
            }
        }

        private async Task PlayCurrentIndexTrack()
        {
            await Load(Tracks[_currentMusicTrackIndex].FilePath);
            Tracks.ForEach(x => x.IsSelected = false);
            Tracks[_currentMusicTrackIndex].IsSelected = true;

            if (_nextTrackIsFirst)
            {
                await SmoothPlay(TargetVolume);

            }
            else
            {
                await JustPlay();
            }

            if (!_history.Any() || _history.Peek() != _currentMusicTrackIndex)
            {
                _history.Push(_currentMusicTrackIndex);
            }
        }

        private bool _stopPlayNext = false;

        private async void OnIsPlayingChanged(object sender, OnIsPlayingChangedEventArgs e)
        {
            if (!e.IsPlaying)
            {
                if (!e.IsManualStopped && !_stopPlayNext)
                {
                    await ChooseNextTrackAndPlayIt();
                }
                else 
                {
                    IsMusicPlaying = false;
                    Volume = TargetVolume;
                }
            }

            MessagingCenter.Send(this, Messages.MusicTrackPlayingStatusChanged);
        }

        private void RegisterNextTrackHandler()
        {
            _stopPlayNext = false;
        }

        private void ResetState()
        {
            ActivityIndicatorIsVisible = true;
            _currentMusicTrackIndex = -1;
            _stillNotPlayedMusicTracks.Clear();
            _nextTrackIsFirst = true;
            _stopPlayNext = true;
        }

        public Command NextTrackCommand { get => new Command(async () => await SwitchTrack(true)); }
        public Command PreviousTrackCommand { get => new Command(async () => await SwitchTrack(false)); }
        public Command PlaySpecificTrackCommand { get => new Command<SceneComponentViewModel>(async (music) => await PlaySpecificTrack(music)); }

        private async Task PlaySpecificTrack(SceneComponentViewModel music)
        {
            if (IsPlaying)
            {
                Stop(false);
            }

            _currentMusicTrackIndex = Tracks.IndexOf(music);
            await PlayCurrentIndexTrack();
            IsMusicPlaying = true;
        }

        private async Task SwitchTrack(bool forward)
        {
            _stopPlayNext = true;

            if (IsMusicPlaying)
            {
                if(forward)
                {
                    Stop(false);
                    await ChooseNextTrackAndPlayIt();
                    IsMusicPlaying = true;
                }
                else
                {
                    if(_history.Count > 1)
                    {
                        Stop(false);
                        _history.Pop();
                        _currentMusicTrackIndex = _history.Peek();
                        await PlayCurrentIndexTrack();
                        IsMusicPlaying = true;
                    }
                }

            }
            else 
            {
                if (forward)
                {
                    await PlayMusic();
                    IsMusicPlaying = true;
                }
                else
                {
                    if (_history.Count > 1)
                    {
                        _history.Pop();
                        _currentMusicTrackIndex = _history.Peek();
                        await PlayCurrentIndexTrack();
                        IsMusicPlaying = true;
                    }
                }
            }

            _stopPlayNext = false;
        }
    }
}
