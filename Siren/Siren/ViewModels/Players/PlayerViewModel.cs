using Siren.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Timer = System.Timers.Timer;

namespace Siren.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        public IAudioPlayer Player { get; }

        public PlayerViewModel()
        {
            OpenCommand = new Command(async () => await Open());
            PlayCommand = new Command(async () => await PlayPause());
            StopCommand = new Command(() => Stop(true));
            SeekCommand = new Command(Seek);
            StopSeekCommand = new Command(StopSeek);

            Player = DependencyService.Get<IAudioPlayer>(DependencyFetchTarget.NewInstance);
            Player.OnPositionChanged += OnPositionChanged;
            Player.OnIsPlayingChanged += OnIsPlayingChanged;
        }

        #region Play & Stop

        private bool _loaded = false;

        public virtual async Task PlayPause()
        {
            try
            {
                if (!_loaded)
                {
                    await Load(FilePath);
                    _loaded = true;
                }

                Player.PlayPause();
                OnPropertyChanged(nameof(IsPlaying));
            }
            catch (FileNotFoundException)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"File {FilePath} is missing. Try to restore file to this path or delete the element and add it again.",
                    "Oh? Ok..."
                );
            }
        }

        public async Task JustPlay(double volume = 100)
        {
            if (!IsPlaying)
            {
                Volume = volume;
                await PlayPause();
            }
        }

        public async Task SmoothPlay(double targetVolume)
        {
            if (!IsPlaying)
            {
                Volume = 0;
                Thread.Sleep(100);
                await PlayPause();
            }
            StartAdjustingVolume(targetVolume);
        }

        private bool _isManualStopped = false;

        public virtual void Stop(bool isManual = true)
        {
            _isManualStopped = isManual;
            _timer.Stop();
            Player.Stop();
            OnPropertyChanged(nameof(IsPlaying));
        }

        public void SmoothStop()
        {
            StartAdjustingVolume(0);
        }
        
        private Timer _timer = new Timer();
        private double _targetVolume;

        private void StartAdjustingVolume(double targetVolume)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = new Timer(4);
            _targetVolume = targetVolume;
            _timer.Elapsed += AdjustVolume;
            _timer.Start();
        }

        public object _timerLocker = new object();

        private void AdjustVolume(object sender, ElapsedEventArgs e)
        {
            lock (_timerLocker)
            {
                double step = 0.5;

                if (_targetVolume > Volume)
                {
                    if (Volume + step >= _targetVolume)
                        Volume = _targetVolume;
                    else
                        Volume += step;
                }
                if (_targetVolume < Volume)
                {
                    if (Volume - step <= _targetVolume)
                        Volume = _targetVolume;
                    else
                        Volume -= step;
                }
                if (_targetVolume == Volume)
                {
                    if (_targetVolume == 0)
                    {
                        Stop();
                        Thread.Sleep(100);
                        Volume = 100;
                    }
                    else
                    {
                        _timer.Stop();
                    }
                }
            }
        }

        #endregion

        public async Task Load(string path)
        {
            FilePath = path;

            await Player.LoadAsync(path);

            TrackDurationSeconds = Player.Duration.TotalSeconds;
            Duration = TimeSpan.FromSeconds(TrackDurationSeconds);
        }

        public void Dispose()
        {
            _timer.Dispose();
            Player.Dispose();
        }

        private bool _isSeeking = false;

        private void Seek()
        {
            _isSeeking = true;
        }

        private void StopSeek()
        {
            Player.Position = TimeSpan.FromSeconds(Position);
            _isSeeking = false;
        }

        private void OnPositionChanged(TimeSpan newPosition)
        {
            if (!_isSeeking)
            {
                Position = newPosition.TotalSeconds;
            }
        }

        public event EventHandler<OnIsPlayingChangedEventArgs> OnIsPlayingChangedEvent;

        private void OnIsPlayingChanged(bool isPlaying)
        {
            OnPropertyChanged(nameof(IsPlaying));
            if (OnIsPlayingChangedEvent != null)
            {
                OnIsPlayingChangedEvent(this, new OnIsPlayingChangedEventArgs(isPlaying, _isManualStopped));
            }
            _isManualStopped = false;
        }

        private async Task Open()
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Default);

            if (result != null)
            {
                await Load(result.FullPath);
            }
        }

        #region Bindable properties

        public ICommand OpenCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SeekCommand { get; }
        public ICommand StopSeekCommand { get; }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private TimeSpan _time;
        public TimeSpan Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private double _trackDurationSeconds = 10;
        public double TrackDurationSeconds
        {
            get => _trackDurationSeconds;
            set => SetProperty(ref _trackDurationSeconds, value);
        }

        private double _position;
        public double Position
        {
            get => _position;
            set
            {
                SetProperty(ref _position, value);
                Time = TimeSpan.FromSeconds(value);
                PositionPercent = Position / TrackDurationSeconds;
                if(Position == TrackDurationSeconds)
                {
                    Stop(false);
                }
            }
        }

        private double _positionPercent;
        public double PositionPercent
        {
            get => _positionPercent;
            set => SetProperty(ref _positionPercent, value);
        }

        public double Volume
        {
            get => Math.Round(Player.Volume * 100d,2);
            set
            {
                Player.Volume = Math.Round(value / 100, 3);
                OnPropertyChanged(nameof(Volume));
            }
        }

        public bool IsPlaying
        {
            get => Player.IsPlaying;
        }

        public bool Loop
        {
            get => Player.Loop;
            set => Player.Loop = value;
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string Name =>  Path.GetFileNameWithoutExtension(FilePath);

        #endregion
    }

    public class OnIsPlayingChangedEventArgs : EventArgs
    {
        public OnIsPlayingChangedEventArgs(bool isPlaying, bool isManualStopped = false)
        {
            IsPlaying = isPlaying;
            IsManualStopped = isManualStopped;
        }

        public bool IsPlaying { get; set; }
        public bool IsManualStopped { get; set; }

    }
}
