using Models.DTO;

namespace AppWebMVC.Models;

public class OverviewViewModel
{
    public ResponseItemDto<GstUsrInfoAllDto> DbInfo { get; set; }
    public List<CityOverview> CityStats { get; set; } = new List<CityOverview>();
    public string SelectedCountry { get; set; }
}

public class CityOverview
{
    public string City { get; set; }
    public int NrFriends { get; set; }
    public int NrPets { get; set; }
}
