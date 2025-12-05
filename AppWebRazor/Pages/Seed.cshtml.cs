using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace AppWebRazor.Pages;


public class SeedModel : PageModel
{


    [BindProperty]
    public string? ErrorMessage { get; set; } = null;
    readonly IAdminService _service;
    public SeedModel(IAdminService service)
    {
        _service = service;
    }
    public async Task<IActionResult> OnPost(int nrOfItems)
    {
        try
        {
            await _service.SeedAsync(nrOfItems);
        }
        catch (Exception ex)
        {

            ErrorMessage = ex.Message;
        }
        return Page();
    }
    public void OnGet()
    {
    }
}

