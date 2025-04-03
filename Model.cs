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
		public class ChatBotsHistoryModel
        {
            public string role { get; set; }
            public string content { get; set; }
        }
		public class MessageAiModel
		{
            public string key_session { get; set; }
            public string from { get; set; }
            public string message { get; set; }
            public string modelai { get; set; }
            public string messageai { get; set; }
        }
		public class ChatAiResponseModel
		{
			public string id { get; set; }
			public string created { get; set; }
			public string model { get; set; }
			public choices_model[] choices { get; set; }
			public usage_model usage { get; set; }

		}
		public class usage_model
		{
			public int prompt_tokens { get; set; }
			public int completion_tokens { get; set; }
			public int total_tokens { get; set; }

		}
		public class choices_model
        {
			public string finish_reason { get; set; }
			public int index { get; set; }
			public message_model message { get; set; }

		}
		public class message_model
		{
			public string role { get; set; }
			public string content { get; set; }

		}
		public class ChatroomModel
        {
            public string from { get; set; }
            public string date { get; set; }
            public string message { get; set; }
        }
		public class ChatAiModel : ChatroomModel
        {
            public int type { get; set; }
            public string key_session { get; set; }
        }
    }
}
