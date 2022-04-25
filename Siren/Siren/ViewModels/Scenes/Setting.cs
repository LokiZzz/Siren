using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Siren.ViewModels
{
    public class Setting
    {
        public string Name { get; set; }
        public ObservableCollection<Scene> Scenes { get; set; } = new ObservableCollection<Scene>();
        public ObservableCollection<PlayerViewModel> Elements { get; set; } = new ObservableCollection<PlayerViewModel>();
        public ObservableCollection<PlayerViewModel> Effects { get; set; } = new ObservableCollection<PlayerViewModel>();
    }

    public class Scene
    {
        public string Name { get; set; }
        public ObservableCollection<TrackSetup> Elements { get; set; } = new ObservableCollection<TrackSetup>();
        public ObservableCollection<TrackSetup> Effects { get; set; } = new ObservableCollection<TrackSetup>();
    }

    public class TrackSetup
    {
        public string Alias { get; set; }
        public string Path { get; set; }
        public bool Enabled { get; set; }
        public decimal Volume { get; set; }
    }
}
