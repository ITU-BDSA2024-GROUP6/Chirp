﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure.Repositories;
using Chirp.Core.RepositoryInterfaces;
using Chirp.Core.DTOs;


namespace Chirp.Web.Pages
{
    public class UserTimelineModel : PageModel
    {
        private readonly ICheepRepository _service;

        [Required]
        public required List<CheepDTO> Cheeps { get; set; }

        [Required]
        public required string Author { get; set; }

        public int CurrentPage { get; set; }
        private const int PageSize = 32;

        public UserTimelineModel(ICheepRepository service)
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