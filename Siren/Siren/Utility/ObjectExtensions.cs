using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Siren.Utility
{
    public static class ObjectExtensions
    {
        public static T GetDeepCopy<T>(this T objToCopy) where T : class
        {
            string serialized = JsonConvert.SerializeObject(objToCopy);

            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
