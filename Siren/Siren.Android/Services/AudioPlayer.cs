using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Siren.Droid.Services;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayer))]
namespace Siren.Droid.Services
{
    public class AudioPlayer : IAudioPlayer
    {
        private MediaPlayer _player = new MediaPlayer();

        public AudioPlayer()
        {
            _player = new MediaPlayer();
            _positionChangedTimer.Elapsed += FirePositionChanged;
        }

        public TimeSpan CutFrom { get; set; }

        public TimeSpan CutTo { get; set; }

        public TimeSpan FadeIn { get; set; }

        public TimeSpan FadeOut { get; set; }

        public bool IsPlaying { get; private set; }

        public bool Loop
        {
            get => _player.Looping;
            set => _player.Looping = value;
        }

        public TimeSpan Position
        {
            get => TimeSpan.FromMilliseconds(_player.CurrentPosition);
            set => _player.SeekTo(Convert.ToInt32(value.TotalMilliseconds));
        }

        public event PositionChangedHandler OnPositionChanged;

        public TimeSpan Duration => TimeSpan.FromMilliseconds(_player.Duration);

        private double _maxVolume = 1;
        private double _currentVolume = 1;

        public double Volume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                float log = (float)(Math.Log(_maxVolume - _currentVolume) / Math.Log(_maxVolume));
                _player.SetVolume(log, log);
            }
        }

        private double _mutedVolume;

        public bool Mute
        {
            get => _currentVolume == 0;
            set
            {
                if(value)
                {
                    _mutedVolume = _currentVolume;
                    _currentVolume = 0;
                }
                else
                {
                    _currentVolume = _mutedVolume;
                }
            }
        }

        public async Task LoadAsync(string path)
        {
            await Task.Run(() =>
            {
                _player.Reset();
                _player.SetDataSource(path);
                _player.Prepare();
            });
        }

        Timer _positionChangedTimer = new Timer { Interval = 250 };

        public void PlayPause()
        {
            if (IsPlaying)
            {
                _player.Pause();
                _positionChangedTimer.Enabled = false;
            }
            else
            {
                _player.Start();
                _positionChangedTimer.Enabled = true;
            }

            IsPlaying = !IsPlaying;

        }

        private void FirePositionChanged(object sender, ElapsedEventArgs e)
        {
            OnPositionChanged(Position);
        }

        public void Stop()
        {
            _player.Pause();
            Position = TimeSpan.FromMilliseconds(0);
            IsPlaying = false;
        }

        public async Task<List<string>> GetAvailiableAudioDevicesAsync()
        {
            return await Task.Run(() =>
            {
                AudioManager manager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
                AudioDeviceInfo[] devices = manager.GetDevices(GetDevicesTargets.Outputs);

                return devices.Select(x => $"{x.ProductName}: {x.Type}").ToList();
            });
        }

        public async Task SetAudioDeviceAsync(string deviceName)
        {
            await Task.Run(() =>
            {
                AudioManager manager = (AudioManager)Android.App.Application.Context.GetSystemService(Context.AudioService);
                AudioDeviceInfo[] devices = manager.GetDevices(GetDevicesTargets.Outputs);

                string productName = deviceName.Split(": ")[0];
                string type = deviceName.Split(": ")[1];

                AudioDeviceInfo device = devices.FirstOrDefault(x => x.ProductName.Equals(productName) && x.Type.ToString().Equals(type));

                _player.SetPreferredDevice(device);
            });
        }

        public void Dispose()
        {
            _player.Dispose();
        }
    }
}