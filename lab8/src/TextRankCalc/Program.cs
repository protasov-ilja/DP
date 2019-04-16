using System;
using StackExchange.Redis;
using System.Threading;
using Newtonsoft.Json;
using TextRankCalc.Data;

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
                var data = JsonConvert.DeserializeObject<DataWrapper>(message);
                if (data == null || data.DataType != "ProcessingAccepted")
                {
                    Console.WriteLine("data is null");
                    return;
                }
                
                var realData = data.DeserializeDataFromString<ProcessStatusData>();
                if (!realData.Status)
                {
                    Console.WriteLine("processing forbiden");
                    return;
                }

                var id = realData.TextId;
                if (id == null)
                {
                    Console.WriteLine("id is null");  
                    return;
                }
                
                var idDb = GetDbId(id);
                var db = _redis.GetDatabase(idDb);
                Console.WriteLine($"TextCreated: {id}"); 
                var value = db.StringGet(id).ToString();

                Console.WriteLine($"TextCreated: {value}");
                if (value == null)
                {
                    Console.WriteLine("value is null");
                    return;
                }
                
                Console.WriteLine($"{id} : {value}");
                SendMessage($"{id}");
            });

            Console.ReadLine();
        }

        private static void SendMessage(string contextId)
        {
            _db.ListLeftPush(COUNTER_QUEUE_NAME, contextId, flags: CommandFlags.FireAndForget);
            _redis.GetSubscriber().Publish(COUNTER_HINTS_CHANNEL, "");
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
