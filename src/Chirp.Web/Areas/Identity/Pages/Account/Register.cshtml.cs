// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Chirp.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Chirp.Infrastructure.Data;

namespace Chirp.Web.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Represents the model for the user registration page in the Identity area.
    /// Handles the registration of new users, including validation and account creation.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Author> _signInManager;
        private readonly UserManager<Author> _userManager;
        private readonly IUserStore<Author> _userStore;
        private readonly IUserEmailStore<Author> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ChatDBContext _context;

        /// <summary>
        /// Initializes the RegisterModel with required services for user registration.
        /// </summary>
        public RegisterModel(
        UserManager<Author> userManager,
        IUserStore<Author> userStore,
        SignInManager<Author> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        ChatDBContext context)
        {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _context = context;
        }

        /// <summary>
        /// Represents the data entered by the user during registration.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        /// The URL to redirect to after successful registration.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// External login providers available during registration.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        /// Nested model for user input data during registration.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
            [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            public required string Username { get; set; }
            
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        /// <summary>
        /// Handles the HTTP GET request to display the registration page.
        /// </summary>
        
        /// <param name="returnUrl">The URL to redirect to after registration.</param>
        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        /// <summary>
        /// Handles the HTTP POST request to register a new user.
        /// </summary>
        
        /// <param name="returnUrl">The URL to redirect to after successful registration.</param>
        /// <returns>
        /// Redirects to the return URL on success or redisplays the registration page on failure.
        /// </returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Set the username and email
                await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }


        /// <summary>
        /// Creates a new instance of the Author user model.
        /// </summary>
        
        /// <returns>A new instance of the Author class.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the Author class cannot be instantiated.
        /// </exception>
        private Author CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Author>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Author)}'. " +
                    $"Ensure that '{nameof(Author)}' is not an abstract class and has a parameterless constructor, or " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml.");
            }
        }

        /// <summary>
        /// Retrieves the email store for the user model.
        /// </summary>¨
        
        /// <returns>An IUserEmailStore instance for managing user emails.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the user store does not support email operations.
        /// </exception>
        private IUserEmailStore<Author> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Author>)_userStore;
        }
    }
}