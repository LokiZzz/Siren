using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Siren.ViewModels
{
    public class TrackSetupViewModel : BaseViewModel
    {
        public string Alias { get; set; }
        public string FilePath { get; set; }
        public double Volume { get; set; }
    }
}
