#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.Models;
using Chirp.Core.DTOs;
using NuGet.Packaging.Signing;
using Chirp.Core.RepositoryInterfaces;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    public class PersonalCheepsModel : PageModel
    {
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;

        private readonly ICheepRepository _cheepService;

        public required List<CheepDTO> Cheeps { get; set; }

        public int CurrentPage { get; set; }

        private const int PageSize = 32;

        public PersonalCheepsModel(UserManager<Author> userManager, SignInManager<Author> signInManager, ICheepRepository cheepService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cheepService = cheepService;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int page = 0)
        {
            CurrentPage = page;

            Cheeps = _cheepService.GetCheepsFromAuthor(await _userManager.GetUserAsync(User), page, PageSize);

            return Page();
        }
    }
}