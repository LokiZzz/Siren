using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Forms;
using static System.Net.WebRequestMethods;

[assembly: Dependency(typeof(FileStreamProvider))]
namespace Siren.UWP.Services
{
    public class FileStreamProvider : IFileStreamProvider
    {
        public Stream GetFileStreamToRead(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.GetFileAsync(Path.GetFileName(filePath)).AsTask().Result;

            return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
        }

        public Stream GetFileStreamToWrite(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.ReplaceExisting).AsTask().Result;

            return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
        }
    }
}
