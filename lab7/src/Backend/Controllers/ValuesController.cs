using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;
using Backend.Types;
using Newtonsoft.Json;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, string> _data = new ConcurrentDictionary<string, string>();
        private ConnectionMultiplexer _redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(string id)
        {
            string value = null;
            _data.TryGetValue(id, out value);

            return value;
        }

        [HttpPost("rank")]
        public string GetRank([FromBody] string textId)
        {
            var dataRetrievalAttemptsCount = 3;
            var db = _redis.GetDatabase(GetDbId(textId));
            for (var i = 0; i < dataRetrievalAttemptsCount; ++i) 
            {
                var rank = db.StringGet("rank_" + textId).ToString();
                if (rank != null)
                {
                    return rank;
                }

                Thread.Sleep(1000);
            }

            return null;
        }

        [HttpGet("statistic")]
        public StatisticsData GetStatistic()
        {
            string result = null;
            IDatabase redisDatabase = _redis.GetDatabase(0);
            for (int i = 0; i < 3; i++)
            {
                string stringByKey = redisDatabase.StringGet("statistics");
                if (stringByKey != null)
                {
                    result = stringByKey;
                    break;
                }

                Thread.Sleep(millisecondsTimeout: 300);
            }

            var str = result.Split(":");
            var textStatistics = new StatisticsData
            { 
                TextNum = int.Parse(str[0]), 
                HighRankPart = int.Parse(str[1]), 
                AvgRank = double.Parse(str[2])
            };

            Console.WriteLine($"statistics: {str}");

            return textStatistics;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody] Data value)
        {
            var id = Guid.NewGuid().ToString();
            var dbId = GetDatabaseIdByRegion(value.Region);
            Console.WriteLine($"Region: {value.Region} dbId: {dbId}");
            _redis.GetDatabase(0).StringSet(id, dbId.ToString());
            IDatabase db = _redis.GetDatabase(dbId);
            db.StringSet(id, value.TextId);
            var data = PackToJson<string>("TextCreated", id);
            var sub = _redis.GetSubscriber();
            sub.Publish("events", data);
           
            return id;
        }

        private int GetDatabaseIdByRegion(RegionType region)
        {
            switch (region)
            {
                case RegionType.Ru:
                    return 1;
                case RegionType.Eu:
                    return 2;
                case RegionType.Usa:
                    return 3;
                default:
                    throw new ApplicationException("Unknown region");
            }
        }

        private int GetDbId(string textId)
        {   
            var db = _redis.GetDatabase(0);
            var databaseId = db.StringGet(textId);
            Console.WriteLine(databaseId);
            return int.Parse(databaseId);
        }

        private string PackToJson<T>(string dataType, T data)
        {
            var wrapper = new DataWrapper<T>(dataType, data);

            return JsonConvert.SerializeObject(wrapper);   
        }
    }
}
