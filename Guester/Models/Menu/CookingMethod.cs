namespace Guester.Models;

[MapTo("CookingMethods")]
public class CookingMethod : RealmObject
{
    [PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;
    [MapTo("name")]
    public string Name { get; set; } = string.Empty;
    [MapTo("default_percent")]
    public double DefaultPercent { get; set; } = 0;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
