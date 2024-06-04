using RestOlympe_Server.Models.DTO;
using System.Net.Http.Headers;

namespace RestOlympe_Server.Services
{
    public class OsmApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://data.opendatasoft.com/api/explore/v2.1/catalog/datasets/osm-france-food-service@public/records";
        private const string BASE_SELECTION = "?select=meta_geo_point%2C%20meta_osm_id%2C%20name%2C%20type";
        private const int PAGE_SIZE = 100;

        public OsmApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new(BASE_URL)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string GetUrlParamsForSingle(string osmId)
        {
            if (!osmId.All(char.IsDigit) && !(osmId.StartsWith('-') && osmId[1..].All(char.IsDigit)))
                throw new ArgumentException("Argument osmId must contain only digits");

            return BASE_SELECTION + $"&where=meta_osm_id%20%3D%20%22{osmId}%22&limit=1";
        }

        private static string GetUrlParamsForList(GeoPoint? center, uint? kmRadius, uint page)
        {
            if (kmRadius.HasValue && center != null)
                return BASE_SELECTION + $"&where=within_distance(meta_geo_point%2C%20geom%27POINT({center.lat}%20{center.lon})%27%2C%20{kmRadius}km)&limit={PAGE_SIZE}&offset={PAGE_SIZE * page}";
            else if (center == null && !kmRadius.HasValue)
                return BASE_SELECTION + $"&limit={PAGE_SIZE}&offset={PAGE_SIZE * page}";
            else
                throw new ArgumentException("Arguments center and kmRadius must both be null or have a value.");
        }

        public async Task<OpenRestaurantDTO?> GetAsync(string osmId)
        {
            var response = await _httpClient.GetAsync(GetUrlParamsForSingle(osmId));
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadFromJsonAsync<RestaurantListDTO>())?.results?.SingleOrDefault();
            }
            return null;
        }

        public async Task<RestaurantListDTO?> GetListAroundLocationAsync(GeoPoint? center, uint? kmRadius, uint page)
        {
            var response = await _httpClient.GetAsync(GetUrlParamsForList(center, kmRadius, page));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<RestaurantListDTO>();
            }
            return null;
        }
    }
}
