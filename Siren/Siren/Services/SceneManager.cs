using Siren.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Services
{
    public interface ISceneManager
    {
        List<Setting> Settings { get; set; }
    }

    public class SceneManager
    {
        public List<Setting> Settings { get; set; } = new List<Setting>();
    }
}
