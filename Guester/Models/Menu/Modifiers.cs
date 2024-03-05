using MongoDB.Bson.Serialization.Attributes;
using System.Linq;

namespace Guester.Models;


public sealed class Modifiers : RealmObject
{
    /// <summary>
    /// Modifier
    /// </summary>
    [MapTo("_id"),PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;
    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("is_modifier_required")]
    public bool IsModifierRequired { get; set; } = true;


    [MapTo("is_only_one")]
    public bool IsOnlyOne { get; set; } = true;
    [MapTo("min_number_of_modifiers")]
    public int MinNumberOfModifiers { get; set; }
    [MapTo("max_number_of_modifiers")]
    public int MaxNumberOfModifiers { get; set; }
    [MapTo("modifier_goods")]
    public IList<Goods> ModifiersList { get; }

    [Ignored]
    public object SelectedGods  
    { 
        get=>_selectedGods;
        set
        {
            if(value != null && _selectedGods != value)
            {
                var modif = HomePageViewModel.getInstanceOrderDetailPage().SelectedModifiers;
                if (modif.Contains((Goods)SelectedGods))
                    modif.Remove((Goods)SelectedGods);
                _selectedGods = value;

                modif.Add((Goods)value);

                RaisePropertyChanged(nameof(SelectedGods));
               HomePageViewModel.getInstanceOrderDetailPage().PriceModificationsUpdate();
            
              
               
            }
        }
    }

    [Ignored]
    private object _selectedGods { get; set; } 

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }=DateTimeOffset.Now;

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }


    //protected override void OnPropertyChanged(string propertyName)
    //{
    //    base.OnPropertyChanged(propertyName);
    //    if (propertyName == nameof(SelectedGods))
    //    {
    //        if (SelectedGods != null)
    //        {
    //            ServiceHelper.GetService<OrderDetailPageViewModel>().PriceModificationsUpdate();
    //        }
    //    }
    //}

}

public partial class Goods : EmbeddedObject
{

    [MapTo("_id")]
    public string Id { get; set; }=ObjectId.GenerateNewId().ToString();
    [MapTo("can_be_written_off")]
    public bool CanBeWrittenOff { get; set; }
    [MapTo("name")]
    public string Name { get; set; } = string.Empty;
    [MapTo("picture")]
    public string Picture { get; set; } = string.Empty;
    [MapTo("price")]
    public decimal Price { get; set; } = 0;
    [MapTo("product_id")]
    public Product Product { get; set; }
    [MapTo("gros")]
    public double Gros { get; set; } = 0;
    [MapTo("cost_price")]
    public decimal CostPrice { get; set; } = 0;
    [MapTo("sort_index")]
    public int SortIndex { get; set; } = 0;

  
    [MapTo("count")]
    public  int Amount { get; set; }

    [Ignored]
    private int count { get; set; }
    [Ignored]
    public int Count { get => count; set { count = value; RaisePropertyChanged(nameof(Count)); } } 

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }=DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }= DateTimeOffset.Now;

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;


    



}
