using NetTopologySuite.Index.HPRtree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json.Serialization;
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
    /// Interaction logic for ListUsers.xaml
    /// </summary>
    public partial class ListUsers : Window
    {
        private readonly BE.RedisServerIni RedisServerIni;

        public ListUsers(BE.RedisServerIni redisServerIni)
        {
            this.RedisServerIni = redisServerIni;
            InitializeComponent();
            var lstusers = this.RedisServerIni.GetAllUsers();
            GridLength colWidthLeft = new GridLength(0.5, GridUnitType.Star);
            GridLength colWidthRight = new GridLength(1, GridUnitType.Star);
            grdLstUsers.ColumnDefinitions.Add(new ColumnDefinition() { Width = colWidthLeft });
            grdLstUsers.ColumnDefinitions.Add(new ColumnDefinition() { Width = colWidthRight });
            int row = 0;
            var data = lstusers.Select(x=>new AccountInfo() { username=JsonConvert.DeserializeObject<AccountInfo>(x.ToString()).username,
            password= JsonConvert.DeserializeObject<AccountInfo>(x.ToString()).password
            }).Where(x => x.username != this.RedisServerIni.currentusr);
            foreach (var item in data)
            {
                grdLstUsers.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto, MaxHeight = 200 });
                //img element
                Image uIElementImg = new Image();
                uIElementImg.SetValue(Grid.ColumnProperty, 0);
                uIElementImg.SetValue(Grid.RowProperty, row);
                uIElementImg.Source = new BitmapImage(
    new Uri(@"/ChatAppRealTime;component/img/OIP.jpg", UriKind.Relative));
                //end
                //info element
                Button uIElementInfo = new Button();
                uIElementInfo.SetValue(Grid.ColumnProperty, 1);
                uIElementInfo.SetValue(Grid.RowProperty, row);
                uIElementInfo.SetValue(Button.ContentProperty, item.username);
                uIElementInfo.AddHandler(Button.ClickEvent,new RoutedEventHandler(btnsend_Click));
                //end
                row++;
                grdLstUsers.Children.Add(uIElementImg);
                grdLstUsers.Children.Add(uIElementInfo);
            }
        }	

        private void btnsend_Click(object sender, RoutedEventArgs e)
        {
         
        }
    }
}
