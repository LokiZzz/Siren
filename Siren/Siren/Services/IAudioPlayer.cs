using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Siren.Services
{
    public delegate void PositionChangedHandler(TimeSpan newPosition);
    
    public delegate void IsPlayingChangedHandler(bool isPlaying);

    public interface IAudioPlayer
    {
        TimeSpan CutFrom { get; set; }
        TimeSpan CutTo { get; set; }
        TimeSpan FadeIn { get; set; }
        TimeSpan FadeOut { get; set; }
        bool IsPlaying { get; }
        bool Loop { get; set; }
        bool Mute { get; set; }
        TimeSpan Position { get; set; }
        TimeSpan Duration { get; }
        double Volume { get; set; }

        Task<List<string>> GetAvailiableAudioDevicesAsync();
        Task LoadAsync(string path);
        void PlayPause();
        void Stop();
        Task SetAudioDeviceAsync(string deviceName);
        void Dispose();
        
        event PositionChangedHandler OnPositionChanged;
        event IsPlayingChangedHandler OnIsPlayingChanged;
    }
}
