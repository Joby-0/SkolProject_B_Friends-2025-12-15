using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.DTO;
using Services.Interfaces;

namespace AppWebRazor.Pages;

public class OverviewModel : PageModel
{

    readonly IAdminService _adminService;

    public ResponseItemDto<GstUsrInfoAllDto> dbInfo { get; set; }

    public List<CityOverview> CityStats { get; set; } = new List<CityOverview>();

    [BindProperty(SupportsGet = true)]
    public string? SelectedCountry { get; set; }

    public OverviewModel(IAdminService adminService)
    {
        _adminService = adminService;
    }
    public async Task OnGet(string? country = null)
    {
        dbInfo = await _adminService.GuestInfoAsync();

        if (!string.IsNullOrWhiteSpace(country))
        {
            var friendInfo = dbInfo.Item.Friends.Where(f => f.Country == country && !string.IsNullOrEmpty(f.City)).ToList();

            var petInfo = dbInfo.Item.Pets
                .Where(p => p.Country == country && !string.IsNullOrEmpty(p.City))
                .ToList();

            foreach(var city in friendInfo)
            {
                CityStats.Add(new CityOverview()
                {
                   City = city.City,
                   NrFriends = city.NrFriends,
                   NrPets = petInfo.Where(p => p.City == city.City).Sum(p => p.NrPets)
                });
            }
        }
    }


}

public class CityOverview
{
    public string City { get; set; }
    public int NrFriends { get; set; }
    public int NrPets { get; set; }
}

