using System.Text.Json.Serialization;

namespace RestOlympe_Server.Models.Entities
{
    public class VoteModel
    {
        public Guid LobbyId { get; set; }
        [JsonIgnore]
        public LobbyModel Lobby { get; set; }

        public Guid UserId { get; set; }
        [JsonIgnore]
        public UserModel User { get; set; }

        public long OsmId { get; set; }

        public int Value { get; set; }
    }
}
