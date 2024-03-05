using MongoDB.Bson.Serialization.Attributes;
using System.Reflection.Metadata;

namespace Guester.Models;

[MapTo("Posts")]
public class Post : RealmObject
{
    [MapTo("_id"),PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("description")]
    public string Description { get; set; }

  
    [MapTo("is_terminal")]
    public bool IsTerminal { get; set; } = false;

    [MapTo("is_administrator")]
    public bool IsAdministrator { get; set; } = false;



    ////[MapTo("stat")]
    ////public int StatRaw { get; set; }
    //[MapTo("finansial")]
    //public int FinansialRaw { get; set; }
    //[MapTo("menu")]
    //public int MenuRaw { get; set; }
    //[MapTo("warehouse")]
    //public int WarehouseRaw { get; set; }
    //[MapTo("marketing")]
    //public int MarketingRaw { get; set; }
    //[MapTo("accessControl")]
    //public int AccessControlRaw { get; set; }
    //[MapTo("settings")]
    //public int SettingsRaw { get; set; }
    //[MapTo("security")]
    //public int SecurityRaw { get; set; }

    //[Ignored]
    //public PostEnum State { get => (PostEnum)StatRaw; set => StatRaw = (int)value; } 
    //[Ignored]
    //public PostEnum Finansial { get => (PostEnum)StatRaw; set => StatRaw = (int)value; } 
    //[Ignored]
    //public PostEnum Menu { get => (PostEnum)StatRaw; set => StatRaw = (int)value; }
    //[Ignored]
    //public PostEnum Warehouse { get => (PostEnum)StatRaw; set => StatRaw = (int)value; }
    //[Ignored]
    //public PostEnum Marketing { get => (PostEnum)StatRaw; set => StatRaw = (int)value; } 
    //[Ignored]
    //public PostEnum AccessControl { get => (PostEnum)StatRaw; set => StatRaw = (int)value; } 
    //[Ignored]
    //public PostEnum Settings { get => (PostEnum)StatRaw; set => StatRaw = (int)value; } 
    //[Ignored]
    //public PostEnum Security { get => (PostEnum)StatRaw; set => StatRaw = (int)value; }

    [MapTo("is_confirmInstall")]
    public bool IsConfirmInstall { get; set; } = false;

    [MapTo("is_courier")]
    public bool IsCourier { get; set; } = false;

    [MapTo("fixed_in_an_hour")]

    public decimal FixedInAnHour { get; set; } = 0;

    [MapTo("fixed_per_shift")]
    public decimal FixedPerShift { get; set; } = 0;

    [MapTo("fixed_per_month")]
    public decimal FixedPerMonth { get; set; } = 0;


    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

    [MapTo("personal_sales")]
    public IList<PersonalSale> PersonalSales { get; }

    [MapTo("shift_sales")]
    public IList<ShiftSale> ShiftSales { get; }


    [MapTo("access_details")]
    public IList<AccessDetail> AccessDetails { get; }

}



public class AccessDetail : EmbeddedObject
{
    [MapTo("page")]
    public int PageRaw { get; set; }

    [Ignored]
    public PageEnum Page
    {
        get { return (PageEnum)PageRaw; }
        set { PageRaw = (int)value; }
    }

    [MapTo("page_detail")]
    public IList<PageDetail> PageDetail { get; }
    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}
public class PageDetail : EmbeddedObject
{
    [MapTo("sub_page")]
    public string SubPage { get; set; } = string.Empty;
    [MapTo("access")]
    public int AccessRaw { get; set; }

    [Ignored]
    public PostEnum Access { get => (PostEnum)AccessRaw; set => AccessRaw = (int)value;  }
    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}

public class PersonalSale:EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("category_id")]
    public string CategoryId { get; set; }
    [MapTo("percent_personal")]
    public double PercentPersonal { get; set; } = 0;
    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}
public class ShiftSale:EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("category_id")]
    public string CategoryId { get; set; }
    [MapTo("percent_shift")]
    public double PercentShift { get; set; } = 0;
    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}
