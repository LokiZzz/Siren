using Siren.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.ViewModels.Bundles
{
    public class BundleViewModel : BaseViewModel
    {
        public BundleViewModel(Bundle bundle)
        {
            Bundle = bundle;
        }

        public Bundle Bundle { get; set; }

        private bool _isActivated;
        public bool IsActivated
        {
            get => Bundle.IsActivated;
            set
            {
                SetProperty(ref _isActivated, value);
                Bundle.IsActivated = value;
            }
        }
    }
}
