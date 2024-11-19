using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Chirp.Web.Pages.Shared
{
    public abstract class CheepPageModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "Please enter some text")]
        [StringLength(160, ErrorMessage = "Cheeps cannot exceed 160 characters")]
        [RegularExpression(@"^[^<>{}\\]+$", ErrorMessage = "Invalid characters detected")]
        public required string Text { get; set; }

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