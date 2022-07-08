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
        public bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string _alias;
        public string Alias
        {
            get => string.IsNullOrEmpty(_alias) ? Name : _alias;
            set => SetProperty(ref _alias, value);
        }

        public override async Task PlayPause()
        {
            await base.PlayPause();
            MessagingCenter.Send(this, Messages.ElementPlayingStatusChanged);
        }

        public override void Stop(bool isManual = true)
        {
            base.Stop(isManual);
            MessagingCenter.Send(this, Messages.ElementPlayingStatusChanged);
        }
    }
}
