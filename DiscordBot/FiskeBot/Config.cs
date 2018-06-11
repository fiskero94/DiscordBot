using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

namespace FiskeBot
{
    public static class Config
    {
        private static readonly string path = AppDomain.CurrentDomain.BaseDirectory + "/config.json";
        public static char Char(this string str) => str.ToCharArray()[0];
        
        public static string CommandPrefix { get; private set; }
        public static string Token { get; private set; }
        public static string GoogleApiKey { get; private set; }
        public static string SearchEngineID { get; private set; }

        static Config()
        {
            if (!File.Exists(path))
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
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                object defaultValue = ((DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute))).Value;
                property.SetValue(config, defaultValue);
            }
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private static void ReadConfig()
        {
            string json = File.ReadAllText(path);
            ConfigFile config = JsonConvert.DeserializeObject<ConfigFile>(json);
            ValidateConfig(config);
            foreach (PropertyInfo property in config.GetType().GetProperties())
            {
                PropertyInfo publicProperty = typeof(Config).GetProperty(property.Name);
                publicProperty.SetValue(null, property.GetValue(config));
            }
        }

        private static void ValidateConfig(ConfigFile config)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(config, new ValidationContext(config), results, true))
            {
                Console.WriteLine("Config contains the following errors:" + Environment.NewLine);
                foreach (ValidationResult result in results)
                    Console.WriteLine(result.ErrorMessage);
                Console.ReadKey();
                Environment.Exit(1);
            }
        }

        private class ConfigFile
        {
            [Required]
            [StringLength(1)]
            [DefaultValue("!")]
            public string CommandPrefix { get; set; }

            [Required]
            [StringLength(59, MinimumLength = 59)]
            [DefaultValue("None")]
            public string Token { get; set; }
            
            [StringLength(39, MinimumLength = 39)]
            [DefaultValue(null)]
            public string GoogleApiKey { get; set; }

            [StringLength(33, MinimumLength = 33)]
            [DefaultValue(null)]
            public string SearchEngineID { get; set; }
        }
    }
}