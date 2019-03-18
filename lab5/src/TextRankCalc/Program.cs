using System;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        const string COUNTER_HINTS_CHANNEL = "counter_hints";
        const string COUNTER_QUEUE_NAME = "vowel-cons-counter-jobs";
        private static ConnectionMultiplexer _redis;
        private static IDatabase _db;

        static void Main(string[] args)
        {
            Console.WriteLine("TextRankCalc runing");
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe("events", (channel, message) => {
                var id = message.ToString(); 
                if (id == null)
                {
                    Console.WriteLine("id is null");  
                    return;
                }

                Console.WriteLine($"TextCreated: {id}"); 
                var value = _db.StringGet(id).ToString();
                if (value == null)
                {
                    Console.WriteLine("value is null");  
                    return;
                }
                
                Console.WriteLine($"{id} : {value}");
                SendMessage($"{id}:{value}");
            });

            Console.ReadLine();
        }

        private static void SendMessage(string contextId)
        {
            // put message to queue
            _db.ListLeftPush(COUNTER_QUEUE_NAME, contextId, flags: CommandFlags.FireAndForget);
            // and notify consumers
            _redis.GetSubscriber().Publish(COUNTER_HINTS_CHANNEL, "");
        }
    }
}
