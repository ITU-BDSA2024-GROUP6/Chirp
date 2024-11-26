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
        public required List<CheepDTO> Cheeps { get; set; }

        public int CurrentPage { get; set; }

        private const int PageSize = 32;

        public PublicModel(ICheepRepository cheepService, IAuthorRepository authorService)
        {
            _cheepService = cheepService;
            _authorService = authorService;
        }

        public ActionResult OnGet([FromQuery] int page = 0)
        {
            CurrentPage = page;
            Cheeps = _cheepService.GetCheeps(page, PageSize);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity!.IsAuthenticated || string.IsNullOrWhiteSpace(Text) || Text.Length > 160)
            {
                return OnGet();
            }

            var authorName = User.Identity.Name ?? "";
            var author = _authorService.GetAuthorByName(authorName);

            if (author == null)
            {
                return OnGet(); 
            }

            // Add sanitization before creating the cheep
            var sanitizedText = SanitizeText(Text);
            await _cheepService.CreateCheep(sanitizedText, author, DateTime.UtcNow);
            
            return RedirectToPage("/Public", new { page = 1 });
        }
    }
}