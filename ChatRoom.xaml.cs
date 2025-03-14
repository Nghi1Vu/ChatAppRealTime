using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private List<string> lstonline;
        public ChatRoom()
        {
            InitializeComponent();
            ldChatRoom();
            lstonline = ldlstOnline().Result;
			Constant.RedisServerIni.HeartbeatTTL(new Action<string>(async (message) =>
            {
                string key = message.ToString();

                // Chỉ xử lý key liên quan đến trạng thái user
                if (key.StartsWith("user_status:"))
                {
                    string userId = key.Replace("user_status:", "");
                    this.Dispatcher.Invoke(() =>
                    {
                        if (lstonline.Contains(userId))
                        {
                            lstonl.Items.Remove(userId);
                            lstonline.Remove(userId);
                        }
                    });
                }
                if (key.StartsWith("user_login:"))
                {
                    string userId = key.Replace("user_login:", "");
                    this.Dispatcher.Invoke(() =>
                    {
                        if (!lstonline.Contains(userId))
                        {
                            lstonl.Items.Add(userId);
                            lstonline.Add(userId);
                        }
                    });
                }
            }));
            Constant.RedisServerIni.Subscriber(new Action<RedisValue>((x) =>
            {

                this.Dispatcher.Invoke(() =>
                {
                    var item = JsonConvert.DeserializeObject<ChatroomModel>(x);
                    UIElement[] uIElement = new UIElement[grdchat.Children.Count];
                    grdchat.Children.CopyTo(uIElement, 0);
                    int row = int.Parse(uIElement.LastOrDefault()?.GetValue(Grid.RowProperty)?.ToString() ?? "0") + 1;
                    createGrdChat(ref row, item);
                    sclchat.ScrollToEnd();
                });
            }));
        }
        private void createGrdChat(ref int row, ChatroomModel item)
        {
            grdchat.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, MaxHeight = 100 });
            //img element
            Image uIElementImg = new Image();
            uIElementImg.SetValue(Grid.ColumnProperty, item.from == Constant.RedisServerIni.currentusr ? 3 : 0);
            uIElementImg.SetValue(Grid.RowProperty, row);
            uIElementImg.SetValue(Image.HeightProperty, (double)50);
            uIElementImg.Source = new BitmapImage(
new Uri(@"/ChatAppRealTime;component/img/OIP.jpg", UriKind.Relative));
            TextBlock uIElementUsr = new TextBlock();
            uIElementUsr.SetValue(Grid.ColumnProperty, item.from == Constant.RedisServerIni.currentusr ? 3 : 0);
            uIElementUsr.SetValue(Grid.RowProperty, row);
            uIElementUsr.SetValue(TextBlock.TextProperty, item.from);
            uIElementUsr.SetValue(TextBlock.MarginProperty, new Thickness(5, 50, 0, 0));

            TextBlock uIElementTime = new TextBlock();
            uIElementTime.SetValue(Grid.ColumnProperty, 2);
            uIElementTime.SetValue(Grid.RowProperty, row);
            uIElementTime.SetValue(TextBlock.MarginProperty, new Thickness(0, 25, 0, 0));
            uIElementTime.SetValue(TextBlock.FontSizeProperty, (double)10);
            uIElementTime.SetValue(TextBlock.TextProperty, JsonConvert.DeserializeObject<DateTime>("\"" + item.date.Split(":")[0] + ":" + item.date.Split(":")[1] + "\"").ToString("dd/MM/yyyy hh:mm tt"));
            //end
            //info element
            Button uIElementInfo = new Button();
            uIElementInfo.SetValue(Grid.ColumnProperty, item.from == Constant.RedisServerIni.currentusr ? 4 : 1);
            uIElementInfo.SetValue(Grid.RowProperty, row);
            uIElementInfo.SetValue(Button.ContentProperty, item.message);
            //end
            row++;
            grdchat.Children.Add(uIElementImg);
            grdchat.Children.Add(uIElementUsr);
            grdchat.Children.Add(uIElementInfo);
            grdchat.Children.Add(uIElementTime);
        }
        private void ldChatRoom()
        {
            var messages = Constant.RedisServerIni.FTSearch("idx:messages", "");
            int row = 0;
            var data = messages.Select(x => new ChatroomModel()
            {
                from = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).from,
                date = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).date,
                message = JsonConvert.DeserializeObject<ChatroomModel>(x.ToString()).message
            });
            data = data.OrderBy(x => JsonConvert.DeserializeObject<DateTime>("\"" + x.date.Split(":")[0] + ":" + x.date.Split(":")[1] + "\"")).ToList();
            foreach (var item in data)
            {
                createGrdChat(ref row, item);
            }
            sclchat.ScrollToEnd();
        }
        private async Task<List<string>> ldlstOnline()
        {
            List<string> strings = new List<string>();
            var lstonline = Constant.RedisServerIni.FTSearch("idx:users", "");
            var data = lstonline.Select(x => new AccountInfo()
            {
                username = JsonConvert.DeserializeObject<AccountInfo>(x.ToString()).username,
                password = JsonConvert.DeserializeObject<AccountInfo>(x.ToString()).password
            });
            foreach (var item in data)
            {
                bool chk = await Constant.RedisServerIni.GetOnlineByUser(item.username);
                if (chk)
                {
                    lstonl.Items.Add(item.username);
                    strings.Add(item.username);
                }
            }
            return strings;
        }
        private void btnsend_Click(object sender, RoutedEventArgs e)
        {
            Constant.RedisServerIni.Publish(txtchat.Text);
        }

        private void txtchat_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtchat.Text.Trim() == "Nhấn để chat..")
            {
                txtchat.Text = "";
            }
        }

        private void txtchat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtchat.Text.Trim() == "")
            {
                txtchat.Text = "Nhấn để chat..";
            }
        }

		private void txtchat_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != System.Windows.Input.Key.Enter) return;
			e.Handled = true;
			Constant.RedisServerIni.Publish(txtchat.Text);
            //MessageBox.Show("Enter pressed");
            txtchat.Text = string.Empty;
		}
	}
}
