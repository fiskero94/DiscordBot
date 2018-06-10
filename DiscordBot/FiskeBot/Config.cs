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

        public static char Char(string str) => str.ToCharArray()[0];
        public static string CommandPrefix { get; private set; }
        public static string Token { get; private set; }

        static Config()
        {
            if (!File.Exists(configPath))
            {
                CreateConfig();
                Console.WriteLine("Config created, please insert token and restart the bot.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            ReadConfig();
        }

        private static void CreateConfig()
        {
            ConfigFile config = new ConfigFile();
            foreach (FieldInfo field in config.GetType().GetFields())
            {
                object defaultValue = ((DefaultValueAttribute)field.GetCustomAttribute(typeof(DefaultValueAttribute))).Value;
                object configBoxed = config;
                field.SetValue(configBoxed, defaultValue);
                config = (ConfigFile)configBoxed;
            }
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        private static void ReadConfig()
        {
            string json = File.ReadAllText(configPath);
            ConfigFile config = JsonConvert.DeserializeObject<ConfigFile>(json);
            foreach (FieldInfo field in config.GetType().GetFields())
            {
                PropertyInfo property = typeof(Config).GetProperty(field.Name);
                property.SetValue(null, field.GetValue(config));
            }
        }

        private struct ConfigFile
        {
            [DefaultValue("!")]
            public string CommandPrefix;

            [DefaultValue("None")]
            public string Token;
        }
    }
}