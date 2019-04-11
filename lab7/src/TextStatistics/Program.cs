using System;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace TextStatistics
{
    class Program
    {
        private static int _textNum = 0;
        private static int _highRankPart = 0;
        private static double _avrRank = 0;
        private static double _rankSum = 0;

        private static IConnectionMultiplexer  _redis;
        private static IDatabase _db;

        static void Main(string[] args)
        {
            Console.WriteLine("TextStatistics runing");
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe("events", (channel, message) => {
                var data = JsonConvert.DeserializeObject<DataWrapper<StatisticsEventData>>(message);
                if (data == null || data.DataType != "TextCalculated")
                {
                    Console.WriteLine("data is null");
                    return;
                }

                var rank = data.Data.Rank;

                _textNum++;
                _rankSum += rank;

                _avrRank = _rankSum / _textNum;

                if (rank >= 0.5)
                {
                    _highRankPart++;
                }

                Console.WriteLine($"textNum: {_textNum} highRankPart: {_highRankPart} avrRank: {_avrRank}");

                var db = _redis.GetDatabase(0);
                db.StringSet($"statistics", $"{_textNum}:{_highRankPart}:{_avrRank}");
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
