using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace ChatAppRealTime
{
    internal static class Constant
    {
        public static IConfiguration AppSetting = GetAppSettings();
        public static readonly BE.RedisServerIni RedisServerIni = new BE.RedisServerIni();
        static IConfiguration GetAppSettings()
        {
            // Build a configuration object from JSON file
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            // Get a configuration section
            AppSetting = config;

            return AppSetting;
        }
    }
}
