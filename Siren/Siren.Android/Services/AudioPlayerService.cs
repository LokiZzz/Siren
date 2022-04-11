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
using Xamarin.Forms;

[assembly: Dependency(typeof(AudioPlayerService))]
namespace Siren.Droid.Services
{
    public class AudioPlayerService : IAudioPlayerService
    {
        MediaPlayer player;

        public async Task Load(string filePath)
        {
            if (player == null) player = new MediaPlayer();

            player.Reset();
            player.SetDataSource(filePath);
            player.Prepare();
        }

        bool isPlaying = false;

        public void PlayPause()
        {
            if (isPlaying) player.Pause(); else player.Start();

            isPlaying = !isPlaying;
        }
    }
}