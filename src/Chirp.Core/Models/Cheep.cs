using System.ComponentModel.DataAnnotations;
namespace Chirp.Core.Models
{
    public class Cheep
    {
        public int CheepId { get; set; }

        [Required]
        [StringLength(160, ErrorMessage = "Cheeps cannot exceed 160 characters")] 
        public required string Text { get; set; }

        public DateTime TimeStamp { get; set; }

        public required Author Author { get; set; }
    }
}