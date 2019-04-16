using System;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using TextProcessingLimiter.Data;

namespace TextProcessingLimiter
{
    class Program
    {
        private static ConnectionMultiplexer _redis;
        private static IDatabase _db;

        private static int _processingTimeDelay = 60000;
        private static int _limitCount = 3;
        private static int _counter = 0; 

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Not enough arguments. Must set count limit words");
                return;
            }

            Console.WriteLine("TextProcessingLimiter runing");
            _limitCount = int.Parse(args[0]);
            _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            sub.Subscribe("events", (channel, message) => {
                Console.WriteLine($"{message}");
                var data = JsonConvert.DeserializeObject<DataWrapper>(message);
                if ((data != null) && data.DataType == "TextCalculated")
                {
                    ProcessTextRankCalculatedEvent(data.DeserializeDataFromString<StatisticsEventData>());
                }

                if ((data != null) && (data.DataType == "TextCreated"))
                {
                    ProcessTextCreatedEvent(data.DeserializeDataFromString<string>());
                }
                else
                {
                    Console.WriteLine("data is null");
                }
            });

            Console.ReadLine();
        }

        private static void ProcessTextCreatedEvent(string id)
        {
            _counter++;
            var db = _redis.GetDatabase(GetDbId(id));
            var value = db.StringGet(id);
            
            Console.WriteLine($"TextCreated: {id} {value}");

            var isProcessingAccepted = _counter < _limitCount;
            var data = PackToJson<ProcessStatusData>("ProcessingAccepted", new ProcessStatusData{ TextId = id, Status = isProcessingAccepted});
            var subscr = _redis.GetSubscriber();
            subscr.Publish("events", data);

            if (!isProcessingAccepted)
            {
                Task.Run(async () => {
                    await Task.Delay(_processingTimeDelay);
                    _counter = 0;
                    Console.WriteLine($"timer reset");
                });
            }
        }

        private static void ProcessTextRankCalculatedEvent(StatisticsEventData statistics)
        {   
            Console.WriteLine($"TextCalculated: {statistics.TextId} {statistics.Rank}");
            if (statistics.Rank <= 0.5)
            {
                _counter--;
            }
        }

        private static int GetDbId(string textId)
        {   
            var db = _redis.GetDatabase(0);
            var databaseId = db.StringGet(textId);
            Console.WriteLine(databaseId);
            return int.Parse(databaseId);
        }

        private static string PackToJson<T>(string dataType, T data)
        {
            var wrapper = new DataWrapper(dataType);
            wrapper.SerializeDataToString(data);

            return JsonConvert.SerializeObject(wrapper);   
        }
    }
}
