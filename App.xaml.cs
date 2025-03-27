using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatAppRealTime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Environment.SetEnvironmentVariable("AI_API_KEY", Constant.AppSetting["AI_API_KEY"]); 
            var redisServer=Constant.RedisServerIni;
        }

		private void Application_Startup(object sender, StartupEventArgs e)
		{

		}
	}
}
