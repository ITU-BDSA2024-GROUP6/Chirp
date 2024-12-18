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

        /// <summary>
        /// Initializes the UserTimelineModel with dependencies for cheep and author operations.
        /// </summary>
        public UserTimelineModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {
            _cheepService = cheepService;
            _authorService = authorService;
        }

        /// <summary>
        /// Handles HTTP POST requests to create a new cheep on the timeline.
        /// </summary>
        
        // <param name="author">The username of the timeline owner.</param>
        /// <returns>An IActionResult that redirects back to the timeline page.</returns>
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

            Cheeps = _cheepService.GetCheepsFromAuthor(targetAuthor, page, PageSize);

            if (IsOwnTimeline(CurrentUser))
            {
                Cheeps.AddRange(_cheepService.GetUsersFollowingCheeps(targetAuthor, page, PageSize));
            }
            
            Cheeps = Cheeps
                .OrderByDescending(cheep => DateTime.Parse(cheep.TimeStamp))
                .ToList();

            if (CurrentUser != string.Empty)
            {
                foreach (var cheep in Cheeps)
                {
                    cheep.IsFollowing = await _authorService.IsFollowing(CurrentUser, cheep.AuthorDTO.Name);
                }
            }

            return Page();
        }

        /// <summary>
        /// Handles HTTP POST requests to create a new cheep on the timeline.
        /// </summary>
        
        /// <param name="author">The username of the timeline owner.</param>
        /// <returns>An IActionResult that redirects back to the timeline page.</returns>
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

        /// <summary>
        /// Handles HTTP POST requests to follow a specified author.
        /// </summary>
        
        /// <param name="author">The username of the author to follow.</param>
        /// <returns>An IActionResult that redirects to the timeline page.</returns>
        public async Task<IActionResult> OnPostFollowAsync(string author)
        {
            try
            {
                
                await _authorService.FollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to follow this author. Please try again.");
                Console.WriteLine(ex.Message);
            }
            
            if (IsOwnTimeline(User.Identity!.Name!))
            {
                return RedirectToPage("/UserTimeline", new { author = CurrentUser });
            }
            return RedirectToPage("/UserTimeline", new { author });
        }

        /// <summary>
        /// Handles HTTP POST requests to unfollow a specified author.
        /// </summary>
        
        /// <param name="author">The username of the author to unfollow.</param>
        /// <param name="currentAuthor">The username of the timeline owner.</param>
        /// <returns>An IActionResult that redirects to the timeline page.</returns>
        public async Task<IActionResult> OnPostUnfollowAsync(string author, string currentAuthor) 
        {
            try
            {
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
                Console.WriteLine(ex.Message);
            }
            return RedirectToPage("/UserTimeline", new { author = currentAuthor });
        }

        /// <summary>
        /// Determines if the current timeline belongs to the logged-in user.
        /// </summary>
        
        /// <param name="user">The username of the logged-in user.</param>
        /// <returns>True if the timeline belongs to the user, otherwise false.</returns>
        public bool IsOwnTimeline(string user)
        { 
            var currentPath = Request.Path.ToString();

            var authorFromUrl = currentPath.TrimStart('/'); // Remove leading slash
            var decodedAuthorFromUrl = Uri.UnescapeDataString(authorFromUrl); // Decode URL-encoded string

            return string.Equals(decodedAuthorFromUrl, user, StringComparison.OrdinalIgnoreCase);
        }
    }
}