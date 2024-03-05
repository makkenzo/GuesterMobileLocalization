using MongoDB.Bson.Serialization.Attributes;

namespace Guester.Models;

[MapTo("Menus")]
public class Menu:RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; }

    [MapTo("sales_points_id")]
    public IList<SalesPoint> SalePoints { get; }

    [MapTo("name")]
    public string Name { get; set; }= string.Empty;

    [MapTo("is_time")]
    public bool IsTime { get; set; }

    [MapTo("start_time")]
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;

    [MapTo("end_time")]
    public DateTimeOffset EndTime { get; set; }= DateTimeOffset.Now;


    [MapTo("menu_items")]
    public IList<Items> Items { get;  } 

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; } = false;

}

/*[MapTo("Items")]*/
public class Items : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("name")]
    public string Name { get; set; }= string.Empty;

    [MapTo("description")]
    public string Description { get; set; }=string.Empty;

    [MapTo("is_main")]
    public bool IsMain { get; set; }

    [MapTo("priority")]
    public int Priority { get; set; }

    [MapTo("product_id")]
    public Product Product { get; set; }



    [MapTo("picture")]
    public string Picture { get; set; }= string.Empty;

    [MapTo("is_self_price")]
    public bool IsSelfPrice { get; set; }//надо сделать

    [MapTo("price")]
    public decimal Price { get; set; }

    [MapTo("is_discount")]
    public bool IsDiscount { get; set; }

    [MapTo("is_active")]
    public bool IsActive { get; set; }


    [MapTo("parent_id")]

    public string ParentItemId { get; set; }


    [Ignored]
    private int count { get; set; }
    [Ignored]
    public int Count { get { return count; } set { count = value; RaisePropertyChanged(nameof(Count)); }  }

    [MapTo("is_category")]
    public bool IsCategory { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }=DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get;set; }=DateTimeOffset.Now;

 

}



/*[Ignored]
public partial  class VisualItem:ObservableObject
{

    [ObservableProperty]
     string name;   
    public  int Priority { get; set; }

    public bool IsCategory { get; set; }

    public Category ParentCategory { get; set; }

    public Product Product { get; set; }

    [ObservableProperty]
     string picture;


    public bool IsSelfPrice { get; set; }

    public decimal Price { get; set; }

    public bool IsDiscount { get; set; }
}*/