using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NRedisStack.Search.Aggregation;
using OpenAI;
using OpenAI.Chat;
using StackExchange.Redis;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
    public partial class ChatAi : Window
    {
        string modelai = "meta-llama/Llama-3.3-70B-Instruct-Turbo";
        string key_session { get; set; }
        public ChatAi()
        {
            InitializeComponent();
            ldChatHistory();
            dsHistory();
        }
        private void dsHistory()
        {
            var _key_session = Constant.RedisServerIni.FTSelectOne("idx:aihistories", "", "key_session");
            var data = _key_session.Select(x => new ChatAiModel()
            {
                message = x[3].ToString(),
                key_session = x[1].ToString(),
                timestamp = long.Parse(x[5].ToString() ?? "0")
            });
            lsthistory.ItemsSource = null;
            lsthistory.ItemsSource = data.OrderByDescending(x => x.timestamp);
        }
        private void createGrdChat(ref int row, ChatroomModel item, int type) //type==1 user, type==2 bot
        {
            DateTime outDate;
            grdchat.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            //img element
            Image uIElementImg = new Image();
            uIElementImg.SetValue(Grid.ColumnProperty, type == 1 ? 3 : 0);
            uIElementImg.SetValue(Grid.RowProperty, row);
            uIElementImg.SetValue(Image.HeightProperty, (double)50);
            uIElementImg.Source = new BitmapImage(
new Uri(type == 2 ? @"/ChatAppRealTime;component/img/chatbot.jpg" : @"/ChatAppRealTime;component/img/OIP.jpg", UriKind.Relative));
            TextBlock uIElementUsr = new TextBlock();
            uIElementUsr.SetValue(Grid.ColumnProperty, type == 1 ? 3 : 0);
            uIElementUsr.SetValue(Grid.RowProperty, row);
            uIElementUsr.SetValue(TextBlock.TextProperty, item.from);
            uIElementUsr.SetValue(TextBlock.MarginProperty, new Thickness(5, 50, 0, 0));

            TextBlock uIElementTime = new TextBlock();
            uIElementTime.SetValue(Grid.ColumnProperty, 2);
            uIElementTime.SetValue(Grid.RowProperty, row);
            uIElementTime.SetValue(TextBlock.MarginProperty, new Thickness(0, 25, 0, 0));
            uIElementTime.SetValue(TextBlock.FontSizeProperty, (double)10);
            uIElementTime.SetValue(TextBlock.TextProperty, DateTime.TryParse(item.date, out outDate) ? outDate.ToString("dd/MM/yyyy hh:mm tt") : JsonConvert.DeserializeObject<DateTime>("\"" + item.date.Split(":")[0] + ":" + item.date.Split(":")[1] + "\"").ToString("dd/MM/yyyy hh:mm tt"));
            //end
            //info element
            TextBlock uIElementInfo = new TextBlock();
            uIElementInfo.SetValue(Grid.ColumnProperty, type == 1 ? 4 : 1);
            uIElementInfo.SetValue(Grid.RowProperty, row);
            //uIElementInfo.Background = new SolidColorBrush(Colors.AliceBlue);
            uIElementInfo.TextWrapping = TextWrapping.Wrap;
            uIElementInfo.Padding = new Thickness(10);
            uIElementInfo.SetValue(TextBlock.TextProperty, item.message);
            //end
            grdchat.Children.Add(uIElementImg);
            grdchat.Children.Add(uIElementUsr);
            grdchat.Children.Add(uIElementInfo);
            grdchat.Children.Add(uIElementTime);
            row++;
        }
        private void ldChatHistory()
        {
            grdchat.Children.Clear();
            var messages = Constant.RedisServerIni.FTSearch("idx:aihistories", "");
            int row = 0;
            var data = messages.Select(x => new ChatAiModel()
            {
                from = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).from,
                date = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).date,
                message = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).message,
                type = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).type,
                key_session = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).key_session,
                timestamp = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).timestamp,
            });
            data = data.Where(x => (x.from == Constant.RedisServerIni.currentusr || x.from == modelai) && x.key_session == (key_session ?? "")).OrderBy(x => x.timestamp).ToList();
            foreach (var item in data)
            {
                createGrdChat(ref row, item, item.type);
            }
            sclchat.ScrollToEnd();
        }
        public async Task SendFunc()
        {
            if (string.IsNullOrEmpty(txtchat.Text))
            {
                return;
            }
            string ApiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
            if (string.IsNullOrEmpty(ApiKey))
            {
                MessageBox.Show("Lỗi hệ thống! Vui lòng liên hệ với nhà phát triển");
                return;
            }
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.together.xyz");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
            var req = await client.PostAsJsonAsync("/v1/chat/completions", new
            {
                model = modelai,
                messages = ChatBotsHistory(new ChatBotsHistoryModel() { role = "user", content = txtchat.Text })
            });
            req.EnsureSuccessStatusCode();
            var res = await req.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ChatAiResponseModel>(res);
            key_session = key_session ?? Guid.NewGuid().ToString();
            var msai = new MessageAiModel()
            {
                key_session = key_session,
                from = Constant.RedisServerIni.currentusr,
                message = txtchat.Text,
                modelai = modelai,
                messageai = result?.choices.FirstOrDefault()?.message?.content ?? ""
            };
            bool kq = Constant.RedisServerIni.SaveMessageAi(msai);
            if (kq)
            {
                ChatroomModel chataiModel = new ChatroomModel() { date = DateTime.Now.ToString(), from = modelai, message = result?.choices.FirstOrDefault()?.message?.content ?? "" };
                ChatroomModel chatuserModel = new ChatroomModel() { date = DateTime.Now.ToString(), from = Constant.RedisServerIni.currentusr, message = txtchat.Text };
                UIElement[] uIElement = new UIElement[grdchat.Children.Count];
                grdchat.Children.CopyTo(uIElement, 0);
                int row = int.Parse(uIElement.LastOrDefault()?.GetValue(Grid.RowProperty)?.ToString() ?? "0");
                row = row > 0 ? row + 1 : 0;
                createGrdChat(ref row, chatuserModel, 1);
                createGrdChat(ref row, chataiModel, 2);
                sclchat.ScrollToEnd();
                txtchat.Text = "";
            }
            else
            {
                MessageBox.Show("Máy chủ xảy ra lỗi!");
                return;
            }
        }
        private List<ChatBotsHistoryModel> ChatBotsHistory(ChatBotsHistoryModel model)
        {
            var messages = Constant.RedisServerIni.FTSearch("idx:aihistories", "");
            var data = messages.Select(x => new ChatAiModel()
            {
                from = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).from,
                date = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).date,
                message = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).message,
                type = JsonConvert.DeserializeObject<ChatAiModel>(x.ToString()).type,
            });
            data = data.OrderBy(x => JsonConvert.DeserializeObject<DateTime>("\"" + x.date.Split(":")[0] + ":" + x.date.Split(":")[1] + "\"")).ToList();
            var array = new List<ChatBotsHistoryModel>();
            array.AddRange(data.Select(x => new ChatBotsHistoryModel() { content = x.message, role = x.from == "meta-llama/Llama-3.3-70B-Instruct-Turbo" ? "assistant" : "user" }));
            array.Add(model);
            return array;
        }
        private async void btnsend_Click(object sender, RoutedEventArgs e)
        {
            await SendFunc();
            dsHistory();

        }

        private void txtchat_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtchat.Text.Trim() == "Nhấn để trò chuyện..")
            {
                txtchat.Text = "";
            }
        }

        private void txtchat_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtchat.Text.Trim() == "")
            {
                txtchat.Text = "Nhấn để trò chuyện..";
            }
        }

        private async void txtchat_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift):
                    {
                        e.Handled = true;
                        txtchat.Text += "\n";
                        txtchat.SelectionStart = txtchat.Text.Length + 1;
                    }
                    break;
                case System.Windows.Input.Key.Enter:
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        e.Handled = true;
                        await SendFunc();
                        txtchat.Text = string.Empty;
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                    break;

                default:
                    break;
            }
        }

        private void changeModel_Click(object sender, RoutedEventArgs e)
        {
            chooseModel.IsOpen = true;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            modelai = ((ComboBoxItem)((ComboBox)e.OriginalSource).SelectedItem).Content?.ToString();
            if (modelai != null)
                ldChatHistory();
        }

        private void lsthistory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            key_session = ((ChatAiModel)((ListBox)e.OriginalSource).SelectedItem)?.key_session;
            if (key_session != null)
            {
                ldChatHistory();
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            key_session = Guid.NewGuid().ToString();
            ldChatHistory();
        }
    }
}
