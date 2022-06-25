using Microsoft.Win32.SafeHandles;
using Siren.Services;
using Siren.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Xamarin.Essentials;
using Xamarin.Forms;
using static System.Net.WebRequestMethods;

[assembly: Dependency(typeof(FileManager))]
namespace Siren.UWP.Services
{
    public class FileManager : IFileManager
    {
        public async Task CreateFolderIfNotExistsAsync(string folderPath, string folderName)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            await storageFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            IStorageItem item = await storageFolder.TryGetItemAsync(Path.GetFileName(filePath));

            if (item != null)
            {
                await item.DeleteAsync();
            }
        }

        public async Task DeleteFolderAsync(string folderPath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);

            await storageFolder.DeleteAsync();
        }

        public async ValueTask<Stream> GetStreamToReadAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.GetFileAsync(Path.GetFileName(filePath));

            return await file.OpenStreamForReadAsync();
        }

        public async ValueTask<Stream> GetStreamToWriteAsync(string filePath)
        {
            StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(filePath));
            StorageFile file = await storageFolder.CreateFileAsync(Path.GetFileName(filePath), CreationCollisionOption.OpenIfExists);

            return await file.OpenStreamForWriteAsync();
        }

        public async Task<bool> TestFileManagerAsync()
        {
            try
            {
                string fileName = "permission_test.txt";
                string fileContent = "Swift as a shadow...";

                //Create
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path));
                StorageFile createFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                //Write
                await FileIO.WriteTextAsync(createFile, fileContent);

                //Read
                StorageFile readFile = await storageFolder.GetFileAsync(fileName);
                string text = await Windows.Storage.FileIO.ReadTextAsync(readFile);
                if (!text.Equals(fileContent))
                {
                    throw new Exception("Test file content is not valid.");
                }

                //Delete
                StorageFile deleteFile = await storageFolder.GetFileAsync(fileName);
                await deleteFile.DeleteAsync();

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}
