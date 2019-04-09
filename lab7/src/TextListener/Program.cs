using System;
using StackExchange.Redis;

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
                var db = _redis.GetDatabase(GetDbId((string)message));
                var value = db.StringGet((string)message);
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
