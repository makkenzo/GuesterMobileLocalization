using Guester.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{

    [MapTo("RemainsOfWarehouses")]
    public class RemainsOfWarehouse :RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        //[BsonElement("remains_snapshot_date")]
        //public DateTimeOffset RemainsSnapshotDate { get; set; }

        [MapTo("remains_products")]
        public IList<RemainsProduct> RemainsProducts { get; }

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; } = DateTime.Now;
        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; } = DateTime.Now;
        [MapTo("user_id")]
        public string UserID { get; set; }
    }

    public class RemainsProduct : EmbeddedObject
    {

        [MapTo("product_id")]
        public Product Product { get; set; }
        [MapTo("unit")]
        public int UnitRaw { get; set; }
        [Ignored]
        public UnitType Unit { get => (UnitType)UnitRaw; set => UnitRaw = (int)value; } 

        [MapTo("amount")]
        public double Amount { get; set; } = 0;

        [MapTo("cost")]
        public string Cost { get; set; } = string.Empty;

        [MapTo("total_cost_sum")]
        public string TotalCostSum { get; set; } = string.Empty;

        [MapTo("limit")]
        public double Limit { get; set; } = 0;

        [MapTo("entrance_operations")]
        public object EntranceOperations { get; }//ingored

        [MapTo("remains")]
        public IList<Remains> Remains { get; }

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; } = DateTime.Now;
        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; } = DateTime.Now;
        [MapTo("user_id")]
        public string UserID { get; set; }


        [MapTo("_id"),]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


        [Ignored]
        public string DisplayName { get => Product?.Category?.Name??""; }
    }

    public class Remains : EmbeddedObject
    {
        [MapTo("warehouse_id")]
        public WareHouse Warehouse { get; set; }

        [MapTo("sales_point_id")]
        public SalesPoint SalesPoint { get; set; } 

        [MapTo("amount")]
        public double Amount { get; set; } = 0;

        [MapTo("cost")]
        public string Cost { get; set; } = string.Empty;

        [MapTo("total_cost_sum")]
        public string TotalCostSum { get; set; } = string.Empty;

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; } = DateTime.Now;
        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; } = DateTime.Now;
        [MapTo("user_id")]
        public string UserID { get; set; }
        [MapTo("_id"),]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    }
}
