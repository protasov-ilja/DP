using System;
using StackExchange.Redis;

namespace VowelConsRater
{
    class Program
    {
        const string RATE_HINTS_CHANNEL = "rate_hints";
        const string RATE_QUEUE_NAME = "vowel-cons-rater-jobs";
        private static IConnectionMultiplexer  _redis;
        private static IDatabase _db;

        static void Main(string[] args)
        {
            Console.WriteLine("VowelConsRater runing");
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe(RATE_HINTS_CHANNEL, delegate {
                string msg = _db.ListRightPop(RATE_QUEUE_NAME);
                while (msg != null)
                {
                    var argumets = msg.Split(':');
                    string id = argumets[0];
                    var vowelsAmount = int.Parse(argumets[1]);
                    var consonantsAmount = int.Parse(argumets[2]);
                    var rank = (consonantsAmount != 0) ? ((double)vowelsAmount / consonantsAmount).ToString() : "0";
                    Console.WriteLine("rank_" + id);
                    _db.StringSet($"rank_{id}", rank);
                    msg = _db.ListRightPop(RATE_QUEUE_NAME);
                }
            });

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}
