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

    public ListPageModel(IAddressesService addressesService, IFriendsService friendsService)
    {
        _addressesService = addressesService;
        _friendsService = friendsService;
    }

    public async Task<IActionResult> OnPost()
    {
        await populatedAddressListAsync();
        if (!string.IsNullOrWhiteSpace(SelectedCountry))
        {
            // Filter cities based on the selected country
            CityList = AddressesList
                .Where(a => a.Country == SelectedCountry)
                .Select(a => a.City)
                .Distinct()
                .ToList();

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

