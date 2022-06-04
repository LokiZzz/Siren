using Microsoft.Win32.SafeHandles;
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
            StorageFile file = storageFolder
                .CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists)
                .AsTask()
                .Result;

            Windows.Storage.Streams.IRandomAccessStreamWithContentType randomAccessStream = file.OpenReadAsync().AsTask().Result;
            return randomAccessStream.AsStreamForWrite() as FileStream;
        }

        public Stream GetStreamToRead(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.GetFileAsync(Path.GetFileName(filePath)).AsTask().Result;

            return file.OpenStreamForReadAsync().GetAwaiter().GetResult();
        }

        public async Task<Stream> GetStreamToReadAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.GetFileAsync(Path.GetFileName(filePath));
            SafeFileHandle handle = file.CreateSafeFileHandle(); //storageFolder.CreateSafeFileHandle(mode)

            return new FileStream(handle, FileAccess.Read);
        }

        public Stream GetStreamToWrite(string filePath)
        {
            StorageFolder storageFolder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath)).AsTask().Result;
            StorageFile file = storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists).AsTask().Result;

            return file.OpenStreamForWriteAsync().GetAwaiter().GetResult();
        }

        public async Task<Stream> GetStreamToWriteAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists);

            //SafeFileHandle handle = file.CreateSafeFileHandle(access: FileAccess.ReadWrite);
            //return new FileStream(handle, FileAccess.Write);

            return await file.OpenStreamForWriteAsync();
        }
    }
}
