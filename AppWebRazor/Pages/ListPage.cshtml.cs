using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppWebRazor.Pages;

public class ListPageModel : PageModel
{

    [BindProperty]
    public string SelectedCountry { get; set; }

    [BindProperty]
    public string SelectedCity { get; set; }

    public ResponsePageDto<IFriend> FriendsList { get; set; } = new ResponsePageDto<IFriend>();

    public List<IAddress> AddressesList { get; set; } = new List<IAddress>();

    public List<string> CityList { get; set; } = new List<string>();

    readonly IAddressesService _addressesService;
    readonly IFriendsService _friendsService;

    readonly IAdminService _adminService;

    public ListPageModel(IAddressesService addressesService, IFriendsService friendsService, IAdminService adminService)
    {
        _addressesService = addressesService;
        _friendsService = friendsService;
        _adminService = adminService;
    }

    public async Task<IActionResult> OnPost()
    {
        await populatedAddressListAsync();
        if (!string.IsNullOrWhiteSpace(SelectedCountry))
        {
            var dbInfo = await _adminService.GuestInfoAsync();

            
            CityList = dbInfo.Item.Friends.Where(f => f.Country == SelectedCountry && !string.IsNullOrEmpty(f.City)).ToList().Select(c => c.City).ToList();

            // Reset city if country changed
            if (SelectedCity != null && !CityList.Contains(SelectedCity))
            {
                SelectedCity = "";
            }
        }
        else
        {
            CityList = new List<string>();
            SelectedCity = "";
        }
        if (!string.IsNullOrWhiteSpace(SelectedCountry) && string.IsNullOrWhiteSpace(SelectedCity))
        {
            FriendsList = await _friendsService.ReadFriendsAsync(true, true, SelectedCountry.ToLower(), 0, 20);
        }
        else if (!string.IsNullOrWhiteSpace(SelectedCountry) && !string.IsNullOrWhiteSpace(SelectedCity))
        {
            FriendsList = await _friendsService.ReadFriendsAsync(true, true, SelectedCity.ToLower(), 0, 20);
        }

        return Page();
    }

    public void ToDetailsPage(Guid id)
    {
        Console.WriteLine(id);
    }

    public async Task<IActionResult> OnGet()
    {
        await populatedAddressListAsync();

        return Page();
    }

    public async Task populatedAddressListAsync()
    {
        var x = await _addressesService.ReadAddressesAsync(true, false, null, 0, 10);
        AddressesList = x.PageItems;
    }
}

