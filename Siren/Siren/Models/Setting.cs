using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Models
{
    public class Setting
    {
        public string Name { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<Track> Elements { get; set; }
        public List<Track> Effects { get; set; }
    }

    public class Scene
    {
        public string Name { get; set; }
        public List<TrackSetup> Elements { get; set; }
        public List<TrackSetup> Effects { get; set; }
    }

    public class TrackSetup
    {
        public Track Track { get; set; }
        public bool Enabled { get; set; }
        public decimal Volume { get; set; }
    }

    public class Track
    {
        public string Alias { get; set; }
        public string Path { get; set; }
    }
}
