using System;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace TextListener
{    
    class Program
    {
        private static ConnectionMultiplexer _redis;
        private static IDatabase _db;

        static void Main(string[] args)
        {
            Console.WriteLine("TextListener runing");

            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe("events", (channel, message) => {
                var data = JsonConvert.DeserializeObject<DataWrapper<string>>(message);
                if (data == null || data.DataType != "TextCreated")
                {
                    Console.WriteLine("data is null");
                    return;
                }

                var db = _redis.GetDatabase(GetDbId(data.Data));
                var value = db.StringGet(data.Data);
                
                Console.WriteLine($"TextCreated: {message} {value}");
            });

            Console.ReadLine();
        }

        private static int GetDbId(string textId)
        {   
            var db = _redis.GetDatabase(0);
            var databaseId = db.StringGet(textId);
            Console.WriteLine(databaseId);
            return int.Parse(databaseId);
        }
    }
}
