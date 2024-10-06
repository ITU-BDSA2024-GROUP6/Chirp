using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyChat.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IChatService _service;
    public List<CheepViewModel> Cheeps { get; set; }

    public PublicModel(IChatService service)
    {
        _service = service;
    }

    public ActionResult OnGet()
    {
        Cheeps = _service.GetCheeps();
        return Page();
    }
}
