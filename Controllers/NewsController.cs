using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Proiect_medical.Controllers
{
    [AllowAnonymous]
    public class NewsController : Controller
    {
        private readonly HttpClient _httpClient;

        public NewsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "https://localhost:7211/api/News/latest";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return View(new JArray()); // Trimitem un JArray gol dacă API-ul nu funcționează
            }

            string responseData = await response.Content.ReadAsStringAsync();

            // Parsăm răspunsul ca JObject
            var json = JObject.Parse(responseData);

            // Extragem doar array-ul "results"
            JArray newsData = (JArray)json["results"];

            return View(newsData);
        }

    }
}
