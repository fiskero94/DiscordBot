using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiskeBot
{
    public static class Persistence
    {
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "/persistence.json";

        public static T Retrieve<T>(string field)
        {
            JObject persistence = JObject.Parse(GetFile());
            return persistence[field].ToObject<T>();
        }

        public static void Persist(string field, object obj)
        {
            dynamic persistence = JsonConvert.DeserializeObject(GetFile());
            persistence[field] = JToken.FromObject(obj);
            string file = JsonConvert.SerializeObject(persistence, Formatting.Indented);
            File.WriteAllText(path, file);
        }

        private static string GetFile()
        {
            if (!File.Exists(path))
                File.WriteAllText(path, "{}");
            return File.ReadAllText(path);
        }
    }
}