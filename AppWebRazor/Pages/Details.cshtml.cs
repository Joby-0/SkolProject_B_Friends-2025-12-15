using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    readonly IAddressesService _addressesService;



    [BindProperty]
    public FriendFormModel FriendForm { get; set; } = new();

    public string? ErrorMessage { get; set; } = null;

    //For Server Side Validation set by IsValid()
    public bool HasValidationErrors { get; set; }
    public IEnumerable<string> ValidationErrorMsgs { get; set; }
    public IEnumerable<KeyValuePair<string, ModelStateEntry>> InvalidKeys { get; set; }

    public DetailsModel(IFriendsService friendsService, IAddressesService addressesService)
    {
        _friendsService = friendsService;
        _addressesService = addressesService;
    }

    public async Task<IActionResult> OnPostSave()
    {
        if (!IsValid())
        {
            return Page();
        }
        try
        {
            var dbInfo = await _friendsService.ReadFriendAsync(FriendForm.FriendId, false);
            var original = dbInfo.Item.Address;

            if (original.City != FriendForm.Address.City ||
                original.Country != FriendForm.Address.Country ||
                original.StreetAddress != FriendForm.Address.StreetAddress ||
                original.ZipCode != FriendForm.Address.ZipCode)
            {
                FriendForm.Address.Status = StatusIM.Modified;
            }

            if (FriendForm.Address.Status == StatusIM.Modified)
            {
                AddressCuDto newAddress = new AddressCuDto()
                {
                    City = FriendForm.Address.City,
                    ZipCode = FriendForm.Address.ZipCode,
                    Country = FriendForm.Address.Country,
                    StreetAddress = FriendForm.Address.StreetAddress
                };
                var addedAddress = await _addressesService.CreateAddressAsync(newAddress);
                FriendForm.Address.AddressId = addedAddress.Item.AddressId;
            }

            FriendForm.Pets.RemoveAll(p => p.Status == StatusIM.Deleted);
            FriendForm.Quotes.RemoveAll(p => p.Status == StatusIM.Deleted);

            FriendCuDto UppdatedFriend = new FriendCuDto()
            {
                FriendId = FriendForm.FriendId,
                FirstName = FriendForm.FirstName,
                LastName = FriendForm.LastName,
                Email = FriendForm.Email,
                Birthday = FriendForm.Birthday,

                AddressId = FriendForm.Address.AddressId,

                PetsId = FriendForm.Pets.Select(p => p.PetId).ToList(),
                QuotesId = FriendForm.Quotes.Select(q => q.QuoteId).ToList()
            };
            await _friendsService.UpdateFriendAsync(UppdatedFriend);
            canEdit = false;

        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        return Page();
    }
    public IActionResult OnPostDelete(Guid itemId, string itemType)
    {
        if (itemType == "pet")
        {
            var pet = FriendForm.Pets.First(x => x.PetId == itemId);
            pet.Status = StatusIM.Deleted;
        }
        else if (itemType == "quote")
        {
            var quote = FriendForm.Quotes.First(x => x.QuoteId == itemId);
            quote.Status = StatusIM.Deleted;
        }
        canEdit = true;

        return Page();
    }


    public async Task<IActionResult> OnGet(string id, bool canedit)
    {

        try
        {
            Guid fId = Guid.Parse(id);
            var x = await _friendsService.ReadFriendAsync(fId, false);
            FriendForm = new FriendFormModel(x.Item);

            if (canedit)
            {
                canEdit = true;
            }
        }
        catch (Exception e)
        {
            ErrorMessage = e.Message;
        }
        return Page();

    }

    #region Server Side Validation
    private bool IsValid(string[] validateOnlyKeys = null)
    {
        InvalidKeys = ModelState
           .Where(s => s.Value.ValidationState == ModelValidationState.Invalid);

        if (validateOnlyKeys != null)
        {
            InvalidKeys = InvalidKeys.Where(s => validateOnlyKeys.Any(vk => vk == s.Key));
        }

        ValidationErrorMsgs = InvalidKeys.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage);
        HasValidationErrors = InvalidKeys.Any();

        return !HasValidationErrors;
    }
    #endregion
}

public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }
public class PetFormModel
{
    public StatusIM Status { get; set; } = StatusIM.Unchanged;
    public Guid PetId { get; set; }
    public string Name { get; set; }
    public AnimalMood Mood { get; set; }
    public AnimalKind Kind { get; set; }

    // Copy constructor
    public PetFormModel(PetFormModel original)
    {
        PetId = original.PetId;
        Name = original.Name;
        Mood = original.Mood;
        Kind = original.Kind;
    }

    // Domain model => InputModel constructor
    public PetFormModel(IPet original)
    {
        PetId = original.PetId;
        Name = original.Name;
        Mood = original.Mood;
        Kind = original.Kind;
    }

    // InputModel => Domain model
    public Pet UpdateModel(Pet model)
    {
        model.PetId = PetId;
        model.Name = Name;
        model.Mood = Mood;
        model.Kind = Kind;
        return model;
    }

    // Parameterless constructor (required for model binding)
    public PetFormModel() { }
}

public class QuoteFormModel
{
    public Guid QuoteId { get; set; }
    public string QuoteText { get; set; }
    public string Author { get; set; }
    public StatusIM Status { get; set; } = StatusIM.Unchanged;

    public QuoteFormModel(IQuote original)
    {
        QuoteId = original.QuoteId;
        QuoteText = original.QuoteText;
        Author = original.Author;
    }

    public QuoteFormModel(QuoteFormModel original)
    {
        QuoteId = original.QuoteId;
        QuoteText = original.QuoteText;
        Author = original.Author;
    }
    public QuoteFormModel() { }
}

public class FriendFormModel
{
    public StatusIM Status { get; set; } = StatusIM.Unchanged;


    public Guid FriendId { get; set; }

    [Required(ErrorMessage = "You must provide a first Name")]
    public string FirstName { get; set; }
    [Required(ErrorMessage = "You must provide a last name")]
    public string LastName { get; set; }
    [Required(ErrorMessage = "You must provide a email")]
    public string Email { get; set; }
    [Required(ErrorMessage = "You must provide a birthday")]
    public DateTime? Birthday { get; set; }

    public AddressFormModel Address { get; set; }
    public List<PetFormModel> Pets { get; set; } = new();
    public List<QuoteFormModel> Quotes { get; set; } = new();


    // Copy constructor
    public FriendFormModel(FriendFormModel original)
    {
        FriendId = original.FriendId;
        FirstName = original.FirstName;
        LastName = original.LastName;
        Email = original.Email;
        Birthday = original.Birthday;
        Address = original.Address;
        Pets = original.Pets.Select(p => new PetFormModel(p)).ToList();
        Quotes = original.Quotes.Select(q => new QuoteFormModel(q)).ToList();
    }

    // Domain model => InputModel constructor
    public FriendFormModel(IFriend original)
    {
        FriendId = original.FriendId;
        FirstName = original.FirstName;
        LastName = original.LastName;
        Email = original.Email;
        Birthday = original.Birthday;

        Address = new AddressFormModel(original.Address);
        Pets = original.Pets?.Select(p => new PetFormModel(p)).ToList() ?? new List<PetFormModel>();
        Quotes = original.Quotes?.Select(p => new QuoteFormModel(p)).ToList() ?? new List<QuoteFormModel>();

    }

    // InputModel => Domain model
    public IFriend UpdateModel(IFriend model)
    {
        model.FriendId = FriendId;
        model.FirstName = FirstName;
        model.LastName = LastName;
        model.Email = Email;
        model.Birthday = Birthday;

        Address.AddressId = Address.AddressId;


        if (model.Pets == null)
            model.Pets = new List<IPet>();
        if (model.Quotes == null)
            model.Quotes = new List<IQuote>();

        return model;
    }

    // Parameterless constructor
    public FriendFormModel() { }
}


public class AddressFormModel
{
    public StatusIM Status { get; set; } = StatusIM.Unchanged;

    public Guid AddressId { get; set; }
    [Required(ErrorMessage = "You must provide a street")]
    public string StreetAddress { get; set; }
    [Required(ErrorMessage = "You must provide a zipcode")]
    public int ZipCode { get; set; }
    [Required(ErrorMessage = "You must provide a city")]
    public string City { get; set; }
    [Required(ErrorMessage = "You must provide a country")]
    public string Country { get; set; }

    public List<Guid> FriendsId { get; set; } = new();

    // Parameterless constructor (required for model binding)
    public AddressFormModel() { }

    // Copy constructor
    public AddressFormModel(AddressFormModel original)
    {
        AddressId = original.AddressId;
        StreetAddress = original.StreetAddress;
        ZipCode = original.ZipCode;
        City = original.City;
        Country = original.Country;
        FriendsId = original.FriendsId?.ToList();
        Status = original.Status;
    }

    // Create InputModel from domain model
    public AddressFormModel(IAddress org)
    {
        AddressId = org.AddressId;
        StreetAddress = org.StreetAddress;
        ZipCode = org.ZipCode;
        City = org.City;
        Country = org.Country;

        FriendsId = org.Friends?.Select(i => i.FriendId).ToList() ?? new List<Guid>();
    }

    // Update domain model from InputModel
    public Address UpdateModel(Address model)
    {
        model.AddressId = AddressId;
        model.StreetAddress = StreetAddress;
        model.ZipCode = ZipCode;
        model.City = City;
        model.Country = Country;

        // You will probably handle Friends → Address mapping somewhere else
        return model;
    }



}

