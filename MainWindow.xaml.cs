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

namespace ChatAppRealTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : AdonisUI.Controls.AdonisWindow
    {
        private readonly BE.RedisServerIni RedisServerIni;
        private bool isRegister;
        public MainWindow(BE.RedisServerIni redisServerIni, bool isRegister = false)
        {
            this.RedisServerIni = redisServerIni;
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
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            bool login = RedisServerIni.Login(txtac.Text, txtpw.Password);
            if (!login)
            {
                MessageBox.Show("Đăng nhập thất bại!");
                return;
            }
            MessageBox.Show($"Đăng nhập thành công. Xin chào: " + txtac.Text);
            ListUsers lstusers = new ListUsers(this.RedisServerIni);
            lstusers.Show();
            this.Close();
        }
        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {

            if (this.isRegister)
            {
                bool ichk = Helper.Common.isMatch(txtpw.Password, txtpwr.Password);

                if (!ichk)
                {
                    MessageBox.Show("Nhập lại mật khẩu không khớp!");
                    return;
                }
                bool checkacc = RedisServerIni.CheckExistsAcc(txtac.Text);

                if (checkacc)
                {
                    MessageBox.Show("Tài khoản đã tồn tại!");
                    return;
                }
                bool register = RedisServerIni.Register(txtac.Text, txtpw.Password);
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
                MainWindow main = new MainWindow(this.RedisServerIni, true);
                main.ShowDialog();
            }
        }
    }
}
