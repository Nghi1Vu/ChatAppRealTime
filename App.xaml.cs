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
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
		}
		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			// Hiển thị thông báo lỗi (hoặc ghi log)
			MessageBox.Show($"Có lỗi xảy ra: {e.Exception.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

			// Đánh dấu là đã xử lý lỗi để tránh crash app
			e.Handled = true;
		}
		private void Application_Startup(object sender, StartupEventArgs e)
		{

		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window win)
            {
                win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

    }
}
