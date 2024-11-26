using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;
using Chirp.Web.Pages.Shared;

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
        private const int PageSize = 32;

        public UserTimelineModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {

            _cheepService = cheepService;
            _authorService = authorService;
        }

        public IActionResult OnGet(string author, [FromQuery] int page = 0)
        {
            Author = author;
            CurrentPage = page;
            Cheeps = _cheepService.GetCheepsFromAuthor(_authorService.GetAuthorByName(Author)!, page, PageSize);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string author)
        {
            if (!User.Identity!.IsAuthenticated || string.IsNullOrWhiteSpace(Text) || Text.Length > 160)
            {
                return RedirectToPage("/UserTimeline", new { author });
            }

            var authorName = User.Identity.Name ?? "";
            var currentUser = _authorService.GetAuthorByName(authorName);

            if (currentUser == null)
            {
                return RedirectToPage("/UserTimeline", new { author });
            }

            // Add sanitization before creating the cheep
            var sanitizedText = SanitizeText(Text);
            await _cheepService.CreateCheep(sanitizedText, currentUser, DateTime.UtcNow);
            
            return RedirectToPage("/UserTimeline", new { author });
        }
    }
}