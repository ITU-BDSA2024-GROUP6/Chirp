using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Chirp.Web.Pages.Shared
{
    /// <summary>
    /// An abstract base class for Razor Pages that manage cheep-related functionality.
    /// Provides shared properties and methods for text handling and validation.
    /// </summary>
    public abstract class CheepPageModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Please enter some text")]
        [StringLength(160, ErrorMessage = "Cheeps cannot exceed 160 characters")]
        [RegularExpression(@"^[^<>{}\\]+$", ErrorMessage = "Invalid characters detected")]
        public required string Text { get; set; }

        /// <summary>
        /// Sanitizes the input text by removing potentially harmful content.
        /// </summary>
        
        /// <param name="input">The raw input text to sanitize.</param>
        /// <returns>
        /// A sanitized string with HTML tags and potentially dangerous characters removed.
        /// Preserves emojis and trims surrounding whitespace.
        /// </returns>
        protected string SanitizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove HTML tags
            input = Regex.Replace(input, "<.*?>", string.Empty);
            
            // Remove potentially dangerous characters but preserve emojis
            input = Regex.Replace(input, @"[<>\\]", string.Empty);
            
            return input.Trim();
        }

        /// <summary>
        /// Checks if the input text contains suspicious content that might indicate a security threat.
        /// </summary>
        
        /// <param name="text">The input text to analyze.</param>
        /// <returns>
        /// True if suspicious patterns (e.g., SQL injection or script tags) are found; otherwise, false.
        /// </returns>
        protected bool HasSuspiciousContent(string text)
        {
            var suspicious = new[]
            {
                "<script>",
                "javascript:",
                "onerror=",
                "onload=",
                "SELECT",
                "UNION",
                "DROP TABLE",
                "DELETE FROM"
            };

            return suspicious.Any(s => text.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
    }
}