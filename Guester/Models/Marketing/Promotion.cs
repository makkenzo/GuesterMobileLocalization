namespace Guester.Models;

[MapTo("Promotions")]//акция
public class Promotion : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("start_date")]
    public DateTimeOffset StartDate { get; set; }

    [MapTo("end_date")]
    public DateTimeOffset EndDate { get; set; }

    [MapTo("is_all_sales_points")]
    public bool IsAllSalesPoints { get; set; } = true;

    [MapTo("sales_points_id")]
    public IList<SalesPoint> SalesPointsId { get; }

    [MapTo("is_accrue_bonuses")]
    public bool IsAccrueBonuses { get; set; } = false;

    [MapTo("is_automatic_add_action")]
    public bool IsAutomaticAddAction { get; set; } = false;

    [MapTo("is_any_conditions")]
    public bool IsAnyConditions { get; set; } = false;


    [MapTo("promotion_conditions")]
    public  IList<PromotionCondition> PromotionConditions { get;}

    [MapTo("tue")]
    public bool Tue { get; set; } = false;

    [MapTo("wed")]
    public bool Wed { get; set; } = false;

    [MapTo("thu")]
    public bool Thu { get; set; } = false;

    [MapTo("fri")]
    public bool Fri { get; set; } = false;

    [MapTo("sat")]
    public bool Sat { get; set; } = false;

    [MapTo("mon")]
    public bool Mon { get; set; } = false;

    [MapTo("sun")]
    public bool Sun { get; set; } = false;

    [MapTo("work_times")]
    public IList<WorkTime> WorkTimes { get; }

    [MapTo("participient_type")]

    public int ParticipientTypeRaw { get; set; }

    [Ignored]
    public ParticipientType ParticipientType { get => (ParticipientType)ParticipientTypeRaw; set => ParticipientTypeRaw = (int)value; }

    [MapTo("participants_id")]
    public IList<ClientGroup> Participants { get; }

    [MapTo("result_type")]
    public int ResultTypeRaw { get;set; }

    [Ignored]
    public ResultType ResultType { get => (ResultType)ResultTypeRaw; set => ResultTypeRaw=(int)value; }

    [MapTo("is_result_now")]
    public bool IsResultNow { get; set; } = false;

    [MapTo("result_value")]
    public decimal ResultValue { get; set; } = 0;

    [MapTo("result_bonus")]
    public ResultBonus ResultBonus { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class PromotionCondition : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("is_category")]
    public bool IsCategory { get; set; } = false;

    [MapTo("product_id")]
    public string ProductId { get; set; } = string.Empty;


    [MapTo("category_id")]
    public string CategoryId { get; set; } = string.Empty;

    [MapTo("is_equels")]// равен :true ? не менее
    public bool IsEquels { get; set; } = true;

    [MapTo("count")]//количество для попадение в акцию
    public double Count { get; set; } = 0;

    [MapTo("for_how_long")]//на сколько должен заказать
    public decimal ForHowLong { get; set; } = 0;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class WorkTime : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("start_time")]// начало работы акции
    public DateTimeOffset StartTime { get; set; }

    [MapTo("end_time")]// конец работы акции
    public DateTimeOffset EndTime { get; set; }

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class ResultBonus : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("bonus_product_amount")]//Количество бонусных товаров макс добавление
    public double BonusProductAmount { get; set; } = 0;

    [MapTo("condition_type")]
    public int ConditionTypeRaw { get; set; }

    [Ignored]
    public ConditionType ConditionType { get => (ConditionType)ConditionTypeRaw; set => ConditionTypeRaw = (int)value; }

    [MapTo("condition_value")]//либо процент либо сумма либо фактическая цена
    public double ConditionValue { get; set; } = 0;

    [MapTo("result_bonus_goods")]
    public IList<ResultBonusGoods> ResultBonusGoods { get; }

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class ResultBonusGoods : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("is_all")] // все продукты могут использоваться
    public bool IsAll { get; set; } = false;

    [MapTo("is_category")]
    public bool IsCategory { get; set; } = false;

  
    [MapTo("product_id")]
    public Product Product { get; set; }

    [MapTo("category_id")]
    public Category CategoryId { get; set; }

    [MapTo("modifiers_id")]
   public IList<Modifiers> Modifiers { get;  }
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class Participant : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("clients_group_id")]
    public ClientGroup ClientGroup { get; set; }
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

}
