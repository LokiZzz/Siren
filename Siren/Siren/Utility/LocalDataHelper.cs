using Newtonsoft.Json;
using Siren.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

                using (Stream stream = await fileManager.GetStreamToReadAsync(path))
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    string content = Encoding.UTF8.GetString(buffer);

                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch(FileNotFoundException ex)
            {
                return null;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static async Task WriteToTheLocalAppFile<T>(T objectToWrite, string filePath) where T : class
        {
            string content = JsonConvert.SerializeObject(objectToWrite);
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);

            IFileManager fileManager = DependencyService.Resolve<IFileManager>();

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            using (Stream stream = await fileManager.GetStreamToWriteAsync(path))
            {
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
