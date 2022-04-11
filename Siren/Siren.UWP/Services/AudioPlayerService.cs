using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayerService))]
namespace Siren.UWP.Services
{
    public class AudioPlayerService : IAudioPlayerService
    {
        MediaPlayer player;

        public async Task Load(string filePath)
        {
            if (player == null) player = new MediaPlayer();

            string audioSelector = MediaDevice.GetAudioRenderSelector();
            DeviceInformationCollection outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
            player.AudioDevice = outputDevices.FirstOrDefault(o => o.Name.Equals("Speakers (Focusrite Usb Audio)"));

            //MediaSource source = MediaSource.CreateFromUri(new Uri(@"C:\Users\lokiz\Desktop\123.mp3"));

            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
            MediaSource source = MediaSource.CreateFromStorageFile(file);

            player.Source = source;
        }

        bool isPlaying = false;

        public void PlayPause()
        {
            if (isPlaying) player.Pause(); else player.Play();

            isPlaying = !isPlaying;
        }
    }
}
