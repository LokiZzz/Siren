using Siren.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Siren.Services
{
    public class SceneManager
    {
        public Setting SelectedSetting { get; set; }
        public List<Setting> Settings { get; private set; } = new List<Setting>();

        public void AddSetting(Setting setting)
        {
            Settings.Add(setting);

            MessagingCenter.Send(this, SceneManagerMessages.SettingAdded);
        }

        public void AddScene(Scene scene)
        {
            SelectedSetting.Scenes.Add(scene);
        }
    }

    public static class SceneManagerMessages
    {
        public static string SettingAdded = nameof(SettingAdded);
    }
}
