using Microsoft.AspNetCore.Mvc.Rendering;
using Models.DTO;
using Models.Interfaces;

namespace AppWebMVC.Models;

public class ListViewModel()
{
    public string SelectedCountry { get; set; }

    public string SelectedCity { get; set; }

    public ResponsePageDto<IFriend> FriendsList { get; set; } = new ResponsePageDto<IFriend>();

    public SelectList CountryList { get; set; }

    public SelectList CityList { get; set; }

}