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
    /// Handles the display and management of authors that the current user is following.
    /// </summary>
    public class FollowingModel : PageModel
    {
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;
        private readonly IAuthorRepository _authorService;

        // List of authors the current user is following, mapped as DTOs
        public required List<AuthorDTO> Authors { get; set; }

        // Constructor to inject dependencies for user management and author services
        public FollowingModel(UserManager<Author> userManager, SignInManager<Author> signInManager, IAuthorRepository authorService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authorService = authorService;
        }

        // Handles the GET request to retrieve and display the authors the user is following
        public async Task<IActionResult> OnGet()
        {
            Authors = _authorService.GetFollowers(await _userManager.GetUserAsync(User));
            return Page();
        }

        // Handles the POST request to unfollow an author
        public async Task<IActionResult> OnPostUnfollowAsync(string author, string currentAuthor)
        {
            try
            {
                // Removes the specified author from the current user's following list
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                // Logs the error and provides feedback to the user
                Console.WriteLine($"Error while unfollowing author: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
            }
            return RedirectToPage();
        }
    }
}
