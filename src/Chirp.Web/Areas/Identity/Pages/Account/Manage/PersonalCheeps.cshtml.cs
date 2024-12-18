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
    /// <summary>
    /// Displays the personal Cheeps of the currently authenticated user.
    /// Allows for pagination of Cheeps, showing a fixed number per page.
    /// </summary>
    public class PersonalCheepsModel : PageModel
    {
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;
        private readonly ICheepRepository _cheepService;

        // List of Cheeps authored by the current user, mapped as DTOs
        public required List<CheepDTO> Cheeps { get; set; }

        // The current page index for pagination
        public int CurrentPage { get; set; }

        // Constant for the number of Cheeps displayed per page
        private const int PageSize = 32;

        // Constructor to inject dependencies for user management and Cheep services
        public PersonalCheepsModel(UserManager<Author> userManager, SignInManager<Author> signInManager, ICheepRepository cheepService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cheepService = cheepService;
        }

        // Handles the GET request to retrieve and display the current user's Cheeps
        // The page number is passed via the query string, defaulting to 0 (first page)
        public async Task<IActionResult> OnGetAsync([FromQuery] int page = 0)
        {
            CurrentPage = page;

            // Retrieves the Cheeps of the current user and assigns them to the Cheeps property
            Cheeps = _cheepService.GetCheepsFromAuthor(await _userManager.GetUserAsync(User), page, PageSize);

            return Page();
        }
    }
}
