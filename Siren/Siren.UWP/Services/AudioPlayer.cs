using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayer))]
namespace Siren.UWP.Services
{
    public class AudioPlayer : IAudioPlayer
    {
        private MediaPlayer _player;

        public AudioPlayer()
        {
            _player = new MediaPlayer();
            _player.PlaybackSession.PositionChanged += FirePositionChanged;
            _player.MediaEnded += MediaEnded;
        }

        private bool _isPlatying;
        public bool IsPlaying
        {
            get => _isPlatying;
            private set
            {
                _isPlatying = value;
                OnIsPlayingChanged(value);
            }
        }

        public TimeSpan Position
        {
            get => _player.PlaybackSession.Position;
            set => _player.PlaybackSession.Position = value;
        }

        public event PositionChangedHandler OnPositionChanged;

        private void FirePositionChanged(MediaPlaybackSession sender, object args)
        {
            OnPositionChanged(sender.Position);
        }

        public event IsPlayingChangedHandler OnIsPlayingChanged;

        private void MediaEnded(MediaPlayer sender, object args)
        {
            //VM итак вызывает Stop() по окончанию времени.
            //if (!Loop) IsPlaying = false;
        }

        public TimeSpan Duration => _player.PlaybackSession.NaturalDuration;

        public double Volume
        {
            get => _player.Volume;
            set => _player.Volume = value;
        }

        public bool Mute
        {
            get => _player.IsMuted;
            set => _player.IsMuted = value;
        }

        public bool Loop
        {
            get => _player.IsLoopingEnabled;
            set => _player.IsLoopingEnabled = value;
        }

        public TimeSpan CutFrom { get; set; }

        public TimeSpan CutTo { get; set; }

        public TimeSpan FadeIn { get; set; }

        public TimeSpan FadeOut { get; set; }

        private MediaSource _source = null;
        public async Task LoadAsync(string path)
        {
            string fileName = Path.GetFileName(path);
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            _source = MediaSource.CreateFromStorageFile(file);
            await _source.OpenAsync();
            _player.Source = _source;

            while(Duration <= TimeSpan.FromMilliseconds(0))
            {
                await Task.Delay(2);
            }
        }

        public void PlayPause()
        {
            if (IsPlaying) _player.Pause(); else _player.Play();
            IsPlaying = !IsPlaying;
        }

        public void Stop()
        {
            _player.Pause();
            _player.PlaybackSession.Position = TimeSpan.FromSeconds(0);
            IsPlaying = false;
        }

        public async Task<List<string>> GetAvailiableAudioDevicesAsync()
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            DeviceInformationCollection outputDevices = await DeviceInformation.FindAllAsync(audioSelector);

            return outputDevices.Select(o => o.Name).ToList();
        }

        public async Task SetAudioDeviceAsync(string deviceName)
        {
            string audioSelector = MediaDevice.GetAudioRenderSelector();
            DeviceInformationCollection outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            DeviceInformation device = outputDevices.FirstOrDefault(o => o.Name.Equals(deviceName));

            if (device != null)
            {
                _player.AudioDevice = device;
            }
        }

        public void Dispose()
        {
            _player.Dispose();

            if(_source != null)
            {
                _source.Dispose();
            }
        }
    }
}
