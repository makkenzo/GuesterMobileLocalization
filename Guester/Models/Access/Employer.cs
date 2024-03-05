
using MongoDB.Bson.Serialization.Attributes;

namespace Guester.Models;


[MapTo("Employees")]
public class Employer:RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("post_id")]
    public Post Post { get; set; } = new(); // курьер

    [MapTo("full_name")]
    public string FullName { get; set;} = string.Empty;

    [MapTo("pin_code")]
    public string PinCode { get; set; } = string.Empty;
    [MapTo("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [MapTo("telegram_id")]
    public string TelegramId { get; set; } = string.Empty;
    [MapTo("login")]
    public string Login { get; set; } = string.Empty;
    [MapTo("password")]
    public string Password { get; set; } = string.Empty;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get;set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
