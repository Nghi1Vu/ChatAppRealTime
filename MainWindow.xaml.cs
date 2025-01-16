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

namespace ChatAppRealTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:32769");
                IDatabase db = redis.GetDatabase();
                db.StringSet("foo", "bar");
                Debug.WriteLine(db.StringGet("foo")); // prints bar
            }
            catch (Exception ex)
            {

            }

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
