using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models;

[MapTo("DeliveryZones")]
public class DeliveryZone : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;


    [MapTo("sales_point_id")]
    public string salesPointId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }


    [MapTo("price")]
    public decimal Price { get; set; } = 0m;

    [MapTo("service_standard_time")]
    public long ServiceStandardTime { get; set; }

    [MapTo("creation_date")]
    public DateTimeOffset CreationDate { get; set; }

    [MapTo("modify_date")]
    public DateTimeOffset ModifyDate { get; set; }

    [MapTo("user_id")]
    public string UserId { get; set; } = string.Empty;

    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
