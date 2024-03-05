
namespace Guester.Models;

[MapTo("SalesPoints")]
public class SalesPoint :RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; } = string.Empty;

    [MapTo("address")]
    public string Address { get; set; } = string.Empty;

    [MapTo("terminal_login")]
    public string Login { get; set; } = string.Empty;

    [MapTo("terminal_password")]
    public string Password { get; set; } = string.Empty;

    [MapTo("is_around_the_clock")]
    public bool IsAroundTheClock { get; set; }

    [MapTo("start_time")]
    public DateTimeOffset StartTime { get; set; }

    [MapTo("end_time")]
    public DateTimeOffset EndTime { get; set;}

    [MapTo("cashless_account_id")]
    public string CashLessAccountId { get; set; }= string.Empty;

    [MapTo("cash_account_id")]
    public Account CashAccountId { get; set; }

    [MapTo("collection_account_id")]
    public string CollectionAccountId { get; set; }=string.Empty;


    [MapTo("write_off_rules")]
    public IList<WriteOffRule>WriteOffRulesList { get;  }

    [MapTo("premises")]
    public IList<Premises> Premises { get; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get;set; }

    [MapTo("user_id")]
    public string UserId { get; set; }  =string.Empty;
    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

}

public class WriteOffRule : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("premises_id")]
    public Premises Premise { get; set; } 

    [MapTo("cash_register_id")]
    public string CashregisterId { get; set; } = string.Empty;

    [MapTo("warehouses")]
    public IList<WareHouse> WriteOffRuleWareHouses { get; }
}
public class Register : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; }
    [MapTo("cash_register_id")]
    public string CashRegisterId { get; set; } = string.Empty;
}


