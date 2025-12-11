using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppWebMVC.Models;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models.DTO;
using Services.Interfaces;

namespace AppWebMVC.Controllers;

public class CountryController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IFriendsService _friendsService;
    private readonly IAddressesService _addressesService;

    public CountryController(IAdminService adminService, IFriendsService friendsService, IAddressesService addressesService)
    {
        _adminService = adminService;
        _friendsService = friendsService;
        _addressesService = addressesService;
    }
    public async Task<ActionResult> OverviewAsync(string country = null)
    {
        var dbInfo = await _adminService.GuestInfoAsync();
        var viewModel = new OverviewViewModel
        {
            DbInfo = dbInfo,
            SelectedCountry = country
        };

        if (!string.IsNullOrWhiteSpace(country))
        {
            var friendInfo = dbInfo.Item.Friends
                .Where(f => f.Country == country && !string.IsNullOrEmpty(f.City))
                .ToList();

            var petInfo = dbInfo.Item.Pets
                .Where(p => p.Country == country && !string.IsNullOrEmpty(p.City))
                .ToList();

            foreach (var city in friendInfo)
            {
                viewModel.CityStats.Add(new CityOverview
                {
                    City = city.City,
                    NrFriends = city.NrFriends,
                    NrPets = petInfo.Where(p => p.City == city.City).Sum(p => p.NrPets)
                });
            }
        }

        return View(viewModel);
    }


    [HttpGet]
    public async Task<IActionResult> List(string? selectedCountry = null, string? selectedCity = null, int pageNr = 0)
    {
        var model = new ListViewModel
        {
            SelectedCountry = selectedCountry,
            SelectedCity = selectedCity,
        };
        var dbinfo = await _adminService.GuestInfoAsync();

        model.CountryList = new SelectList(dbinfo.Item.Friends.Where(f => f.Country != null).Select(f => f.Country).Distinct().ToList(), selectedCountry);

        if (!string.IsNullOrWhiteSpace(selectedCountry))
        {
            var cities = dbinfo.Item.Friends.Where(f => f.Country == selectedCountry).Where(f => f.City != null).Select(f => f.City).Distinct().ToList();

            if (!cities.Contains(selectedCity))
            {
                selectedCity = null;
            }

            model.CityList = new SelectList(cities, selectedCity);
        }
        else
        {
            selectedCity = null;

            model.CityList = new SelectList(Enumerable.Empty<string>());

        }

        if (!string.IsNullOrWhiteSpace(selectedCountry) && string.IsNullOrWhiteSpace(selectedCity))
        {
            model.FriendsList = await _friendsService.ReadFriendsAsync(true, true, selectedCountry.ToLower(), pageNr, 10);
        }
        else if (!string.IsNullOrWhiteSpace(selectedCountry) && !string.IsNullOrWhiteSpace(selectedCity))
        {
            model.FriendsList = await _friendsService.ReadFriendsAsync(true, true, selectedCity.ToLower(), pageNr, 10);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ListAsync(ListViewModel model)
    {

        // Normalize empty values
        var country = string.IsNullOrWhiteSpace(model.SelectedCountry) ? null : model.SelectedCountry;
        var city = string.IsNullOrWhiteSpace(model.SelectedCity) ? null : model.SelectedCity;

        var dbinfo = await _adminService.GuestInfoAsync();

        model.CountryList = new SelectList(dbinfo.Item.Friends.Where(f => f.Country != null).Select(f => f.Country).Distinct().ToList(), country);

        if (!string.IsNullOrWhiteSpace(country))
        {
            var cities = dbinfo.Item.Friends.Where(f => f.Country == country)
                                           .Where(f => f.City != null)
                                           .Select(f => f.City)
                                           .Distinct()
                                           .ToList();

            if (!cities.Contains(city))
            {
                city = null;
            }
        }
        else
        {
            city = null;
        }

        // CASE 1: both null → no query string
        if (country == null && city == null)
            return RedirectToAction(nameof(List));

        // CASE 2: only country selected → only selectedCountry in URL
        if (country != null && city == null)
            return RedirectToAction(nameof(List), new { selectedCountry = country });

        // CASE 3: both selected → include both
        return RedirectToAction(nameof(List), new { selectedCountry = country, selectedCity = city });
    }


    public async Task<ActionResult> Seed(int nrOfItems)
    {
        var viewModel = new SeedViewModel();
        try
        {
            await _adminService.SeedAsync(nrOfItems);
        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, bool canedit)
    {
        var viewModel = new DetailsViewModel()
        {
            canEdit = canedit
        };

        var item = await _friendsService.ReadFriendAsync(id, false);

        viewModel.FriendForm = new FriendFormModel(item.Item);

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Save(DetailsViewModel viewModel)
    {
        var friend = viewModel.FriendForm;

        if (!ModelState.IsValid)
        {
            viewModel.HasValidationErrors = true;
            viewModel.ValidationErrorMsgs = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);

            return View("Details", viewModel);
        }
        try
        {
            var dbInfo = await _friendsService.ReadFriendAsync(friend.FriendId, false);
            var original = dbInfo.Item.Address;

            if (original.City != friend.Address.City ||
                original.Country != friend.Address.Country ||
                original.StreetAddress != friend.Address.StreetAddress ||
                original.ZipCode != friend.Address.ZipCode)
            {
                friend.Address.Status = StatusIM.Modified;
            }

            if (friend.Address.Status == StatusIM.Modified)
            {
                AddressCuDto newAddress = new AddressCuDto()
                {
                    City = friend.Address.City,
                    ZipCode = friend.Address.ZipCode,
                    Country = friend.Address.Country,
                    StreetAddress = friend.Address.StreetAddress
                };
                var addedAddress = await _addressesService.CreateAddressAsync(newAddress);
                friend.Address.AddressId = addedAddress.Item.AddressId;
            }

            friend.Pets.RemoveAll(p => p.Status == StatusIM.Deleted);
            friend.Quotes.RemoveAll(p => p.Status == StatusIM.Deleted);

            FriendCuDto UppdatedFriend = new FriendCuDto()
            {
                FriendId = friend.FriendId,
                FirstName = friend.FirstName,
                LastName = friend.LastName,
                Email = friend.Email,
                Birthday = friend.Birthday,

                AddressId = friend.Address.AddressId,

                PetsId = friend.Pets.Select(p => p.PetId).ToList(),
                QuotesId = friend.Quotes.Select(q => q.QuoteId).ToList()
            };
            await _friendsService.UpdateFriendAsync(UppdatedFriend);
            viewModel.canEdit = false;

        }
        catch (Exception ex)
        {
            viewModel.ErrorMessage = ex.Message;
        }

        var x = await _friendsService.ReadFriendAsync(friend.FriendId, false);
        viewModel.FriendForm = new FriendFormModel(x.Item);

        return View("Details", viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(DetailsViewModel viewModel, Guid itemId, string itemType)
    {
        viewModel.canEdit = true;
        if (itemType == "pet")
            viewModel.FriendForm.Pets.First(x => x.PetId == itemId).Status = StatusIM.Deleted;

        if (itemType == "quote")
            viewModel.FriendForm.Quotes.First(x => x.QuoteId == itemId).Status = StatusIM.Deleted;

        return View("Details", viewModel);
    }

}