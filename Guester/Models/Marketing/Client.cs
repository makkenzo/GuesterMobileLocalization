namespace Guester.Models;

public class Client : RealmObject
{

    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [MapTo("brand_Id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("full_name")]
    public string FullName { get; set; } = string.Empty;

    [MapTo("gender")]
    public int GenderRaw { get; set; }

    [Ignored]
    public Gender Gender { get => (Gender)GenderRaw; set { GenderRaw = (int)value; } }

    [MapTo("date_of_birth")]
    public DateTimeOffset DateOfBirth { get; set; } = DateTimeOffset.Now;

    [Ignored]
    public DateTime BirthDay { get => DateOfBirth.DateTime; set => DateOfBirth = value; }

    //Сделать ссылку на  группу клиента
    [MapTo("client_group_id")]
    public ClientGroup ClientGroup { get; set; }

    [MapTo("phone_number")]
    public string PhoneNumber { get; set; }

    [MapTo("email")]
    public string Email { get; set; }

    [MapTo("description")]
    public string Description { get; set; }

    [MapTo("client_card")]
    public IList<ClientCard> ClientCard { get; }

    [MapTo("client_address")]
    public IList<ClientAddress> ClientAddress { get; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; } = DateTimeOffset.Now;

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

    [Ignored]
    public decimal TotalClientsSum { get => ClientCard.Sum(x => x.BonusSum); }

    [Ignored]
    public decimal DepositsSum { get => ClientCard.Sum(x => x.DepositSum); }

    [Ignored]
    public decimal TotalPersonalBonus { get => ClientCard.Sum(x => x.PersonalBonus); }
}


public class ClientCard : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("card_number")]
    public string CardNumber { get; set; } = string.Empty;

    [MapTo("bonus_sum")]
    public decimal BonusSum { get; set; } = 0;

    [MapTo("deposit_sum")]
    public decimal DepositSum { get; set; } = 0;

    [MapTo("personal_bonus")]
    public decimal PersonalBonus { get; set; } = 0;

    [MapTo("is_delete")]
    public bool IsDelete { get; set; } = false;
}


public class ClientAddress : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("country")]
    public string Country { get; set; } = string.Empty;

    [MapTo("city")]
    public string City { get; set; } = string.Empty;

    [MapTo("address")]
    public string Address { get; set; } = string.Empty;

    [MapTo("address_additional")]
    public string AddressAdditional { get; set; } = string.Empty;

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }


}
