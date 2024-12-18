namespace Chirp.Core.DTOs
{
    //AuthorDTO class used to display Authors in the program, without unnecessary information
    public class AuthorDTO
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}