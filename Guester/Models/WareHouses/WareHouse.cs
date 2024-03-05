namespace Guester.Models;


[MapTo("warehouses")]
public class WareHouse : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; }


    [MapTo("brand_id")]
    public string BrandID { get; set; }

    [MapTo("sales_point_id")]
    public string SalePointId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("address")]
    public string Address { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTime.Now;
    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; } = DateTime.Now;
    [MapTo("user_id")]
    public string UserID { get; set; }

    
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
