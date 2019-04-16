using System;
using StackExchange.Redis;
using Newtonsoft.Json;
using TextListener.Data;

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
                var data = JsonConvert.DeserializeObject<DataWrapper>(message);
                if (data == null || data.DataType != "TextCreated")
                {
                    if (data != null)
                    {
                        Console.WriteLine($"data is not correct {data.DataType} {data.Data}");
                    }
                    else
                    {
                        Console.WriteLine($"data is null");
                    }
                    
                    return;
                }

                var realData = data.DeserializeDataFromString<string>();
                var db = _redis.GetDatabase(GetDbId(realData));
                var value = db.StringGet(realData);
                
                Console.WriteLine($"TextCreated: {realData} {value}");
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
