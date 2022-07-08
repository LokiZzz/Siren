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
        private ObservableCollection<SceneComponentViewModel> _tracks = new ObservableCollection<SceneComponentViewModel>();
        public ObservableCollection<SceneComponentViewModel> Tracks
        {
            get => _tracks;
            set => SetProperty(ref _tracks, value);
        }

        private bool _isMusicPlaying;
        public bool IsMusicPlaying
        {
            get => _isMusicPlaying;
            set => SetProperty(ref _isMusicPlaying, value);
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
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task Play()
        {
            if(!IsMusicPlaying)
            {
                RegisterNextTrackHandler();

                await PlayNextTrack();
                _nextTrackIsFirst = false;
                IsMusicPlaying = true;
            }
        }

        public void Stop()
        {
            if (IsMusicPlaying)
            {
                UnregisterNextTrackHandler();

                _currentMusicTrackIndex = -1;
                _stillNotPlayedMusicTracks.Clear();
                SmoothStop();
                _nextTrackIsFirst = true;
                IsMusicPlaying = false;
            }
        }

        private async Task PlayNextTrack()
        {
            await _semaphore.WaitAsync();

            try
            {
                if(Shuffle)
                {
                    if(!_stillNotPlayedMusicTracks.Any())
                    {
                        _stillNotPlayedMusicTracks = Tracks.Select(x => Tracks.IndexOf(x)).ToList();
                    }

                    int trackIndex = _stillNotPlayedMusicTracks[new Random().Next(0, _stillNotPlayedMusicTracks.Count())];
                    _currentMusicTrackIndex = trackIndex;
                    _stillNotPlayedMusicTracks.Remove(trackIndex);
                }
                else
                {
                    if(_currentMusicTrackIndex == Tracks.Count() - 1)
                    {
                        _currentMusicTrackIndex = 0;
                    }
                    else
                    {
                        _currentMusicTrackIndex++;
                    }
                }

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
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async void OnIsPlayingChanged(object sender, OnIsPlayingChangedEventArgs e)
        {
            if (!e.IsPlaying && !e.IsManualStopped)
            {
                await PlayNextTrack();
            }
        }

        private void RegisterNextTrackHandler()
        {
            OnIsPlayingChangedEvent += OnIsPlayingChanged;
        }

        private void UnregisterNextTrackHandler()
        {
            OnIsPlayingChangedEvent -= OnIsPlayingChanged;
        }
    }
}
