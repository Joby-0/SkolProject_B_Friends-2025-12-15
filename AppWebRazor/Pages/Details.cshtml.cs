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
    public FriendCuDto friendCu { get; set; }

    public IFriend friend { get; set; }


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
            await _friendsService.UpdateFriendAsync(friendCu);
        }
        catch (Exception ex)
        {

        }
        return RedirectToPage("/Details", new
        {
            id = friendCu.FriendId,
            canedit = false
        });
    }
    public async Task<IActionResult> OnPostDeletePet(Guid petId)
    {
        friendCu.PetsId.Remove(petId);

        await _friendsService.UpdateFriendAsync(friendCu);
        return RedirectToPage("/Details", new
        {
            id = friendCu.FriendId,
            canedit = true
        });
    }
    public async Task<IActionResult> OnGet(string id, bool canedit)
    {

        try
        {
            Guid fId = Guid.Parse(id);
            var x = await _friendsService.ReadFriendAsync(fId, false);
            friend = x.Item;
            friendCu = new FriendCuDto(friend);

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

