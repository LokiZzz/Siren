using Newtonsoft.Json;
using Siren.Services;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Siren.Utility
{
    public static class LocalDataHelper
    {
        public static async Task<T> GetObjectFromLocalAppFile<T>(string fileName) where T : class
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);

            try
            {
                IFileManager fileManager = DependencyService.Resolve<IFileManager>();

                using (Stream stream = await fileManager.GetStreamToRead(path))
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    string content = Encoding.UTF8.GetString(buffer);

                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch(FileNotFoundException)
            {
                return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public static async Task WriteToTheLocalAppFile<T>(T objectToWrite, string filePath) where T : class
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                string content = JsonConvert.SerializeObject(objectToWrite);
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);

                IFileManager fileManager = DependencyService.Resolve<IFileManager>();

                await fileManager.DeleteFileAsync(path);

                byte[] buffer = Encoding.UTF8.GetBytes(content);

                using (Stream stream = await fileManager.GetStreamToWrite(path))
                {
                    await stream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            finally 
            { 
                _semaphoreSlim.Release(); 
            }
        }

        public static async Task OpenAppDataFolder()
        {
            IFileManager fileManager = DependencyService.Resolve<IFileManager>();
            await fileManager.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }
    }
}
