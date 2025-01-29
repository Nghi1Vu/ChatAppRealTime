using Newtonsoft.Json;
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
using System.Windows.Shapes;

namespace ChatAppRealTime
{
    /// <summary>
    /// Interaction logic for ChatRoom.xaml
    /// </summary>
    public partial class ChatRoom : Window
    {
        private readonly BE.RedisServerIni RedisServerIni;

        public ChatRoom(BE.RedisServerIni redisServerIni)
        {
            this.RedisServerIni= redisServerIni; 
            InitializeComponent();
            this.RedisServerIni.Subscriber(new Action<RedisValue>((x) =>
            {
                var obj=JsonConvert.DeserializeObject<dynamic>(x);
                this.Dispatcher.Invoke(()=> txtreply.Text = obj?.message);
            }));
        }

        private void btnsend_Click(object sender, RoutedEventArgs e)
        {
            this.RedisServerIni.Publish(txtchat.Text);
        }
    }
}
