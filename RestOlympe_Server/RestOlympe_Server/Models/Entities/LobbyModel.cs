using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestOlympe_Server.Models.Entities
{
    public class LobbyModel
    {
        [Key]
        public Guid LobbyId { get; set; }

        [Required]
        [MaxLength(32)]
        public string Name { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public float? VoteRadiusKm { get; set; }

        public bool IsClosed { get; set; }


        [Required]
        public Guid AdminId { get; set; }

        public UserModel Admin { get; set; }

        [JsonIgnore]
        public List<UserModel> Users { get; set; }

        [JsonIgnore]
        public List<VoteModel> Votes { get; set; }
    }
}
