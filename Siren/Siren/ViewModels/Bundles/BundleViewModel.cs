using Siren.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public int SettingsCount => Bundle?.Settings?.Count ?? 0;
        public int ScenesCount => Bundle?.Settings?.SelectMany(x => x.Scenes)?.Count() ?? 0;
        public int ElementsCount => Bundle?.Settings?.SelectMany(x => x.Elements)?.Count() ?? 0;
        public int EffectsCount => Bundle?.Settings?.SelectMany(x => x.Effects)?.Count() ?? 0;
        public int MusicCount => Bundle?.Settings?.SelectMany(x => x.Music)?.Count() ?? 0;
    }
}
