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
        public IAudioPlayerService AudioPlayerService { get; }

        public AboutViewModel()
        {
            Title = "About";
            PlayCommand = new Command(async () => await Play());
            AudioPlayerService = DependencyService.Get<IAudioPlayerService>();
        }
        public ICommand PlayCommand { get; }

        bool loaded = false;

        private async Task Play()
        {
            try
            {
                if (!loaded)
                {
                    FileResult result = await FilePicker.PickAsync(PickOptions.Default);
                    await AudioPlayerService.Load(result.FullPath);
                    loaded = true;
                }

                AudioPlayerService.PlayPause();
            }
            catch(Exception ex)
            {
                string stop = "";
            }
        }

    }
}