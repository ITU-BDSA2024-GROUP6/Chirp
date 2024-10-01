using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Chirp.Razor.Pages
{
    public class UserTimelineModel : PageModel
    {
        private readonly ICheepService _service;
        public List<CheepViewModel> Cheeps { get; set; } = new List<CheepViewModel>();
        public string Author { get; set; } = string.Empty;
        public int CurrentPage { get; set; }
        private const int PageSize = 32;

        public UserTimelineModel(ICheepService service)
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
    }
}