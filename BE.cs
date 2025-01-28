using NRedisStack.RedisStackCommands;
using NRedisStack.Search.Literals.Enums;
using NRedisStack.Search;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppRealTime
{
    public class BE
    {
        public class RedisServerIni
        {
            private readonly ConnectionMultiplexer conn;
            private readonly IDatabase db;
            private string valkey;
            private string key
            {
                get
                {
                    valkey = Guid.NewGuid().ToString();

                    while (db.KeyExists(key))
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
                conn = ConnectionMultiplexer.Connect("localhost:32770");
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
            }
            public bool Login(string username, string password)
            {

                SearchResult findPaulResult = db.FT().Search(
                            "idx:users",
                            new Query($"@username:{username} @password:{password}")
                        );
                var data = findPaulResult.Documents.Select(x => x["json"]);
                
                return data.Any();
            }
            public bool Register(string username, string password, string pwrepeat)
            {
                bool ichk = Helper.Common.isMatch(password, pwrepeat);

                if (!ichk)
                {
                    return false;
                }
                var user1 = new
                {
                    username = username,
                    password = password
                };
                bool user1Set = db.JSON().Set($"user:{valkey}", "$", user1);

                return user1Set;
            }
        }
    }
}
