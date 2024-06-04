namespace RestOlympe_Server.Models.DTO
{
#pragma warning disable IDE1006 // Styles d'affectation de noms
    public record GeoPoint(double lon, double lat);
    public record OpenRestaurantDTO(string name, string type, string meta_osm_id, GeoPoint meta_geo_point);
    public record RestaurantListDTO(int total_count, List<OpenRestaurantDTO> results);
#pragma warning restore IDE1006 // Styles d'affectation de noms
}
