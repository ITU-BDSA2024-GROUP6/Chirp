using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;
using Chirp.Core.Models;
using Chirp.Web.Pages.Shared;

namespace Chirp.Web.Pages 
{
    public class PublicModel : CheepPageModel  // Changed from PageModel to CheepPageModel
    {
        private readonly ICheepRepository _cheepService;
        private readonly IAuthorRepository _authorService;
        
        [Required]
        public required string Author { get; set; }

        public string CurrentUser { get; set; } = string.Empty; 

        public bool IsOwnTimeline { get; set; }

        public bool IsFollowing { get; set; }
        
        [Required]
        public required List<CheepDTO> Cheeps { get; set; }

        public int CurrentPage { get; set; }

        private const int PageSize = 32;

        public PublicModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {
            _cheepService = cheepService;
            _authorService = authorService;
        }

        public async Task<IActionResult> OnGet([FromQuery] int page = 0)
        {
            CurrentPage = page;
            Cheeps = _cheepService.GetCheeps(page, PageSize);

            if (User.Identity!.IsAuthenticated) 
            {
                CurrentUser = User.Identity!.Name!;
                foreach (var cheep in Cheeps)
                {
                    cheep.IsFollowing = await _authorService.IsFollowing(CurrentUser, cheep.AuthorDTO.Name);
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var authorName = User.Identity!.Name ?? "";
            var author = _authorService.GetAuthorByName(authorName);

            if (author == null)
            {
                return await OnGet(); 
            }

            // Add sanitization before creating the cheep
            var sanitizedText = SanitizeText(Text);
            await _cheepService.CreateCheep(sanitizedText, author, DateTime.UtcNow);
            
            return RedirectToPage("/Public", new{});
        }

        private async Task InitializeFollowStatus()
            {
                CurrentUser = User.Identity!.Name!;
                IsFollowing = await _authorService.IsFollowing(CurrentUser, Author);

                var currentUser = _authorService.GetAuthorByName(CurrentUser);
                var targetAuthor = _authorService.GetAuthorByName(Author);
                IsOwnTimeline = currentUser?.UserName == targetAuthor?.UserName;
            }

        public async Task<IActionResult> OnPostFollowAsync(string author)
        {
            try 
            {
                Author = author;
                await _authorService.FollowAuthor(User.Identity!.Name!, author);
                await InitializeFollowStatus();
            }
            catch (ArgumentException)
            {
                // Handle the error gracefully, maybe add to ModelState if needed
            }

            return RedirectToPage("/Public", new{});
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string author)
        {
            try 
            {
                Author = author;
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
                await InitializeFollowStatus();
            }
            catch (ArgumentException)
            {
                // Handle the error gracefully
            }

            return RedirectToPage("/Public", new{});
        }
    }
}