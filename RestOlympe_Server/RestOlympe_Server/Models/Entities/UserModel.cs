using System.ComponentModel.DataAnnotations;

namespace RestOlympe_Server.Models.Entities
{
    public class UserModel
    {
        [Key]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(32)]
        public string UserName { get; set; }

        public List<LobbyModel> LobbiesAsAdmin { get; set; }

        public List<LobbyModel> LobbiesAsUser { get; set; }

    }
}
