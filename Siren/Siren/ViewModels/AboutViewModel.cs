using Siren.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public IAudioPlayer Player { get; }

        public AboutViewModel()
        {
            Title = "About";

            OpenCommand = new Command(async () => await Open());
            PlayCommand = new Command(Play);
            StopCommand = new Command(Stop);
            SeekCommand = new Command(Seek);
            StopSeekCommand = new Command(StopSeek);

            Player = DependencyService.Get<IAudioPlayer>();
            Player.OnPositionChanged += OnPositionChanged;
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
            }
        }

        private async Task Open()
        {
            FileResult result = await FilePicker.PickAsync(PickOptions.Default);
            await Player.LoadAsync(result.FullPath);
            TrackDurationSeconds = Math.Round(Player.Duration.TotalSeconds, 0);
        }

        private void Play() => Player.PlayPause();

        private void Stop() => Player.Stop();
    }
}