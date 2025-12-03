using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppWebRazor.Pages;

public class ListPageModel : PageModel
{

    [BindProperty(SupportsGet = true)]
    public string SelectedCountry { get; set; }

    [BindProperty(SupportsGet = true)]

    public string SelectedCity { get; set; }
    [BindProperty(SupportsGet = true)]
    public int pageNr { get; set; } = 0;

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
        await populatedCitiesListAsync();
        return RedirectToPage("/Listpage", new
        {
            pageNr = 0,
            selectedCountry = SelectedCountry,
            selectedCity = SelectedCity
        });
    }

    public async Task<IActionResult> OnGet()
    {
        await populatedAddressListAsync();
        await populatedFriendsListAsync();
        await populatedCitiesListAsync();
        return Page();
    }

    public async Task populatedAddressListAsync()
    {
        var x = await _addressesService.ReadAddressesAsync(true, false, null, 0, 10);
        AddressesList = x.PageItems;
    }

    public async Task populatedFriendsListAsync()
    {
        if (!string.IsNullOrWhiteSpace(SelectedCountry) && string.IsNullOrWhiteSpace(SelectedCity))
        {
            FriendsList = await _friendsService.ReadFriendsAsync(true, true, SelectedCountry.ToLower(), pageNr, 10);
        }
        else if (!string.IsNullOrWhiteSpace(SelectedCountry) && !string.IsNullOrWhiteSpace(SelectedCity))
        {
            FriendsList = await _friendsService.ReadFriendsAsync(true, true, SelectedCity.ToLower(), pageNr, 10);
        }
    }
    public async Task populatedCitiesListAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedCountry))
        {
            CityList = new List<string>();
            SelectedCity = "";
            return;
        }

        var dbInfo = await _adminService.GuestInfoAsync();

        CityList = dbInfo.Item.Friends
            .Where(f => f.Country == SelectedCountry && !string.IsNullOrEmpty(f.City))
            .Select(f => f.City)
            .ToList();

        if (!string.IsNullOrEmpty(SelectedCity) && !CityList.Contains(SelectedCity))
        {
            SelectedCity = "";
        }
    }
}

