namespace Chirp.Core.DTOs
{
    public class CheepDTO
    {
        public required string Text { get; set; }

        public required string TimeStamp { get; set; }

        public required AuthorDTO AuthorDTO { get; set; }

        public bool IsFollowing { get; set; }
    }
}