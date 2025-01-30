using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppRealTime
{
    public class Model
    {
        public class AccountInfo
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class ChatroomModel
        {
            public string from { get; set; }
            public string date { get; set; }
            public string message { get; set; }
        }
    }
}
