using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Models
{
    /// <summary>
    /// Represents a Cheep (post) in the application.
    /// Each Cheep is associated with an Author and has text content, a timestamp, and a unique identifier.
    /// </summary>
    public class Cheep
    {
        public int CheepId { get; set; }

        [Required]
        [StringLength(160, ErrorMessage = "Cheeps cannot exceed 160 characters")]
        // The content of the Cheep, limited to 160 characters
        public required string Text { get; set; }

        public DateTime TimeStamp { get; set; }

        // The Author who created the Cheep
        public required Author Author { get; set; }
    }
}
