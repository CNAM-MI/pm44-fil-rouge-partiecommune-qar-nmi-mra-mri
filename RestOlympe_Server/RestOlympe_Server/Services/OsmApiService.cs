using RestOlympe_Server.Models.DTO;
using System.Net.Http.Headers;

namespace RestOlympe_Server.Services
{
    public class OsmApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://data.opendatasoft.com/api/explore/v2.1/catalog/datasets/osm-france-food-service@public/records";

        public OsmApiService() {
            _httpClient = new HttpClient();
        }

        private static string GetUrlParamsForSingle(string osmId)
        {
            if (!osmId.All(char.IsDigit))
                throw new ArgumentException("Argument osmId must contain only digits");

            return $"?select=meta_geo_point%2C%20meta_osm_id%2C%20name%2C%20type&where=meta_osm_id%20%3D%20%22{osmId}%22&limit=1";
        }

        public async Task<OpenRestaurantDTO?> GetAsync(string osmId)
        {
            _httpClient.BaseAddress = new(BASE_URL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.GetAsync(GetUrlParamsForSingle(osmId));
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadFromJsonAsync<RestaurantListDTO>())?.results?.SingleOrDefault();
            }
            return null;
        }
    }
}
