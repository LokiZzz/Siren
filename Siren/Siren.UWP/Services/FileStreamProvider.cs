using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Essentials;
using Xamarin.Forms;
using static System.Net.WebRequestMethods;

[assembly: Dependency(typeof(FileStreamProvider))]
namespace Siren.UWP.Services
{
    public class FileStreamProvider : IFileStreamProvider
    {
        public FileStream GetFileStreamToRead(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.GetFileAsync(Path.GetFileName(filePath)).AsTask().Result;

            Windows.Storage.Streams.IRandomAccessStreamWithContentType randomAccessStream = file.OpenReadAsync().AsTask().Result;
            return randomAccessStream.AsStreamForRead() as FileStream;
        }

        public FileStream GetFileStreamToWrite(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.GetFileAsync(Path.GetFileName(filePath)).AsTask().Result;

            Windows.Storage.Streams.IRandomAccessStreamWithContentType randomAccessStream = file.OpenReadAsync().AsTask().Result;
            return randomAccessStream.AsStreamForWrite() as FileStream;
        }

        public Stream GetStreamToRead(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.GetFileAsync(Path.GetFileName(filePath)).AsTask().Result;

            return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
        }

        public Stream GetStreamToWrite(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists).AsTask().Result;

            return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
        }
    }
}
