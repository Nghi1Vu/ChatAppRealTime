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
            private string key
            {
                get
                {
                    valkey = Guid.NewGuid().ToString();
                    while (db.KeyExists(valkey))
                    {
                        valkey = Guid.NewGuid().ToString();
                    };
                    return valkey;
                }
                set
                {
                }
            }
            public RedisServerIni()
            {
                conn = ConnectionMultiplexer.Connect("localhost:32769");
                db = conn.GetDatabase();
                RedisBuilder();
            }
            public void RedisBuilder()
            {
                //tbl user
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

                //end tbl user
                //tbl message
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

                //end tbl message
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

            public bool Register(string username, string password)
            {
                var user1 = new
                {
                    username = username,
                    password = password
                };
                bool user1Set = db.JSON().Set($"user:{key}", "$", user1);

                return user1Set;
            }
            public bool CheckExistsAcc(string username)
            {

                SearchResult findPaulResult = db.FT().Search(
                            "idx:users",
                            new Query($"@username:{username}")
                        );
                var data = findPaulResult.Documents.Select(x => x["json"]);

                return data.Any();
            }
            public IEnumerable<RedisValue> GetAllUsers()
            {
                SearchResult findPaulResult = db.FT().Search(
                             "idx:users", new Query());
                var data = findPaulResult.Documents.Select(x => x["json"]);

                return data;
            }
            public bool SaveMessage(string from, string message)
            {
                var chatroom = new
                {
                    from = from,
                    date = DateTime.Now,
                    message=message
                };
                bool chatroomSet = db.JSON().Set($"chatroom:{key}", "$", chatroom);

                return chatroomSet;
            }
            public bool Publish(string message)
            {
                string chatRoom = "chatroom_123";
                sub.Publish(chatRoom, message);
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
