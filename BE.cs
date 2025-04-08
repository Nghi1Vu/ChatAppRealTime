using NRedisStack.RedisStackCommands;
using NRedisStack.Search.Literals.Enums;
using NRedisStack.Search;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using Newtonsoft.Json;
using static ChatAppRealTime.Model;
using Newtonsoft.Json.Linq;

namespace ChatAppRealTime
{
	public class BE
	{
		public class RedisServerIni
		{
			private readonly ConnectionMultiplexer conn;
			private readonly IDatabase db;
			public string currentusr;
			private string valkey;
			private ISubscriber sub => conn.GetSubscriber();
			private string this[string name]
			{
				get
				{
					valkey = Guid.NewGuid().ToString();
					while (db.KeyExists(name + valkey))
					{
						valkey = Guid.NewGuid().ToString();
					};
					return valkey;
				}

			}
			public RedisServerIni()
			{
				conn = ConnectionMultiplexer.Connect(
			new ConfigurationOptions
			{
				EndPoints = { { Constant.AppSetting["EndPoints"], 10930 } },
				User = Constant.AppSetting["User"],
				Password = Constant.AppSetting["Password"]
			}
		);
				db = conn.GetDatabase();
				RedisBuilder();
			}
			public void RedisBuilder()
			{
				//user
				var schema = new Schema()
		 .AddTextField(new FieldName("$.username", "username"))
		 .AddTextField(new FieldName("$.password", "password"));
				try
				{
					bool indexCreated = db.FT().Create(
			"idx:users",
			new FTCreateParams()
				.On(IndexDataType.JSON)
				.Prefix("user:"),
			schema
		);
				}
				catch
				{

				}

				//end user
				// message
				schema = new Schema()
		 .AddTextField(new FieldName("$.from", "from"))
		 .AddTextField(new FieldName("$.date", "date"))
		 .AddTextField(new FieldName("$.message", "message"));
				try
				{
					bool indexCreated = db.FT().Create(
			"idx:messages",
			new FTCreateParams()
				.On(IndexDataType.JSON)
				.Prefix("message:"),
			schema
		);
				}
				catch
				{

				}

				//end message
				// message AI
				schema = new Schema()
		 .AddTextField(new FieldName("$.key_session", "key_session"))
		 .AddTextField(new FieldName("$.from", "from"))
		 .AddTextField(new FieldName("$.date", "date"))
		 .AddNumericField(new FieldName("$.timestamp", "timestamp"))
		 .AddTextField(new FieldName("$.message", "message"))
		 .AddNumericField(new FieldName("$.type", "type"));
				try
				{
					bool indexCreated = db.FT().Create(
			"idx:aihistories",
			new FTCreateParams()
				.On(IndexDataType.JSON)
				.Prefix("aihistory:"),
			schema
		);
				}
				catch
				{
					db.FT().DropIndex(
			"idx:aihistories",
			false
		);
					db.FT().Create(
			"idx:aihistories",
			new FTCreateParams()
				.On(IndexDataType.JSON)
				.Prefix("aihistory:"),
			schema
		);
				}

				//end message ai
			}
			public bool Login(string username, string password)
			{

				SearchResult findPaulResult = db.FT().Search(
							"idx:users",
							new Query($"@username:{username} @password:{password}")
						);
				var data = findPaulResult.Documents.Select(x => x["json"]);
				currentusr = data.Any() ? username : "";
				return data.Any();
			}
			public async Task<bool> GetOnlineByUser(string user)
			{
				return db.StringGet($"user_status:{user}") == "online";
			}
			public async Task Heartbeat(string user)
			{
				if (db.StringSet($"user_status:{user}", "online", TimeSpan.FromMinutes(1.15)))
				{
					db.Publish("HeartbeatTTL", user);
				}
			}
			public void HeartbeatTTL(Action<string> action)
			{

				sub.Subscribe("HeartbeatTTL", (channel, message) =>
				{
					action("user_login:" + message);

				});
				sub.Subscribe("__keyevent@0__:expired", (channel, message) =>
				{
					action(message);

				});
			}

			public bool Register(string username, string password)
			{
				var user1 = new
				{
					username = username,
					password = password
				};
				bool user1Set = db.JSON().Set($"user:{this["user:"]}", "$", user1);

				return user1Set;
			}
			public IEnumerable<RedisValue> FTSearch(string name, string query)
			{
				SearchResult findPaulResult = db.FT().Search(
							 name, query != "" ? (new Query(query)).Limit(0, 10000) : (new Query()).Limit(0, 10000));
				var data = findPaulResult.Documents.Select(x => x["json"]);
				return data;
			}
			public IEnumerable<RedisResult> FTSelectOne(string name, string query, string slect)
			{
//				SearchResult findPaulResult = db.FT().Search(
//							 name, query != "" ? (new Query(query)).Limit(0, 10000) : (new Query()).Limit(0, 10000));
//				IEnumerable<JToken> data = null;
//				data = findPaulResult.Documents
//.Select(x => JsonConvert.DeserializeObject<JObject>(x["json"].ToString())?[slect])// Lấy cột "slect"
//.Distinct()// Loại bỏ trùng lặp
//.ToList(); 
				var result = db.Execute("FT.AGGREGATE",
	name,
	"@type:[1 1]",
	"GROUPBY", "1", "@key_session",
	"REDUCE", "FIRST_VALUE", "4", "@message", "BY", "@timestamp", "DESC","AS","message");
				var rows = ((RedisResult[])result).Skip(1);

				return rows;
			}
			public bool SaveMessage(string from, string message)
			{
				var chatroom = new
				{
					from = from,
					date = DateTime.Now,
					message = message
				};
				bool chatroomSet = db.JSON().Set($"message:{this["message:"]}", "$", chatroom);

				return chatroomSet;
			}
			public bool SaveMessageAi(MessageAiModel model)
			{
				var chatUser = new
				{
					key_session = model.key_session,
					from = model.from,
					date = DateTime.Now,
					timestamp = DateTime.Now.Ticks,
					message = model.message,
					type = 1
				};
				var chatAi = new  //type ==1 user, type == 2 bot
				{
					key_session = model.key_session,
					from = model.modelai,
					date = DateTime.Now,
					timestamp = DateTime.Now.Ticks,
					message = model.messageai,
					type = 2
				};
				bool chatuserSet = db.JSON().Set($"aihistory:{this["aihistory:"]}", "$", chatUser);
				bool chataiSet = db.JSON().Set($"aihistory:{this["aihistory:"]}", "$", chatAi);

				return chatuserSet && chataiSet;
			}
			public bool Publish(string message)
			{
				string chatRoom = "chatroom_123";
				var json = JsonConvert.SerializeObject(new
				{
					message = message,
					date = DateTime.Now,
					from = currentusr
				});
				sub.Publish(chatRoom, json);
				SaveMessage(currentusr, message);
				return true;
			}
			public void Subscriber(Action<RedisValue> action)
			{
				sub.Subscribe("chatroom_123", (channel, message) =>
				{
					action(message);
					// Cập nhật UI (có thể sử dụng Dispatcher.Invoke để cập nhật UI trên thread chính)
				});
			}
		}
	}

}
