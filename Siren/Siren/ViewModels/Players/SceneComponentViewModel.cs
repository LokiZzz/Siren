using Siren.Messaging;
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
    /// <summary>
    /// Element or effect player view model.
    /// </summary>
    public class SceneComponentViewModel : PlayerViewModel
    {
        public string Alias { get; set; }

        public ImageSource Image { get; set; }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                Image = ImageSource.FromFile(value);
                OnPropertyChanged(nameof(Image));
            }
        }

        public override void PlayPause()
        {
            base.PlayPause();

            MessagingCenter.Send(this, Messages.ElementPlayingStatusChanged);
        }

        public override void Stop()
        {
            base.Stop();

            MessagingCenter.Send(this, Messages.ElementPlayingStatusChanged);
        }
    }
}
