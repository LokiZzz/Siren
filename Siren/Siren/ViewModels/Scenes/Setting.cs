using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Siren.ViewModels
{
    public class Setting : BaseViewModel
    {
        public bool _isSelected;
        public bool IsSelected 
        { 
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Name { get; set; }
        public ObservableCollection<Scene> Scenes { get; set; } = new ObservableCollection<Scene>();
        public ObservableCollection<ElementPlayerViewModel> Elements { get; set; } = new ObservableCollection<ElementPlayerViewModel>();
        public ObservableCollection<PlayerViewModel> Effects { get; set; } = new ObservableCollection<PlayerViewModel>();
    }

    public class Scene : BaseViewModel
    {
        public bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Name { get; set; }
        public ObservableCollection<TrackSetup> Elements { get; set; } = new ObservableCollection<TrackSetup>();
        public ObservableCollection<TrackSetup> Effects { get; set; } = new ObservableCollection<TrackSetup>();
    }

    public class TrackSetup : BaseViewModel
    {
        public string Alias { get; set; }
        public string Path { get; set; }
        public double Volume { get; set; }
    }
}
