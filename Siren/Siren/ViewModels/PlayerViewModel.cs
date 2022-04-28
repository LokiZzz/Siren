using Siren.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        public IAudioPlayer Player { get; }

        public PlayerViewModel()
        {
            OpenCommand = new Command(async () => await Open());
            PlayCommand = new Command(Play);
            StopCommand = new Command(Stop);
            SeekCommand = new Command(Seek);
            StopSeekCommand = new Command(StopSeek);

            Player = DependencyService.Get<IAudioPlayer>(DependencyFetchTarget.NewInstance);
            Player.OnPositionChanged += OnPositionChanged;
        }

        public void Play()
        {
            Player.PlayPause();
            OnPropertyChanged(nameof(IsPlaying));
        }

        public void SoftPlay(double targetVolume = 1)
        {
            Volume = 0;
            Play();
            IncreaseVolumeSoft();
        }

        Timer timer = new Timer();

        private void IncreaseVolumeSoft()
        {
            timer.Dispose();
            timer = new Timer();
            timer.Interval = 4;
            timer.Elapsed += IncreaseVolume;
            timer.Start();
        }

        private void IncreaseVolume(object sender, ElapsedEventArgs e)
        {
            Volume += 0.5;
        }

        public void Stop()
        {
            Player.Stop();
            OnPropertyChanged(nameof(IsPlaying));
        }

        public async Task Load(string path)
        {
            FilePath = path;

            await Player.LoadAsync(path);

            TrackDurationSeconds = Math.Round(Player.Duration.TotalSeconds, 0);
            Duration = TimeSpan.FromSeconds(TrackDurationSeconds);
        }

        public void Dispose()
        {
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
            get => Player.Volume * 100;
            set
            {
                Player.Volume = value / 100;
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

        private async Task Open()
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Default);

            if (result != null)
            {
                await Load(result.FullPath);
            }
        }
    }
}
