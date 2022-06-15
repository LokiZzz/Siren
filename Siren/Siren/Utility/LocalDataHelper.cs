using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Siren.Utility
{
    public static class LocalDataHelper
    {
        public static T GetObjectFromLocalAppFile<T>(string fileName) where T : class
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<T>(content);
            }
            else
            {
                return null;
            }
        }

        public static void WriteToTheLocalAppFile(object objectToWrite, string filePath)
        {
            string content = JsonConvert.SerializeObject(objectToWrite);

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);
            File.WriteAllText(path, content);
        }
    }
}
