﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using StackExchange.Redis;
using System.Threading;
using Backend.Types;

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

            var sub = _redis.GetSubscriber();
            sub.Publish("events", id);
           
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
    }
}
