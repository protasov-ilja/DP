using System;
using StackExchange.Redis;

namespace TextRankCalc
{
    class Program
    {
        private const string Vowels = "eyuioaEYUIOA";
        private const string Consonants = "qwrtpsdfghjklzxcvbnmQWRTPSDFGHJKLZXCVBNM";
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
                Console.WriteLine(id);     
                if (id == null)
                {
                    Console.WriteLine("id is null");  
                    return;
                }

                   
                var value = _db.StringGet(id).ToString();
                if (value == null)
                {
                    Console.WriteLine("value is null");  
                    return;
                }
                
                Console.WriteLine("rank_" + id);
                Console.WriteLine("val " + GetRankByStr(value));
               _db.StringSet("rank_" + id, GetRankByStr(value));
            });

            Console.ReadLine();
        }

        private static string GetRankByStr(string value)
        {
           var vowelsCounter = 0;
           var consonantsCounter = 0;
           for (var i = 0; i < value.Length; ++i)
           {
                var ch = value[i];
                if (Consonants.IndexOf(ch) != -1)
                {
                    consonantsCounter++;
                }
                else if (Vowels.IndexOf(ch) != -1)
                {
                    vowelsCounter++;
                }
            }   

            return (consonantsCounter != 0) ? ((double)vowelsCounter / consonantsCounter).ToString() : "0";
        }
    }
}
