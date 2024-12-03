namespace Chirp.Core.DTOs
{
    public class AuthorDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}