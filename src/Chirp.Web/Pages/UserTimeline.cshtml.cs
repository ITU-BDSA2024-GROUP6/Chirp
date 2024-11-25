using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;
using Chirp.Core.Models;
using Chirp.Web.Pages.Shared;
using Microsoft.Extensions.WebEncoders.Testing;

namespace Chirp.Web.Pages
{
    public class UserTimelineModel : CheepPageModel
    {
        private readonly ICheepRepository _service;

        [Required]
        public required List<CheepDTO> Cheeps { get; set; }

        [Required]
        public required string Author { get; set; }

        public int CurrentPage { get; set; }
        private const int PageSize = 32;

        public UserTimelineModel(ICheepRepository service)
        {
            _service = service;
        }

        public IActionResult OnGet(string author, [FromQuery] int page = 0)
        {
            Author = author;
            CurrentPage = page;
            Cheeps = _service.GetCheepsFromAuthor(author, page, PageSize);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string author)
        {
            if (!User.Identity!.IsAuthenticated || string.IsNullOrWhiteSpace(Text) || Text.Length > 160)
            {
                return RedirectToPage("/UserTimeline", new { author });
            }

            var authorName = User.Identity.Name ?? "";
            var currentUser = _service.GetAuthorByName(authorName);

            if (currentUser == null)
            {
                return RedirectToPage("/UserTimeline", new { author });
            }

            // Add sanitization before creating the cheep
            var sanitizedText = SanitizeText(Text);
            await _service.CreateCheep(sanitizedText, currentUser, DateTime.UtcNow);
            
            return RedirectToPage("/UserTimeline", new { author });
        }
    }
}