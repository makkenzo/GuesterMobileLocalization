using MongoDB.Bson.Serialization.Attributes;

namespace Guester.Models;

[MapTo("Categories")]
public class Category : RealmObject
{
    [PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("type")]
    public int CategoryTypeRaw { get; set; }

    [Ignored]
    public CategoryType CategoryType { get => (CategoryType)CategoryTypeRaw; set=> CategoryTypeRaw=(int)value; }
    [MapTo("name")]
    public string Name { get; set; } = string.Empty;
    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("tax_id")]
    public Taxes Tax { get; set; }

    [MapTo("parent_id")]
    public Category CategoryParent { get; set; }
    [MapTo("picture")]
    public string Picture { get; set; } = string.Empty;
    [MapTo("sales_points_id")]
    public IList<SalesPoint> SalesPoints { get; }

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }



    /*    [BsonIgnore]
        public List<Category> children { get; set; }*/
}
