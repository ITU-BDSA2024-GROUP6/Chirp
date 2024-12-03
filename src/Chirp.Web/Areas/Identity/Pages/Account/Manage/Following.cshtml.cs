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
    public class FollowingModel : PageModel
    {
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;

        private readonly IAuthorRepository _authorService;

        public required List<AuthorDTO> Authors { get; set; }

        public FollowingModel(UserManager<Author> userManager, SignInManager<Author> signInManager, IAuthorRepository authorService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authorService = authorService;
        }

        public async Task<IActionResult> OnGet()
        {
            Authors = _authorService.GetFollowers(await _userManager.GetUserAsync(User));

            return Page();
        }

        public async Task<IActionResult> OnPostUnfollowAsync(string author, string currentAuthor) 
        {
            try
            {
                await _authorService.UnfollowAuthor(User.Identity!.Name!, author);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error while unfollowing author: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Unable to unfollow this author. Please try again.");
            }
            return RedirectToPage();
        }
    }
}