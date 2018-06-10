using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace FiskeBot
{
    public static class Config
    {
        private static readonly string configPath = AppDomain.CurrentDomain.BaseDirectory + "/config.json";

        public static string CommandPrefix { get; private set; }
        public static string Token { get; private set; }

        static Config()
        {
            if (!File.Exists(configPath))
            {
                CreateConfig();
                ReadConfig();
            }
            else ReadConfig();
        }

        private static void CreateConfig()
        {
            ConfigFile config = new ConfigFile();
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                object defaultValue = ((DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute))).Value;
                property.SetValue(config, Convert.ChangeType(defaultValue, property.PropertyType));
            }
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        private static void ReadConfig()
        {
            string json = File.ReadAllText(configPath);
            ConfigFile config = JsonConvert.DeserializeObject<ConfigFile>(json);
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                PropertyInfo publicProperty = typeof(Config).GetProperty(property.Name);
                publicProperty.SetValue(null, Convert.ChangeType(property.GetValue(property), property.PropertyType));
            }
        }

        private struct ConfigFile
        {
            [DefaultValue("!")]
            public string CommandPrefix { get; set; }

            [DefaultValue(null)]
            public string Token { get; set; }
        }
    }
}
