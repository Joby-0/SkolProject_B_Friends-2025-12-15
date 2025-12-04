using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public SelectList CountryList { get; set; }

    public SelectList CityList { get; set; }

    readonly IFriendsService _friendsService;

    readonly IAdminService _adminService;

    public ListPageModel(IFriendsService friendsService, IAdminService adminService)
    {
        _friendsService = friendsService;
        _adminService = adminService;
    }

    public async Task<IActionResult> OnPost()
    {
        await PopulateSelect();
        await populatedFriendsListAsync();

        return RedirectToPage("/Listpage", new
        {
            pageNr = 0,
            selectedCountry = SelectedCountry,
            selectedCity = SelectedCity
        });
    }

    public async Task<IActionResult> OnGet()
    {
        await PopulateSelect();
        await populatedFriendsListAsync();
        return Page();
    }

    public async Task PopulateSelect()
    {
        var dbInfo = await _adminService.GuestInfoAsync();

        var countries = dbInfo.Item.Friends
            .Where(f => f.Country != null)
            .Select(f => f.Country)
            .Distinct()
            .ToList();

        CountryList = new SelectList(countries, SelectedCountry);

        if (!string.IsNullOrWhiteSpace(SelectedCountry))
        {
            var cities = dbInfo.Item.Friends
                .Where(f => f.Country == SelectedCountry)
                .Where(f => f.City != null)
                .Select(f => f.City)
                .Distinct()
                .ToList();

            // If current SelectedCity does not exist in the new cities list, reset it
            if (!cities.Contains(SelectedCity))
            {
                SelectedCity = null;
            }

            CityList = new SelectList(cities, SelectedCity);
        }
        else
        {
            SelectedCity = null; // no country selected, reset city
            CityList = new SelectList(Enumerable.Empty<string>());
        }
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
}

