using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestOlympe_REST.Models.Entities
{
    public class MessageModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Message { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
