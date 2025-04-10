using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using OpenAI.Chat;
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
using static Emoji.Wpf.EmojiData;

namespace ChatAppRealTime
{
	/// <summary>
	/// Interaction logic for ChatRoom.xaml
	/// </summary>
	public partial class ChatRoom : AdonisUI.Controls.AdonisWindow
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
			grdchat.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			StackPanel stackPanel = new StackPanel();
			stackPanel.SetValue(Grid.ColumnProperty, item.from == Constant.RedisServerIni.currentusr ? 1 : 0);
			stackPanel.SetValue(Grid.RowProperty, row);
			//img element
			Image uIElementImg = new Image();
			uIElementImg.SetValue(Image.HeightProperty, (double)50);
			uIElementImg.Source = new BitmapImage(
new Uri(@"/ChatAppRealTime;component/img/OIP.jpg", UriKind.Relative));
			TextBlock uIElementUsr = new TextBlock();
			uIElementUsr.SetValue(TextBlock.TextProperty, item.from);
			uIElementUsr.SetValue(TextBlock.MarginProperty, new Thickness(5, 50, 0, 0));

			TextBlock uIElementTime = new TextBlock();
			uIElementTime.SetValue(TextBlock.MarginProperty, new Thickness(0, 25, 0, 0));
			uIElementTime.SetValue(TextBlock.FontSizeProperty, (double)10);
			uIElementTime.SetValue(TextBlock.TextProperty, JsonConvert.DeserializeObject<DateTime>("\"" + item.date.Split(":")[0] + ":" + item.date.Split(":")[1] + "\"").ToString("dd/MM/yyyy hh:mm tt"));
			//end
			//info element
			Emoji.Wpf.TextBlock uIElementInfo = new Emoji.Wpf.TextBlock();
			uIElementInfo.TextWrapping = TextWrapping.Wrap;
			uIElementInfo.Padding = new Thickness(10);
			uIElementInfo.SetValue(TextBlock.TextProperty, item.message);
			//end
			row++;
			stackPanel.Children.Add(uIElementImg);
			stackPanel.Children.Add(uIElementUsr);
			stackPanel.Children.Add(uIElementTime);
			stackPanel.Children.Add(uIElementInfo);
			grdchat.Children.Add(stackPanel);

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
			grdchat.RowDefinitions.Clear();

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
			bool kq = Constant.RedisServerIni.Publish(Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text);
			if (kq)
			{
				Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text = "";

			}
			else
			{
				MessageBox.Show("Máy chủ xảy ra lỗi!");
				return;
			}
		}

		private void txtchat_GotFocus(object sender, RoutedEventArgs e)
		{
			if (Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text.Trim() == "Nhấn để trò chuyện..")
			{
				Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text = "";
			}
		}

		private void txtchat_LostFocus(object sender, RoutedEventArgs e)
		{
			if (Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text.Trim() == "")
			{
				Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text = "Nhấn để trò chuyện..";
			}
		}

		private void txtchat_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case System.Windows.Input.Key.Enter when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift):
					{
						e.Handled = true;
						Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text += "\n";
						Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").SelectionStart = Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text.Length + 1;
					}
					break;
				case System.Windows.Input.Key.Enter:
					{
						Mouse.OverrideCursor = Cursors.Wait;
						e.Handled = true;
						Constant.RedisServerIni.Publish(Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text);
						Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text = string.Empty;
						Mouse.OverrideCursor = Cursors.Arrow;
					}
					break;

				default:
					break;

			}
		}

		private void BtnEmoji_Click(object sender, RoutedEventArgs e)
		{
            EmojiViewer emojiViewer = new EmojiViewer();
            emojiViewer.Owner = this; // Đặt MainWindow làm chủ cửa sổ con

            emojiViewer.Show();
        }
		
		//private void lbEmoji_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		//{
		//	if (Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text.Trim() == "Nhấn để trò chuyện..")
		//	{
		//		Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text = "";
		//	}
		//	ListBoxItem clickedItem = FindParent<ListBoxItem>(e.OriginalSource as DependencyObject);

		//	if (clickedItem != null)
		//	{
		//		// Lấy nội dung emoji và thêm vào TextBox
		//		if (clickedItem.Content is Emoji.Wpf.TextBlock emojiBlock)
		//		{
		//			Helper.Common.GetTextBoxInside<TextBox>(txtchat, "chatchild").Text += emojiBlock.Text;
		//		}
		//	}
		
		//}
		// Hàm tìm phần tử cha gần nhất theo kiểu dữ liệu mong muốn
		//private T FindParent<T>(DependencyObject child) where T : DependencyObject
		//{
		//	while (child != null)
		//	{
		//		if (child is T parent)
		//			return parent;

		//		child = VisualTreeHelper.GetParent(child);
		//	}
		//	return null;
		//}
	}
}
