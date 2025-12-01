using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;

namespace AppWebRazor.Pages;

public class DetailsModel : PageModel
{
    [BindProperty]
    public bool canEdit { get; set; } = false;
    readonly IFriendsService _friendsService;

    [BindProperty]
    public csFriend friend { get; set; }

    public DetailsModel(IFriendsService friendsService)
    {
        _friendsService = friendsService;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            var updateitem = new FriendCuDto()
            {
                FriendId = friend.FriendId,
                FirstName = friend.FirstName,
                LastName = friend.LastName,
                Email = friend.Email,
                Birthday = friend.Birthday
            };

            await _friendsService.UpdateFriendAsync(updateitem);

            return RedirectToPage("/Details", new { id = friend.FriendId, canedit = false });

        }
        catch (Exception ex)
        {

        }
        return Page();
    }

    public async Task<IActionResult> OnGet(string id, bool canedit)
    {

        try
        {
            Guid fId = Guid.Parse(id);
            var x = await _friendsService.ReadFriendAsync(fId, false);
            friend = (csFriend)x.Item;
            friend.FriendId = x.Item.FriendId;

            if (canedit)
            {
                canEdit = true;
            }
        }
        catch (Exception e)
        {

        }
        return Page();

    }
}

