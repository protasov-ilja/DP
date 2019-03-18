using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;

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
            var db = _redis.GetDatabase();
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

        // POST api/values
        [HttpPost]
        public string Post([FromBody] string value)
        {
            var id = Guid.NewGuid().ToString();
            _data[id] = value;
            var db = _redis.GetDatabase();
            var sub = _redis.GetSubscriber();
            db.StringSet(id, value);
            sub.Publish("events", id);

            return id;
        }
    }
}
