using Siren.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;

namespace Siren.Services
{
    public interface IBundleService
    {
        Task SaveBundleAsync(Bundle bundle, string filePath);
        Task<Bundle> LoadBundleAsync(string filePath);
    }

    public class BundleService : IBundleService
    {
        public async Task<Bundle> LoadBundleAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task SaveBundleAsync(Bundle bundle, string filePath)
        {
            long sourceSize = 0;
            long destSize = 0;

            IEnumerable<string> elements = bundle.Settings.SelectMany(x => x.Elements).Select(x => x.FilePath);
            IEnumerable<string> effects = bundle.Settings.SelectMany(x => x.Effects).Select(x => x.FilePath);
            IEnumerable<string> settingsImages = bundle.Settings.Select(x => x.ImagePath);
            IEnumerable<string> scenesImages = bundle.Settings.SelectMany(x => x.Scenes).Select(x => x.ImagePath);

            List<string> allFiles = elements.Union(effects).Union(settingsImages).Union(scenesImages).ToList();

            IFileStreamProvider fileStreamProvider = DependencyService.Resolve<IFileStreamProvider>();

            using (Stream fsTarget = await fileStreamProvider.GetFileStreamToWrite(filePath))
            {
                foreach(string file in allFiles)
                {
                    using (Stream fsSource = await fileStreamProvider.GetFileStreamToRead(filePath))
                    {
                        sourceSize += fsSource.Length;

                        using (GZipStream compressionStream = new GZipStream(fsTarget, CompressionLevel.Optimal))
                        {
                            await fsTarget.CopyToAsync(compressionStream);
                        }
                    }
                }

                destSize = fsTarget.Length;
            }
        }
    }
}
