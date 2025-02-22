using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Proiect_medical.Services
{
    public class NewsService
    {
        private readonly HttpClient _httpClient;

        public NewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<JToken> GetLatestNewsAsync()
        {
            string apiUrl = "https://localhost:7211/api/News/latest"; 

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string responseData = await response.Content.ReadAsStringAsync();
            var newsData = JToken.Parse(responseData);
            return newsData;
        }
    }
}
