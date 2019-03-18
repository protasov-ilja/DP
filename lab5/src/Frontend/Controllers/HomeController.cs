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

namespace Frontend.Controllers
{
    public class HomeController : Controller
    {
        private const string UriBackendAddress = "http://localhost:5000/";
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string data)
        {
            string id = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.BaseAddress = new Uri(UriBackendAddress);
            HttpResponseMessage response = await client.PostAsJsonAsync($"api/values", data);
            id = await response.Content.ReadAsStringAsync();

            return RedirectToAction("GetTextDetailsById", new { textId = id });
        }

        [HttpGet("TextDetails")]
        public async Task<IActionResult> GetTextDetailsById(string textId)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(UriBackendAddress);
            HttpResponseMessage response = await client.PostAsJsonAsync($"/api/values/rank", textId.Split("\"")[1]);
            string rank = await response.Content.ReadAsStringAsync();

            return Ok(rank);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
