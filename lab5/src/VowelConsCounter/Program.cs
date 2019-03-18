using System;
using StackExchange.Redis;
using System.Text.RegularExpressions;

namespace VowelConsCounter
{
    class Program
    {
        const string COUNTER_HINTS_CHANNEL = "counter_hints";
        const string COUNTER_QUEUE_NAME = "vowel-cons-counter-jobs";
        const string RATE_HINTS_CHANNEL = "rate_hints";
        const string RATE_QUEUE_NAME = "vowel-cons-rater-jobs";
        private static IConnectionMultiplexer  _redis;
        private static IDatabase _db;

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsCounter runing");
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            var _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe(COUNTER_HINTS_CHANNEL, delegate {
                string msg = _db.ListRightPop(COUNTER_QUEUE_NAME);
                while (msg != null)
                {
                    var id = msg.Split(':')[0];
                    var str = msg.Split(':')[1];
                    Console.WriteLine(msg);
                    int vowelsAmount = Regex.Matches(str, @"[eyuioaEYUIOA]", RegexOptions.IgnoreCase).Count;
                    int consonantsAmount = Regex.Matches(str, @"[qwrtpsdfghjklzxcvbnmQWRTPSDFGHJKLZXCVBNM]", RegexOptions.IgnoreCase).Count;
                    SendMessage($"{id}:{vowelsAmount}:{consonantsAmount}");
                    msg = _db.ListRightPop(COUNTER_QUEUE_NAME);
                }
            });

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        private static void SendMessage(string message)
        {
            _db.ListLeftPush(RATE_QUEUE_NAME, message, flags: CommandFlags.FireAndForget);
            _db.Multiplexer.GetSubscriber().Publish(RATE_HINTS_CHANNEL, "");
        }
    }
}
