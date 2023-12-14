using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Siren.ViewModels
{
    public static class SettingsExtensions
    {
        public static bool CanDeleteSettingAsset(
            this IEnumerable<SettingViewModel> allSettings, 
            SettingViewModel settingDeleteFrom, 
            string fileToDeletePath)
        {
            IEnumerable<SettingViewModel> otherSettings = allSettings.Where(x => x != settingDeleteFrom);

            return !otherSettings.Any(x => x.Elements.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.Effects.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.Music.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.ImagePath == fileToDeletePath)
                && !otherSettings.Any(x => x.Scenes.Any(y => y.ImagePath == fileToDeletePath));
        }

        public static IEnumerable<string> GetAllSettingsFiles(this SettingViewModel setting)
        {
            return setting.Elements.Select(x => x.FilePath)
                .Union(setting.Effects.Select(x => x.FilePath))
                .Union(setting.Music.Select(x => x.FilePath))
                .Union(setting.Scenes.Select(x => x.ImagePath))
                .Union(new List<string> { setting.ImagePath })
                .Where(x => !string.IsNullOrEmpty(x));
        }
    }
}
