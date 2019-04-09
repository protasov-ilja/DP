using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Frontend.Types;
using System.Net.Sockets;

namespace Frontend.Controllers
{
    [Route("[controller]")]
    public class StatisticsController : Controller
    {
        private const string UriBackendAddress = "http://localhost:5000/";

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(UriBackendAddress);
            StatisticsData result = null;
            HttpResponseMessage httpResponse = null;

            httpResponse = await client.GetAsync("api/text/statistics");
            if (httpResponse.IsSuccessStatusCode)
            {
                result = await httpResponse.Content.ReadAsAsync<StatisticsData>(); 
            }

            return View(result);
        }

    }
}