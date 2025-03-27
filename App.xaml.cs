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
<<<<<<< HEAD
            Environment.SetEnvironmentVariable("AI_API_KEY", Constant.AppSetting["AI_API_KEY"]); 
            var redisServer=Constant.RedisServerIni;
=======
																																																									 //add
			var redisServer=Constant.RedisServerIni;
>>>>>>> 87e4e85ff8fa6d992739b8b570e06eddf3dcb3af
        }

		private void Application_Startup(object sender, StartupEventArgs e)
		{

		}
	}
}
