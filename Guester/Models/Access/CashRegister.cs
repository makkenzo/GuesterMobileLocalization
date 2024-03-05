
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace Guester.Models;

[MapTo("CashRegisters")]
public class CashRegister: RealmObject
{
    [PrimaryKey, MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("sales_point_id")]
    public string SalePointId { get; set; } = string.Empty;

    [MapTo("cash_register_type")]
    public int CashRegisterTypeRaw { get; set; }

    [Ignored]
    public CashRegisterType CashRegisterType
    {
        get { return (CashRegisterType)CashRegisterTypeRaw; }
        set { CashRegisterTypeRaw = (int)value; }
    }


    [Ignored]
    public string CashTypeToString
    {
        get
        {
            return CashRegisterType switch { CashRegisterType.Printer => "Принтер", CashRegisterType.CashRegister => "ККМ", CashRegisterType.Scanner => "Сканер", _ => "" };
        }
    }


    [MapTo("cash_register_setting")]
    public CashRegisterSetting CashRegisterSetting { get; set; } = new();

    [MapTo("is_active")]
    public bool IsActive { get; set; } // Todo need add to Model 

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}

public class CashRegisterSetting : EmbeddedObject
{
    [MapTo("_id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [MapTo("ip_address")]
    public string IpAddress { get; set; }
    [MapTo("login")]
    public string Login { get; set; }

    [MapTo("password")]
    public string Password { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifiedDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;
}


[Ignored]
public class ReceiptResponce
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("requestId")]
    public string RequestId { get; set; }
    [JsonProperty("status")]
    public string Status { get; set; }
    [JsonProperty("error")]
    public string Error { get; set; }
    [JsonProperty("command")]
    public string Command { get; set; }
    [JsonProperty("payload")]
    public dynamic Payload { get; set; }
}