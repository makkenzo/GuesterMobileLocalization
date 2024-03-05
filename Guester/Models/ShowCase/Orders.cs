using FontAwesome;
using Guester.Resources;
using MongoDB.Bson.Serialization.Attributes;
using MvvmHelpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Guester.Models;



public class Orders : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("sales_point_id")]
    public string SalePointId { get; set; } = string.Empty;

    [MapTo("brand_id")]
    public string BrandId { private get; set; }

    [MapTo("name")]
    public string Name { get; set; } = string.Empty;
    //[MapTo("delivery_zone_id")]
    //public DeliveryZone DeliveryZone { get; set; }

    [MapTo("shift_id")]
    public string ShiftId { get; set; }


    [MapTo("is_online")]
    public bool IsOnline { get; set; }

    [MapTo("sale_id")]
    public string SaleId { get; set; } = string.Empty;

    [MapTo("order_number")]
    public int Number { get; set; }

    [MapTo("logs")]
    public IList<Logs> Logs { get; }

    [MapTo("order_sales")]
    public IList<OrderSale> OrderSales { get; }

    [MapTo("order_delivery")]
    public OrderDelivery OrderDelivery { get; set; } = new();

    [MapTo("order_receipt")]
    public OrderReceipt OrderReceipt { get; set; } = new();

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get; set; } = DateTimeOffset.Now;

    [MapTo("user_id")]
    public string CreaterEmpId { get; set; }

    [MapTo("guest_count")]
    public int GuestCount { get; set; }


    [MapTo("order_source_id")]
    public Detail Detail { get; set; }

    [MapTo("client_id")]
    public Client Client { get; set; }

    [Ignored]
    public Employer Employer { get; set; } = new();

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }




    [Ignored]
    public string DisplayName { get => CreationDate.Date == DateTimeOffset.Now.Date ? AppResources.Today : AppResources.PreviousDays; }



    [Ignored]
    public OrderStatus OrderStatus
    {
        get { return OrderStates.LastOrDefault()?.OrderStateEnum ?? OrderStatus.New; }
        set
        {
            if (OrderStates.ToList().Any(x => x.OrderStateEnum == value))
                return;
            OrderStates.Add(new OrderStatuses() { OrderStateEnum = value });

        }
    }

    [Ignored]
    public string OrderStatusToString
    {
        get =>
            OrderStatus switch
            {
                OrderStatus.New => AppResources.NewLabel,
                OrderStatus.Deleted => AppResources.WasDeleted,
                //OrderStatus.Precheck => "Предчек",
                OrderStatus.Closed => AppResources.PaidLabel,
                OrderStatus.Success => "",
                OrderStatus.Delivery => OrderDelivery.DeliveryStatus switch
                {
                    DeliveryStatus.New => AppResources.NewLabel,
                    DeliveryStatus.Success => AppResources.Succes,
                    DeliveryStatus.OnWay => AppResources.InWay,
                    DeliveryStatus.Delivered => AppResources.DeliveredLabel,
                    DeliveryStatus.Closed => AppResources.PaidLabel,
                },
                _ => ""


            };
    }

    [Ignored]
    public string OrderTypeToString
    {
        get
        {
            if (Detail is null) return "";
            return Detail?.OrderType switch
            {
                OrderType.InTheInstitution => AppResources.InEnvy,
                OrderType.TakeAway => AppResources.TakeAway,
                OrderType.Delivery => AppResources.DeliveryLabel,
                _ => Detail?.Name
            };

        }

    }

    [Ignored]
    public string OrderTypeGlyph
    {
        get
        {
            if (Detail is null) return "";
            return Detail.OrderType switch
            {
                OrderType.InTheInstitution => "\uf2e7",
                OrderType.TakeAway => "\uf290",
                OrderType.Delivery => "\uf48b",
                _ => ""

            };

        }

    }

    [Ignored]
    public string OrderStatusText
    {
        get
        {
            if (Detail?.OrderType != OrderType.Delivery || OrderDelivery is null) return AppResources.TotalToPay;
            return OrderDelivery?.DeliveryStatus switch
            {
                DeliveryStatus.New => AppResources.Prepared,
                DeliveryStatus.Success => AppResources.OnWayLabel,
                DeliveryStatus.OnWay => AppResources.DeliveredLabel,
                DeliveryStatus.Delivered => AppResources.ClosedLabel,
                _ => AppResources.TotalToPay


            };




        }
    }
    [Ignored]
    public bool PickerVisible { get => OrderDelivery?.DeliveryStatus == DeliveryStatus.Success && Detail?.OrderType == OrderType.Delivery; }

    [MapTo("order_state")]
    public IList<OrderStatuses> OrderStates { get; }









    [Ignored]
    public decimal OrderDiscountTotal
    {

        get
        {
            var sumOrderSaleWhole = OrderSales.Sum(x => x.DiscountSum);
            return sumOrderSaleWhole;
        }
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(OrderDelivery.DeliveryStatus) || propertyName == nameof(OrderDelivery))
        {

            RaisePropertyChanged(nameof(OrderStatusText));
            RaisePropertyChanged(nameof(PickerVisible));
        }

        if (propertyName == nameof(OrderSales))
        {
            RaisePropertyChanged(nameof(OrderDiscountTotal));

        }



    }
    //[Ignored]
    //public Color OrderColorState => OrderStatus switch { OrderStatus.New => Color.FromHex("F7B548"), OrderStatus.Success => Color.FromHex("399EE8"), OrderStatus.Bill => Color.FromHex("#f2c51f"), _ => Color.FromHex("#726E76") };


}



[MapTo("OrderSources")]
public class Detail : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("sales_points_id")]
    public IList<SalesPoint> SalesPointsId { get; }

    [MapTo("name")]
    public string Name { get; set; } = string.Empty;

    [MapTo("on_terminal")]
    public bool OnTerminal { get; set; } = false;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; } = false;
    [MapTo("order_type")]
    public int OrderTypeRaw { get; set; }
    [Ignored]
    public OrderType OrderType
    {
        get { return (OrderType)OrderTypeRaw; }
        set { OrderTypeRaw = (int)value; }
    }

    [Ignored]
    public bool OrderTypePayVisible { get => OrderType != OrderType.Delivery; }
    [Ignored]
    public bool OrderTypeChangeStateVisible { get => !OrderTypePayVisible; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; } = DateTimeOffset.Now;

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(OrderTypeRaw))
        {

            RaisePropertyChanged(nameof(OrderTypePayVisible));
            RaisePropertyChanged(nameof(OrderTypeChangeStateVisible));
        }



    }
}


public class OrderStatuses : EmbeddedObject
{


    [MapTo("duration")]
    public long Duration { get; set; }

    [MapTo("order_state")]

    public int OrderStateRaw { get; set; }

    [Ignored]
    public OrderStatus OrderStateEnum
    {
        get => (OrderStatus)OrderStateRaw; set
        {

            OrderStateRaw = (int)value;


        }
    }



}
public class OrderDeliveryStatuses : EmbeddedObject
{


    [MapTo("duration")]
    public long Duration { get; set; }

    [MapTo("order_state")]

    public int OrderStateRaw { get; set; }

    [Ignored]
    public DeliveryStatus OrderStateEnum
    {
        get => (DeliveryStatus)OrderStateRaw; set
        {

            OrderStateRaw = (int)value;


        }
    }


}


//public class OrderState<Value> : EmbeddedObject where Value : Enum
//{


//    [MapTo("duration")]
//    public long Duration { get; set; }

//    [MapTo("order_state")]

//    public int OrderStateRaw { get; set; }

//    [Ignored]
//    public Value OrderStateEnum { get => (Value)(object)OrderStateRaw; set {

//            OrderStateRaw = (int)(object)value;

//            var i = 0;
//        } }



//}


public class Logs : EmbeddedObject
{
    [MapTo("creater_date")]
    public DateTimeOffset CreaterDate { get; set; } = DateTimeOffset.Now;

    [MapTo("device_name")]
    public string DeviceName { get; set; }

    [MapTo("title")]
    public string Title { get; set; }
}




public class OrderSale : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    //["premise_id"]
    //public Premise Premise { get; set; }
    [MapTo("name")]
    public string Name { get; set; } = string.Empty;

    //TODO заполнить состав позиции модификаторы и т.п. 
    [MapTo("position_detail")]
    public string PositionDetail { get; set; } = string.Empty;

    [MapTo("code")]
    public string Code { get; set; } = string.Empty;

    [MapTo("price")]
    public decimal Price { get; set; }

    [MapTo("amount")]
    public double Amount { get; set; }

    [MapTo("vat")]
    public double Vat { get; set; }

    [MapTo("section")]
    public string Section { get; set; } = string.Empty;

    [MapTo("discount_sum")]
    public decimal DiscountSum { get; set; }

    [MapTo("increase_sum")]
    public decimal IncreaseSum { get; set; }

    [MapTo("discount")]
    public double DiscountPercent { get; set; }

    [MapTo("increase")]
    public double Increase { get; set; }

    [MapTo("promotion_id")]
    public Promotion Promotion { get; set; }


    [MapTo("is_main_promotion_item")]
    public bool IsMainPromotionItem { get; set; } = true;


    [MapTo("sum")]
    public decimal Sum { get; set; }

    [MapTo("is_taxable")]
    public bool IsTaxable { get; set; }

    [MapTo("tax_id")]
    public Taxes Tax { get; set; }

    [MapTo("tax_percent")]
    public double TaxPercent { get; set; }

    [MapTo("transfer_type")]

    public int TransferRaw { get; set; }

    [MapTo("cost")]
    public decimal Cost { get; set; }

    [MapTo("category_id")]
    public Category Category { get; set; }

    [MapTo("contractor")]
    public string Contractor { get; set; } = string.Empty;

    [MapTo("gtin_code")]
    public string BarCode { get; set; } = string.Empty;

    [MapTo("product_id")]
    public Product Product { get; set; }

    [Ignored]
    public string ProductUnit { get => Product?.Unit.ToString(); set { } }
    [Ignored]
    public string ProductId { get => Product?.Id.ToString(); set { } }

    [MapTo("psu")]
    public string Psu { get; set; } = string.Empty;

    [MapTo("comment")]
    public string Comment { get; set; }

    [MapTo("GuestName")]
    public string GuestName { get; set; } = string.Empty;

    /// <summary>
    /// Добавилось в версии 1.0.15. 
    /// Нужен для сортировки заказов. 
    /// Берет данные из поля GuestName для обратной совместимости с другими более ранними версиями.
    /// </summary>
    [MapTo("guest_index")]
    public int GuestIndex { get; set; }

    [Ignored]
    public TransferType TransferType
    {
        get { return (TransferType)TransferRaw; }
        set { TransferRaw = (int)value; }
    }



    public OrderSale DeepCopy()
    {
        using (MemoryStream stream = new MemoryStream())
        {
#pragma warning disable SYSLIB0011 // Тип или член устарел
            IFormatter formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Тип или член устарел
            formatter.Serialize(stream, this);
            stream.Seek(0, SeekOrigin.Begin);
            return (OrderSale)formatter.Deserialize(stream);
        }
    }
    /*  [Ignored]
      public decimal TotalPrice { get => Price * (decimal)Amount - (Price * (decimal)Amount * (decimal)DiscountPercent / 100) - DiscountSum; }
      */


    /// <summary>
    /// По другому считается скидка, DiscountPercent указываются проценты на скидку, сумма скидки должна всегда проставляется в DiscountSum
    /// Учитывать при общей скидке на заказ и полной стоимости товара только DiscountSum
    /// Отчеты считаются так же с учетом только DiscountSum
    /// </summary>
    [Ignored]
    public decimal TotalPrice { get => Price * (decimal)Amount - DiscountSum; }

    [Ignored]
    public bool IsHavePromotion { get => Promotion != null; }
    [Ignored]
    private bool isActive { get; set; }
    [Ignored]
    public bool IsActive { get => isActive; set { isActive = value; OnPropertyChanged(nameof(isActive)); } }


    [Ignored]

    public bool IsHaveProduct { get => !string.IsNullOrWhiteSpace(Name); }


    [MapTo("selected_modifier_goods_id")]
    public IList<SelectedModifiers> ModifierGoods { get; }

    protected override async void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(countOrder))
        {
            RaisePropertyChanged(nameof(CountOrder));
        }
        if (propertyName == "count")
        {
            RaisePropertyChanged(nameof(Count));
        }

        if (propertyName == nameof(isActive))
        {
            RaisePropertyChanged(nameof(IsActive));
        }

        //if (propertyName == nameof(GuestName))
        //    RaisePropertyChanged(nameof(GuestSort));

        if (propertyName == nameof(Name))
            RaisePropertyChanged(nameof(IsHaveProduct));

        if (propertyName == nameof(Price) || propertyName == nameof(Amount) || propertyName == nameof(DiscountPercent))
        {
            try
            {
                await ServiceHelper.GetService<OrderDetailPageViewModel>().SaveOrderDiscountSum(this);
                RaisePropertyChanged(nameof(TotalPrice));
                await ServiceHelper.GetService<OrderDetailPageViewModel>().SaveOrderTotalSumm();
            }
            catch { }

        }



        if (propertyName == nameof(Promotion))
            RaisePropertyChanged(nameof(IsHavePromotion));




    }


    [Ignored]
    private int countOrder { get; set; } = 1;

    [Ignored]
    private int count { get; set; } = 1;

    [Ignored]
    public int CountOrder { get => countOrder; set { countOrder = value; OnPropertyChanged(nameof(countOrder)); } }

    [Ignored]
    public int Count { get => count; set { count = value; OnPropertyChanged(nameof(count)); } }





}




public class OrderDelivery : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("cashier_id")]
    public string CashierId { get; set; } = string.Empty;

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("detail_address")]
    public DeliveryAddress Address { get; set; } = new();


    [MapTo("delivery_zone_id")]
    public DeliveryZone DeliveryZone { get; set; }
    [MapTo("courier_id")]
    public Employer Courier { get; set; }



    [MapTo("delivery_order_state")]
    public IList<OrderDeliveryStatuses> DeliveryOrderState { get; }

    [Ignored]
    public DeliveryStatus DeliveryStatus
    {

        get { return DeliveryOrderState.LastOrDefault()?.OrderStateEnum ?? DeliveryStatus.New; }
        set { DeliveryOrderState.Add(new() { OrderStateEnum = value }); }
    }



    [Ignored]
    public int DeliveryStatusRaw
    {

        get { return (int)DeliveryStatus; }


    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(DeliveryOrderState))
        {
            RaisePropertyChanged(nameof(DeliveryStatusRaw));
            RaisePropertyChanged(nameof(DeliveryStatus));

        }

    }

}




public class DeliveryAddress : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("country")]
    public string Country { get; set; } = string.Empty;

    [MapTo("city")]
    public string City { get; set; } = string.Empty;

    [MapTo("address")]
    public string Address { get; set; } = string.Empty;

    [MapTo("address_additional")]
    public string AddressAdditional { get; set; } = string.Empty;

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;






}



public class OrderReceipt : EmbeddedObject
{

    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("sum")]
    public decimal Sum { get; set; }

    [MapTo("is_write_off")]
    public bool IsWriteOff { get; set; }

    [MapTo("submited")]
    public bool Submited { get; set; }

    [MapTo("handing_over")]
    public string HandingOver { get; set; } = string.Empty;

    [MapTo("cashier_id")]
    public string CashierId { get; set; } = string.Empty;

    [MapTo("is_refund")]
    public bool IsRefund { get; set; }

    [MapTo("is_cancellation")]
    public bool IsCancellation { get; set; }

    [MapTo("discount_sum")]
    public decimal DiscountSum { get; set; }

    [MapTo("increase_sum")]
    public decimal IncreaseSum { get; set; }

    [MapTo("discount_percent")]
    public double DiscountPercent { get; set; }

    [MapTo("increase_percent")]
    public double IncreasePercent { get; set; }

    [MapTo("result_sum")]
    public decimal ResultSum { get; set; }

    [MapTo("table")]
    public Table Table { get; set; }

    [MapTo("order_receipt_payment")]
    public IList<OrderReceiptPayment> OrderReceiptPayments { get; }

    [MapTo("description")]
    public string Description { get; set; } = string.Empty;

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; } = DateTimeOffset.Now;

    [MapTo("close_date")]
    public DateTimeOffset CloseDate { get; set; } = DateTimeOffset.Now;

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}



public class OrderReceiptPayment : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("payment_method_id")]
    public PaymentMethod PaymentMethod { get; set; }

    [Ignored]
    public string PaymentMethodType { get => PaymentMethod.PaymentType.ToString(); set => PaymentMethodType = value; }

    [MapTo("sum")]
    public decimal Sum { get; set; }
}



[Ignored]
public class GuestGroup : ObservableRangeCollection<OrderSale>, INotifyPropertyChanged
{

    public int GroupIndex { get; set; }

    public string FooterTitle { get; set; }



    private string _groupIcon = FontAwesomeIcons.ArrowDown;
    public string GroupIcon
    {
        get => _groupIcon;
        set => SetProperty(ref _groupIcon, value);
    }

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }


    public GuestGroup(int groupIndex, List<OrderSale> orders) : base(orders)
    {
        

        GroupIndex = groupIndex;
        FooterTitle = "";
        IsSelected = false;

    }

    protected bool SetProperty<T>(ref T backingStore, T value,
[CallerMemberName] string propertyName = "",
Action onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    #region INotifyPropertyChanged
#pragma warning disable CS0114 // Член скрывает унаследованный член: отсутствует ключевое слово переопределения
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0114 // Член скрывает унаследованный член: отсутствует ключевое слово переопределения
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        var changed = PropertyChanged;
        if (changed == null)
            return;

        changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion



}
