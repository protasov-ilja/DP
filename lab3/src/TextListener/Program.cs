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
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe("TextCreated", (channel, message) => {
                var value = _db.StringGet((string)message);
                Console.WriteLine($"{message} {value}");
            });
        }
    }
}
