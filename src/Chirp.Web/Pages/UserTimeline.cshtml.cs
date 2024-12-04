using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;
using Chirp.Web.Pages.Shared;
using Chirp.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Chirp.Web.Pages
{
    public class UserTimelineModel : CheepPageModel
    {
        private readonly ICheepRepository _cheepService;
        private readonly IAuthorRepository _authorService;

        [Required]
        public required List<CheepDTO> Cheeps { get; set; }

        [Required]
        public required string Author { get; set; }
        public int CurrentPage { get; set; }
        public string CurrentUser { get; set; } = string.Empty;
        private const int PageSize = 32;
        
        // Add properties for FollowersCount and FollowingCount
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public UserTimelineModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {
            _cheepService = cheepService;
            _authorService = authorService;
        }

        public async Task<IActionResult> OnGetAsync(string author, [FromQuery] int page = 0)
        {
            Author = author;
            CurrentUser = User.Identity!.Name ?? string.Empty;
            CurrentPage = page;

            var targetAuthor = _authorService.GetAuthorByName(Author);
            if (targetAuthor == null)
            {
                return NotFound();
            }

            // Populate FollowersCount and FollowingCount
            FollowersCount = targetAuthor.Followers.Count;
            FollowingCount = targetAuthor.Following.Count;

            Cheeps = IsOwnTimeline(CurrentUser)
                ? _cheepService.GetUsersFollowingCheeps(targetAuthor, page, PageSize)
                : _cheepService.GetCheepsFromAuthor(targetAuthor, page, PageSize);

            if (CurrentUser != string.Empty)
            {
                foreach (var cheep in Cheeps)
                {
                    cheep.IsFollowing = await _authorService.IsFollowing(CurrentUser, cheep.AuthorDTO.Name);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string author)
        {
            var authorName = User.Identity!.Name ?? string.Empty;
            var currentUser = _authorService.GetAuthorByName(authorName);

            if (currentUser == null)
            {
                return RedirectToPage("/UserTimeline", new { author });
            }

            var sanitizedText = SanitizeText(Text);
            await _cheepService.CreateCheep(sanitizedText, currentUser, DateTime.UtcNow);

            return RedirectToPage("/UserTimeline", new { CurrentUser });
        }

        public async Task<IActionResult> OnPostFollowAsync(string author)
        {
            Console.WriteLine("in the follow method");
            try
            {
                
                await _authorService.FollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error while following author: {ex.Message}");

                ModelState.AddModelError(string.Empty, "Unable to follow this author. Please try again.");
            }
            
            if (IsOwnTimeline(User.Identity!.Name!))
            {
                return RedirectToPage("/UserTimeline", new { author = CurrentUser });
            }
            return RedirectToPage("/UserTimeline", new { author });
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string author, string currentAuthor) 
        {
            try
            {
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error while unfollowing author: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
            }
            return RedirectToPage("/UserTimeline", new { author = currentAuthor });
        }

        public bool IsOwnTimeline(string user)
        { 
            Console.WriteLine("In the owntimeline method");
            var currentPath = Request.Path.ToString();

            var authorFromUrl = currentPath.TrimStart('/'); // Remove leading slash
            var decodedAuthorFromUrl = Uri.UnescapeDataString(authorFromUrl); // Decode URL-encoded string

            Console.WriteLine("User: " + User);
            Console.WriteLine("Decoded user: " + decodedAuthorFromUrl);
            return string.Equals(decodedAuthorFromUrl, user, StringComparison.OrdinalIgnoreCase);
        }
    }
}

