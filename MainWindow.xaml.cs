using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using System.Diagnostics;
using AdonisUI;
using System.Timers;
using NetTopologySuite.Index.HPRtree;

namespace ChatAppRealTime
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>

	public partial class MainWindow : AdonisUI.Controls.AdonisWindow
	{
		private bool isRegister;
		private Timer _timer;
		public MainWindow() : this(false)
		{
		}
		public MainWindow(bool isRegister = false)
		{
			this.isRegister = isRegister;
			InitializeComponent();
			if (this.isRegister)
			{
				btnLogin.IsEnabled = false;
				btnLogin.Visibility = Visibility.Hidden;
				btnRegister.SetValue(Grid.ColumnProperty, 0);
				btnRegister.SetValue(Grid.ColumnSpanProperty, 2);
				txtpwr.Visibility = Visibility.Visible;
				lblpwr.Visibility = Visibility.Visible;
			}
			else
			{
				txtpwr.Visibility = Visibility.Hidden;
				lblpwr.Visibility = Visibility.Hidden;
			}
			try
			{
				AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.LightColorScheme);
			}
			catch (Exception ex)
			{

			}
		}
		private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
		{
			if ((new string[2] { txtac.Text, txtpw.Password }).Any(x => x == ""))
			{
				MessageBox.Show("Tài khoản và mật khẩu không được để trống!");
				return;
			}
			bool login = Constant.RedisServerIni.Login(txtac.Text, txtpw.Password);
			if (!login)
			{
				MessageBox.Show("Đăng nhập thất bại!");
				return;
			}
			await Constant.RedisServerIni.Heartbeat(Constant.RedisServerIni.currentusr);
			//heartbeat
			_timer = new Timer(60000); // Gửi heartbeat mỗi 10 giây
			_timer.Elapsed += async (sender, e) => await Constant.RedisServerIni.Heartbeat(Constant.RedisServerIni.currentusr);
			_timer.AutoReset = true;
			_timer.Start();
			ChatRoom room = new ChatRoom();
			room.Show();
			this.Close();

			//end

		}
		private void ButtonRegister_Click(object sender, RoutedEventArgs e)
		{

			if (this.isRegister)
			{
				if ((new string[2] { txtpwr.Password, txtpw.Password }).Any(x => x == ""))
				{
					MessageBox.Show("Mật khẩu và nhập lại mật khẩu không được để trống!");
					return;
				}
				bool ichk = Helper.Common.isMatch(txtpw.Password, txtpwr.Password);

				if (!ichk)
				{
					MessageBox.Show("Nhập lại mật khẩu không khớp!");
					return;
				}
				bool checkacc = Constant.RedisServerIni.FTSearch("idx:users", $"@username:{txtac.Text}").Any();

				if (checkacc)
				{
					MessageBox.Show("Tài khoản đã tồn tại!");
					return;
				}
				bool register = Constant.RedisServerIni.Register(txtac.Text, txtpw.Password);
				if (!register)
				{
					MessageBox.Show("Đăng ký thất bại!");
					return;
				}
				MessageBox.Show($"Đăng ký thành công. Xin chào: " + txtac.Text);
				this.Close();
			}
			else
			{
				MainWindow main = new MainWindow(true);
				main.ShowDialog();
			}
		}

		private void AdonisWindow_KeyUp(object sender, KeyEventArgs e)
		{
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    {
                        e.Handled = true;

                        if (isRegister)
						{
                            ButtonRegister_Click(sender, e);

                        }
						else
						{
                            ButtonLogin_Click(sender, e);
                        }               

                    }
                    break;
                default:
                    break;
            }
		}
	}
}
