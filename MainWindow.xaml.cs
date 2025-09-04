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
using System.Timers;
using NetTopologySuite.Index.HPRtree;

namespace ChatAppRealTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
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
            }
            else
            {
                txtpwr.Visibility = Visibility.Hidden;
                btnReturnLogin.Visibility = Visibility.Hidden;
            }
            try
            {
                //              Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                //Wpf.Ui.Controls.WindowBackdropType.Mica,  // Background type
                //true);

            }
            catch (Exception ex)
            {

            }
        }
        public async Task OnOpenCustomMessageBox(string content, string title = "App thông báo!")
        {
            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = title,
                Content =
                    content,
                
            };

            _ = await uiMessageBox.ShowDialogAsync();
        }
        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if ((new string[2] { txtac.Text, txtpw.Password }).Any(x => x == ""))
            {
                await OnOpenCustomMessageBox("Tài khoản và mật khẩu không được để trống!");     
                return;
            }
            bool login = Constant.RedisServerIni.Login(txtac.Text, txtpw.Password);
            if (!login)
            {
                await OnOpenCustomMessageBox("Đăng nhập thất bại!");
                return;
            }
            await Constant.RedisServerIni.Heartbeat(Constant.RedisServerIni.currentusr);
            //heartbeat
            _timer = new Timer(60000); // Gửi heartbeat mỗi 10 giây
            _timer.Elapsed += async (sender, e) => await Constant.RedisServerIni.Heartbeat(Constant.RedisServerIni.currentusr);
            _timer.AutoReset = true;
            _timer.Start();
            Menu menu = new Menu();
            menu.Show();
            this.Close();

            //end

        }
        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {

            if (this.isRegister)
            {
                if ((new string[2] { txtpwr.Password, txtpw.Password }).Any(x => x == ""))
                {
                    await OnOpenCustomMessageBox("Mật khẩu và nhập lại mật khẩu không được để trống!");
                    return;
                }
                bool ichk = Helper.Common.isMatch(txtpw.Password, txtpwr.Password);

                if (!ichk)
                {
                    await OnOpenCustomMessageBox("Nhập lại mật khẩu không khớp!");
                    return;
                }
                bool checkacc = Constant.RedisServerIni.FTSearch("idx:users", $"@username:{txtac.Text}").Any();

                if (checkacc)
                {
                    await OnOpenCustomMessageBox("Tài khoản đã tồn tại!");
                    return;
                }
                bool register = Constant.RedisServerIni.Register(txtac.Text, txtpw.Password);
                if (!register)
                {
                    await OnOpenCustomMessageBox("Đăng ký thất bại!");
                    return;
                }
                await OnOpenCustomMessageBox($"Đăng ký thành công. Xin chào: " + txtac.Text);
                MainWindow main = new MainWindow(false);
                main.Show();
                this.Close();
            }
            else
            {
                MainWindow main = new MainWindow(true);
                main.Show();
                this.Close();
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

        private void ButtonReturnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow(false);
            main.Show();
            this.Close();
        }
    }
}
