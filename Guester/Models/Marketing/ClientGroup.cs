
namespace Guester.Models;

[MapTo("ClientGroups")]
public class ClientGroup : RealmObject
{
    [PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
 
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }


    [MapTo("loyalty_program_id")]
    public LoyaltyProgram LoyaltyProgram { get; set; } = new();

    [MapTo("value")]
    public double Value { get; set; } = 0;

    [MapTo("bonus_on_birthday")]
    public decimal BonusOnBirthday { get; set; } = 0;

    [MapTo("is_client_deposit")]
    public bool IsClientDeposit { get; set; } = false;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
