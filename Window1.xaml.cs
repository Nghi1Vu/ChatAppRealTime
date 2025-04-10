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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            this.DataContext=this;
            var data = new List<ChatAiModell>()
    {
        new ChatAiModell { message = "Xin chào", key_session = "001" },
        new ChatAiModell { message = "Tạm biệt", key_session = "002" },
    };

            lsthistory.ItemsSource = data;
        }
    }
        public class ChatAiModell
        {
            public string key_session { get; set; }
            public string message { get; set; }
        }

}
