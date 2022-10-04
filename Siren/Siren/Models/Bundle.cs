using System;
using System.Collections.Generic;
using System.Text;
using Siren.Utility;

namespace Siren.Models
{
    public class Bundle
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Setting> Settings { get; set; } = new List<Setting>();
        public bool IsActivated { get; set; }
        public long Size { get; set; }
        public string SizeWithSuffix => Size.ToFileSize();
    }

    public class Setting
    {
        public Guid BundleId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<Scene> Scenes { get; set; } = new List<Scene>();
        public List<Track> Elements { get; set; } = new List<Track>();
        public List<Track> Effects { get; set; } = new List<Track>();
        public List<Track> Music { get; set; } = new List<Track>();
    }

    public class Scene
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<TrackSetup> ElementsSetup { get; set; } = new List<TrackSetup>();
        public bool IsMusicEnabled { get; set; }
        public bool IsMusicShuffled { get; set; }
        public bool IsOneMusicTrackRepeatEnabled { get; set; }
        public double MusicVolume { get; set; }
    }

    public class TrackSetup : Track
    {
        public double Volume { get; set; }
    }

    public class Track
    {
        public string Alias { get; set; }
        public string FilePath { get; set; }
    }
}
