using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure.Repositories;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;


namespace Chirp.Web.Pages 
{

    public class PublicModel : PageModel
    {
        private readonly ICheepRepository _service;

        [Required]
        public required List<CheepDTO> Cheeps { get; set; }

        public int CurrentPage { get; set; }

        private const int PageSize = 32;

        public PublicModel(ICheepRepository service)
        {
            _service = service;
        }

        public ActionResult OnGet([FromQuery] int page = 0)
        {
            CurrentPage = page;
            Cheeps = _service.GetCheeps(page, PageSize);
            return Page();
        }
    }
}