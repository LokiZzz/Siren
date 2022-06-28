using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Utility
{
    public static class CommonExtensions
    {
        public static T GetDeepCopy<T>(this T objToCopy) where T : class
        {
            string serialized = JsonConvert.SerializeObject(objToCopy);

            return JsonConvert.DeserializeObject<T>(serialized);
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ToFileSize(this long value, int decimalPlaces = 1)
        {
            if (value < 0) 
            { 
                return "-" + ToFileSize(-value, decimalPlaces); 
            }

            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }
    }
}
