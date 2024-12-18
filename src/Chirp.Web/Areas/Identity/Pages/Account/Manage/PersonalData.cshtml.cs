// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.Models;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage
{
    // Handles the retrieval and display of personal data for the logged-in user.
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<Author> _userManager; // Manages user-related functionality like retrieving user info
        private readonly ILogger<PersonalDataModel> _logger; // Logger for logging any errors or actions during execution

        // Constructor to inject dependencies for user management and logging
        public PersonalDataModel(
            UserManager<Author> userManager, // UserManager service to handle operations related to the 'Author' entity
            ILogger<PersonalDataModel> logger) // Logger service to log any activities or errors in this page model
        {
            _userManager = userManager;
            _logger = logger;
        }

        // Retrieves the current logged-in user and handles error if not found
        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User); // Fetches the user object from the user manager
            if (user == null) // Checks if the user was not found
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'."); // Returns a "Not Found" page with a message if the user doesn't exist
            }

            return Page(); // Returns the current page if the user is found
        }
    }
}
