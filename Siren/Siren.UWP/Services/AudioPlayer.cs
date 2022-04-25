﻿using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
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

        public bool IsPlaying { get; private set; }

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

        private void MediaEnded(MediaPlayer sender, object args)
        {
            if (!Loop) IsPlaying = false;
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

        public async Task LoadAsync(string path)
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(path);
            MediaSource source = MediaSource.CreateFromStorageFile(file);
            await source.OpenAsync();
            _player.Source = source;
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
        }
    }
}