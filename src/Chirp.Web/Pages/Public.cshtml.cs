﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;
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

        /// <summary>
        /// Initializes the PublicModel with dependencies for cheep and author operations.
        /// </summary>
        public PublicModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {
            _cheepService = cheepService;
            _authorService = authorService;
        }

        /// <summary>
        /// Handles HTTP GET requests to load public cheeps.
        /// </summary>
        
        /// <param name="page">The page number for pagination (default: 0).</param>
        /// <returns>An IActionResult that renders the public cheeps page.</returns>
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

        /// <summary>
        /// Handles HTTP POST requests to create a new cheep.
        /// </summary>
        
        /// <returns>An IActionResult that redirects back to the public cheeps page.</returns>
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

            if (sanitizedText.Length <= 160)
            {
                await _cheepService.CreateCheep(sanitizedText, author, DateTime.UtcNow);
            } 
            else {
                ModelState.AddModelError("Text", "Cheeps cannot exceed 160 characters.");
            }
            
            return RedirectToPage("/Public", new{});
        }

        /// <summary>
        /// Initializes the follow status and ownership of the timeline for the current user.
        /// </summary>
        private async Task InitializeFollowStatus()
        {
            CurrentUser = User.Identity!.Name!;
            IsFollowing = await _authorService.IsFollowing(CurrentUser, Author);

            var currentUser = _authorService.GetAuthorByName(CurrentUser);
            var targetAuthor = _authorService.GetAuthorByName(Author);
            IsOwnTimeline = currentUser?.UserName == targetAuthor?.UserName;
        }

        /// <summary>
        /// Handles HTTP POST requests to follow a specified author.
        /// </summary>
        
        /// <param name="author">The username of the author to follow.</param>
        /// <returns>An IActionResult that redirects to the public cheeps page.</returns>
        public async Task<IActionResult> OnPostFollowAsync(string author)
        {
            try 
            {
                Author = author;
                await _authorService.FollowAuthor(User.Identity!.Name!, author);
                await InitializeFollowStatus();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error while unfollowing author: {ex.Message}");

                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
            }

            return RedirectToPage("/Public", new{});
        }

        /// <summary>
        /// Handles HTTP POST requests to unfollow a specified author.
        /// </summary>
        
        /// <param name="author">The username of the author to unfollow.</param>
        /// <returns>An IActionResult that redirects to the public cheeps page.</returns>
        public async Task<IActionResult> OnPostUnfollowAsync(string author)
        {
            try 
            {
                Author = author;
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
                await InitializeFollowStatus();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error while unfollowing author: {ex.Message}");

                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
            }

            return RedirectToPage("/Public", new{});
        }
    }
}