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
using static ChatAppRealTime.Model;

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
            this.RedisServerIni = redisServerIni;
            InitializeComponent();
            ldChatRoom();
            this.RedisServerIni.Subscriber(new Action<RedisValue>((x) =>
            {

                this.Dispatcher.Invoke(() =>
                {
                    var obj = JsonConvert.DeserializeObject<dynamic>(x);

                    /*txtreply.Text = obj?.message*/
                });
            }));
        }
        private void ldChatRoom()
        {
           var messages = this.RedisServerIni.FTSearch("idx:messages", "");
            int row = 0;
            var data = messages.Select(x => new ChatroomModel()
            {
                from = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).from,
                date = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).date,
                message = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).message
            });
            foreach (var item in data)
            {
                grdchat.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, MaxHeight = 100 });
                //img element
                Image uIElementImg = new Image();
                uIElementImg.SetValue(Grid.ColumnProperty, item.from == this.RedisServerIni.currentusr ? 3 : 0);
                uIElementImg.SetValue(Grid.RowProperty, row);
                uIElementImg.SetValue(Image.HeightProperty, (double)50);
                uIElementImg.Source = new BitmapImage(
    new Uri(@"/ChatAppRealTime;component/img/OIP.jpg", UriKind.Relative));
                TextBlock uIElementUsr = new TextBlock();
                uIElementUsr.SetValue(Grid.ColumnProperty, item.from == this.RedisServerIni.currentusr ? 3 : 0);
                uIElementUsr.SetValue(Grid.RowProperty, row);
                uIElementUsr.SetValue(TextBlock.TextProperty, item.from);
                uIElementUsr.SetValue(TextBlock.MarginProperty, new Thickness(5,50,0,0));
                //end
                //info element
                Button uIElementInfo = new Button();
                uIElementInfo.SetValue(Grid.ColumnProperty, item.from == this.RedisServerIni.currentusr ? 4 : 1);
                uIElementInfo.SetValue(Grid.RowProperty, row);
                uIElementInfo.SetValue(Button.ContentProperty, item.message);
                //end
                row++;
                grdchat.Children.Add(uIElementImg);
                grdchat.Children.Add(uIElementUsr);
                grdchat.Children.Add(uIElementInfo);
            }
        }
        private void btnsend_Click(object sender, RoutedEventArgs e)
        {
            this.RedisServerIni.Publish(txtchat.Text);
            ldChatRoom();
        }

        private void txtchat_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (txtchat.Text == "Bắt đầu trò chuyện...")
            {
                txtchat.Text = "";
            }
        }
    }
}
