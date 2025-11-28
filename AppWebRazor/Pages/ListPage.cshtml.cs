using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppWebRazor.Pages;

public class ListPageModel : PageModel
{
    public List<IFriend> FriendsList {get;set;} = new List<IFriend>();
    public List<IAddress> AddressesList {get;set;} = new List<IAddress>();

    readonly IAddressesService _addressesService;
    public ListPageModel(IAddressesService addressesService)
    {
        _addressesService = addressesService;
    }
    public async Task<IActionResult> OnGet()
    {
        var x = await _addressesService.ReadAddressesAsync(true,false,null,0,10);
        AddressesList = x.PageItems;
        
        return Page();
    }
}

