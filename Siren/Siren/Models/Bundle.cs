﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Models
{
    public class Bundle
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Setting> Settings { get; set; }
        public bool IsActivated { get; set; }
    }

    public class Setting
    {
        public Guid BundleId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<Track> Elements { get; set; }
        public List<Track> Effects { get; set; }
    }

    public class Scene
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<TrackSetup> ElementsSetup { get; set; }
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
