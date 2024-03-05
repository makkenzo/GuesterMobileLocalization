using MongoDB.Bson.Serialization.Attributes;

namespace Guester.Models;

/// <summary>
/// Subscribe need realm
/// </summary>

[MapTo("Products")]

public class Product : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;
    [MapTo("type")]
    public int TypeRaw { get; set; }

    [MapTo("parent_id")]
    public string ParentId { get; set; }


    [MapTo("name")]
    public string Name { get; set; } = string.Empty;

    [MapTo("category_id")]
    public Category Category { get; set; }

    [MapTo("premise_id")]
    public Premises Premises { get; set; }

    [MapTo("tax_id")]
    public Taxes Tax{ get; set; }
    [MapTo("picture")]
    public string Picture { get; set; } = string.Empty;
   [MapTo("modifiers_id")]
    public IList<Modifiers> Modifiers { get; }
    [MapTo("is_weight")]
    public bool IsWEight { get; set; } = false;
    [MapTo("is_different")]
    public bool IsDifferent { get; set; } = false;
    [MapTo("is_not_discount")]
    public bool IsNotDiscount { get; set; } = false;
    [MapTo("is_modify")]
    public bool IsModify { get; set; } = false;
    [MapTo("cost_price")]
    public decimal CostPrice { get; set; } = 0;
    [MapTo("markup_percent")]
    public double MarkupPercent { get; set; } = 0;

    [MapTo("tax_percent")]
    public double TaxPercent { get; set; } = 0;
    [MapTo("sum_without_price")]
    public decimal SumWithoutPrice { get; set; } = 0;

    [MapTo("total_sum")]
    public decimal TotalSum { get; set; } = 0;
    [MapTo("ean_thirteen")]
    public string BarCode { get; set; } = string.Empty;
    [MapTo("foreign_trade_code")]
    public string Code { get; set; } = string.Empty;
    [MapTo("foreign_special_code")]
    public string ForeignSpecialCode { get; set; } = string.Empty;
    [MapTo("cooking_process")]
    public string CookingProcess { get; set; } = string.Empty;
    [MapTo("cooking_min")]
    public int CookingMin { get; set; } = 0;
    [MapTo("cooking_sec")]
    public int CookingSec { get; set; } = 0;
    [MapTo("recipes")]
    public IList<Recipe> Recipes { get; }
    [MapTo("prices")]
    public IList<Price> Prices { get; }

    [MapTo("losses")]
    public IList<Loss> Losses { get; }


    [MapTo("unit")]
    public int UnitRaw { get; set; }

    [Ignored]
    public UnitType Unit { get => (UnitType)UnitRaw; set => UnitRaw = (int)value; }

    [Ignored]
    public Models.TypeCode Type { get => (TypeCode)TypeRaw; set => TypeRaw = (int)value; }

        [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

}
   
    public class Recipe : EmbeddedObject
    {

            [MapTo("_id")]
            public string Id { get; set; }
         [MapTo("cooking_process")]
        public string CookingProcess { get; set; } = String.Empty;

        [MapTo("cooking_method_id")]
        public IList<CookingMethod> CookingMethodsId { get; }



    [MapTo("gros")]
        public double Gros { get; set; } = 0;
        [MapTo("is_gros_net")]
        public bool IsGrosNet { get; set; } = true;
        [MapTo("net")]
        public double Net { get; set; } = 0;
        [MapTo("cost_price")]
        public decimal CostPrice { get; set; } = 0;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }
}

    public class Price : EmbeddedObject
    {
        [MapTo("_id")]
        public string Id { get; set; }
        [MapTo("sales_point_id")]
        public SalesPoint SalesPoint { get; set; }
        [MapTo("cost_price")]
        public decimal CostPrice { get; set; } = 0;
        [MapTo("markup_percent")]
        public double MarkupPercent { get; set; } = 0;
        [MapTo("sum_without_price")]
        public decimal sumWithoutPrice { get; set; } = 0;

        [MapTo("name")]
        public string Name { get; set; }


    [MapTo("ean_thirteen")]
    public string Barcode{ get; set; }
        [MapTo("tax_percent")]
        public double TaxPercent { get; set; } = 0;
        [MapTo("total_sum")]
        public decimal TotalSum { get; set; } = 0;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }


    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }
}
public class Loss :EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("cooking_method_id")]
    public string CookingMethodId { get; set; }
    [MapTo("percentage_losses")]
    public double PercentageLosses { get; set; }
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }
}