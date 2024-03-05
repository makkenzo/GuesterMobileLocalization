namespace Guester.Models;

#region Access
public enum CashRegisterType
{
    // Arca,
    // Other
    CashRegister,
    Printer,
    Scanner,
    TSD
}
#endregion

#region Business
public enum StockReporting
{
    DoNotNotify,
    NotifyATheTimOfSale,
    AtEndShift
}

public enum Currency
{
    KZT,
    PUB,
    USD,
    SOM
}
public enum Language
{
    RU,
    KZ,
    US,
    UZ
}

#endregion

#region Finanse

public enum OperationType
{
    WriteOffs,
    Entrance,
    Moving
}

public enum TransactionType
{
    Income,
    Expenditure,
    Translation,
    OpeningOfCashShift,
    ClosingOfCashShift,
    Collection
}
#endregion

#region Marketing

public enum Gender
{
    Male,
    Female,
    Other
}
public enum ResultType
{
    BonusGoods,
    FixedDiscountAmount,
    DiscountPercent,
    FixedPrice
}

public enum ParticipientType
{
    All,
    NotКegistered,
    Кegistered,
    ClientGroup
}

public enum ConditionType
{
    DiscountPercentOnBonusGoods,
    FixedDiscountAmountOnBonusGoods,
    FixedPriceOnBonusGoods
}
#endregion

#region Menu
public enum PostEnum
{
    NoAccess,
    View,
    FullAccess
}
public enum TypeCode : int
{
    Ingredient,
    Product,
    ProductCard,
    SemiFinished
}
public enum CategoryType
{
    ProductAndProductCard,
    IngredientAndSemiFinished
}
#endregion

#region Settings

public enum PaymentType
{
    Cash,
    Cashless,
    Card,
    Bonus

}
public enum TaxType : int
{
    FromTurnover,
    AddedValue
}
#endregion

#region ShowCase
public enum OrderStatus
{
    New,
   // Precheck,
    Closed,
    Deleted,
    Success,
    Delivery


}

public enum OrderType
{
    Delivery,
    InTheInstitution,
    TakeAway,
    CustomUser
}

public enum OrderFilter
{
    New,//новый
    Online,//онлайн
    Success,//готов
    InWay,//в пути
    Delivered,//доставлен
    All
}
public enum DeliveryStatus
{
    New,
    Unconfirmed,
    Waiting,
    OnWay,
    Delivered,
    Closed,
    Cancelled,
    Success
}
public enum TransferType
{
    Cash,
    Cashless,
    Card,
    Bonus
}
#endregion

#region WareHouse

public enum UnitType
{
    pcs,
    milliliters,
    grams
}
public enum TypeOfInventory
{
    Full,
    Partial
}

#endregion


public enum CheckFilter
{
    Close,
    Cash,

    Card,
    Refund,
}


public enum PageEnum
{
    Statistics,
    Finance,
    Menu,
    Stock,
    Marketing,
    Access,
    Settings
}
