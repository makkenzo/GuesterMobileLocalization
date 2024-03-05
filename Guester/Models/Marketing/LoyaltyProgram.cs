namespace Guester.Models;

[MapTo("LoyaltyPrograms")]
public class LoyaltyProgram : RealmObject
{
    [ PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; } = string.Empty;

    [MapTo("max_discount")]
    public double MaxDiscount { get; set; } = 0;

    [MapTo("welcome_bonus")]
    public decimal WelcomeBonus { get; set; } = 0;

    [MapTo("is_bonus_transition")]
    public bool IsBonusTransition { get; set; } = false;

    [MapTo("transition_condition")]
    public IList<TransitionCondition> TransitionConditions { get; }
    [MapTo("is_send_sms")]
    public bool IsSendSms { get; set; } = false;

    [MapTo("sms_description")]
    public string SmsDescription { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}

public class TransitionCondition : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }


    [MapTo("is_bonus")]
    public bool IsBonus { get; set; } = false;

    [MapTo("total_sum")]
    public decimal TotalSum { get; set; } = 0;


    [MapTo("client_group_id")]
    public ClientGroup ClientGroup { get; set; }
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
