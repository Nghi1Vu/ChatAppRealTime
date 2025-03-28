using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatAppRealTime
{
    class Helper
    {
        public class Common
        {
            public static bool isMatch(string a, string b)
            {
                return a == b;
            }
            //Hàm tìm phần tử cha gần nhất theo kiểu dữ liệu mong muốn
            public static T FindParent<T>(DependencyObject child) where T : DependencyObject
            {
                while (child != null)
                {
                    if (child is T parent)
                        return parent;

                    child = VisualTreeHelper.GetParent(child);
                }
                return null;
            }
            public static T GetTextBoxInside<T>(Control element, string name)
            {
                return (T)element.Template.FindName(name, element);
            }
        }
    }
}
