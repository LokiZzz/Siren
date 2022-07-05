using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Siren.ViewModels.SimplePlayer
{
    public class SimplePlayerViewModel : PlayerViewModel
    {
        public SimplePlayerViewModel()
        {
            Tracks = new ObservableCollection<SimpleTrackViewModel>
            {
                new SimpleTrackViewModel { Name = "Track author name — track name"},
                new SimpleTrackViewModel { Name = "Track author name — track name 2", IsSelected = true},
                new SimpleTrackViewModel { Name = "Track author name — track name 3"},
                new SimpleTrackViewModel { Name = "Track author name — track name 4"},
            };

            Test = "TEST";
        }

        private ObservableCollection<SimpleTrackViewModel> _tracks;
        public ObservableCollection<SimpleTrackViewModel> Tracks
        {
            get => _tracks;
            set => SetProperty(ref _tracks, value);
        }

        private SimpleTrackViewModel _currentTrack;
        public SimpleTrackViewModel CurrentTrack
        {
            get => _currentTrack;
            set => SetProperty(ref _currentTrack, value);
        }

        private string _test;
        public string Test
        {
            get => _test;
            set => SetProperty(ref _test, value);
        }
    }

    public class SimpleTrackViewModel : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Name { get; set; }
    }
}
