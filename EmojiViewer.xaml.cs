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
    public partial class EmojiViewer : Window
    {
        public EmojiViewer()
        {
            InitializeComponent();

        }

        private void Emoji_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Emoji.Wpf.TextBlock textBlock)
            {
                MessageBox.Show($"Bạn đã chọn emoji: {textBlock.Text}");
            }

            TextBlock clickedItem =Helper.Common.FindParent<TextBlock>(e.OriginalSource as DependencyObject);
            Helper.Common.GetTextBoxInside<TextBox>((Control)this.Owner.FindName("txtchat"), "chatchild").Focus();
            if (clickedItem != null)
            {
                // Lấy nội dung emoji và thêm vào TextBox
                if (clickedItem is Emoji.Wpf.TextBlock emojiBlock)
                {
                    Helper.Common.GetTextBoxInside<TextBox>((Control)this.Owner.FindName("txtchat"), "chatchild").Text += emojiBlock.Text;
                }
            }
        }
    }
}
