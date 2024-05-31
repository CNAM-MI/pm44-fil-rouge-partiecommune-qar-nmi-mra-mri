using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RestOlympe_Server.Models.Entities
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(32)]
        public string UserName { get; set; }

        [JsonIgnore]
        public List<LobbyModel> LobbiesAsAdmin { get; set; }

        [JsonIgnore]
        public List<LobbyModel> LobbiesAsUser { get; set; }

    }
}
