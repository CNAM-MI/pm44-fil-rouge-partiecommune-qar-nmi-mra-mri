using System.ComponentModel.DataAnnotations;

namespace RestOlympe_Server.Models.Entities
{
    public class LobbyModel
    {
        [Key]
        public Guid LobbyId { get; set; }

        [Required]
        [MaxLength(32)]
        public string Name { get; set; }

        [MaxLength(256)]
        public string Description { get; set; }

        [Required]
        public float VoteRadiusKm { get; set; }
        

        [Required]
        public Guid AdminId { get; set; }
        public UserModel Admin { get; set; }

        public DateTime? Deadline { get; set; }

        public List<UserModel> Users { get; set; }
    }
}
