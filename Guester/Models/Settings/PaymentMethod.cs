using FontAwesome;
using System.ComponentModel;

namespace Guester.Models;

[MapTo("PaymentMethods")]
public class PaymentMethod:RealmObject
{
    [PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }= string.Empty;

    [MapTo("on_terminal")]
    public bool OnTerminal { get; set; }


    [MapTo("payment_type")]

    public int PaymentTypeRaw { get; set; }

 

    [MapTo("is_same_to_all")]
    public bool IsSameToAll { get; set; }

    [MapTo("commission_percentage")]
    public double CommissionPercentage { get; set; }

    [MapTo("sales_points")]
    public IList<PaymentMethodSalesPoint> PaymentMethodSalesPoints { get; }

    [MapTo("color")]
    public string Color { get; set; }= string.Empty;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get; set; }

    [MapTo("user_id")]
    public string UserID { get; set; } = string.Empty;


    [Ignored]
    public PaymentType PaymentType
    {
        get { return ((PaymentType)PaymentTypeRaw); }
        set { PaymentTypeRaw = (int)value; }
    }

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

  
    [Ignored]
    private decimal paymentSum { get; set; }

    [Ignored]
    public string Icon { get
        {
            return PaymentType switch
            {
                PaymentType.Card =>FontAwesomeIcons.CreditCard,
                PaymentType.Cash=> FontAwesomeIcons.MoneyBill,
                _ => FontAwesomeIcons.MoneyCheckDollar
            };
        }
    } 
   
    
    
    [Ignored]
    public decimal PaymentSum { get { return paymentSum; }  set { paymentSum = value;  var task = Task.Delay(100); task.Wait(); RaisePropertyChanged(nameof(PaymentSum)); } }

    //[Ignored]
    //public bool IsActive { get => IsActiveRaw; }

    //protected override void OnPropertyChanged(string propertyName)
    //{
    //    base.OnPropertyChanged(propertyName);
    //    if (propertyName == nameof(IsActiveRaw))
    //    {
    //        RaisePropertyChanged(nameof(IsActive));


    //    }

    //}
}




public class PaymentMethodSalesPoint : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }  

    [MapTo("sales_point_id")]
    public string SalesPointId { get; set; } = string.Empty;

    [MapTo("commission_percentage")]
    public double CommissionPercentage { get; set; }

}
