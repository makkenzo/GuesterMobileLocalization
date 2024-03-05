namespace Guester.Models;


public class LoyaltyProgramsException : RealmObject
{
    [MapTo("_id"),PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("is_category")]
    public bool IsCategory { get; set; } = false;

    [MapTo("products_id")]
    public IList<Product> Products { get; }
    [MapTo("categories_id")]
    public IList<Category> Categories { get; }


    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
