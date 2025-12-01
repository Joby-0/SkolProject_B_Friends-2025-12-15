using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppWebRazor.Pages;


public class SeedModel : PageModel
{

    readonly IAdminService _service;
    public SeedModel(IAdminService service)
    {
        _service = service;
    }
    public async Task<IActionResult> OnPost(int nrOfItems)
    {
        await _service.SeedAsync(nrOfItems);
        return Page();
    }
    public void OnGet()
    {
    }
}

