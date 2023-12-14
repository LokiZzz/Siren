using Siren.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Siren.ViewModels
{
    public static class UsefullExtensions
    {
        public static bool CanDeleteSettingAsset(
            this IEnumerable<SettingViewModel> allSettings, 
            SettingViewModel settingDeleteFrom, 
            string fileToDeletePath)
        {
            IEnumerable<SettingViewModel> otherSettings = allSettings.Where(x => x != settingDeleteFrom);

            bool settingHaveSameFileInDifferentCategories =
                settingDeleteFrom.GetAllSettingsFiles().Where(x => x == fileToDeletePath).Count() > 1;

            return !otherSettings.Any(x => x.Elements.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.Effects.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.Music.Any(y => y.FilePath == fileToDeletePath))
                && !otherSettings.Any(x => x.ImagePath == fileToDeletePath)
                && !otherSettings.Any(x => x.Scenes.Any(y => y.ImagePath == fileToDeletePath))
                && !settingHaveSameFileInDifferentCategories;
        }

        public static IEnumerable<string> GetAllSettingsFiles(this SettingViewModel setting)
        {
            return setting.Elements.Select(x => x.FilePath)
                .Concat(setting.Effects.Select(x => x.FilePath))
                .Concat(setting.Music.Select(x => x.FilePath))
                .Concat(setting.Scenes.Select(x => x.ImagePath))
                .Concat(new List<string> { setting.ImagePath })
                .Where(x => !string.IsNullOrEmpty(x));
        }

        public static async Task<Stream> GetStream(this string path)
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            MemoryStream memoryStream = new MemoryStream();

            Stream sourceStream = await fileManager.GetStreamToRead(path);
            sourceStream.CopyTo(memoryStream);
            sourceStream.Close();
            sourceStream.Dispose();
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}
