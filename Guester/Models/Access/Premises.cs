using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    public class Premises : RealmObject
    {
        [MapTo("_id"),PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        [MapTo("sales_point_id")]
        public string SalesPointId { get; set; } = string.Empty;

        [MapTo("name")]
        public string Name { get; set; }

        [MapTo("address")]
        public string Address { get; set; }

        [MapTo("is_workshop")]
        public bool IsWorkShop { get; set; }
        [MapTo("warehouses")]
        public IList<WareHouse> WareHouse { get; }
        [MapTo("tables")]
        public IList<Table> Tables { get; }

        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; }
    }
    public class Table : EmbeddedObject
    {
        [MapTo("_id")]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("name")]
        public string Name { get; set; } = string.Empty;
        [MapTo("seats")]
        public int Seats { get; set; } = 1;
     
        [MapTo("width")]
        public int Width { get; set; } = 0;
        [MapTo("height")]
        public int Height { get; set; } = 0;
        [MapTo("color")]
        public string Color { get; set; } = "Green";
        [MapTo("border_radius")]
        public float BorderRadius { get; set; } = 0;
        [MapTo("pos_x")]
        public int PosX { get; set; } = 0;
        [MapTo("pos_y")]
        public int PosY { get; set; } = 0;
        [MapTo("zindex")]
        public int Zindex { get; set; } = 0;

        [MapTo("picture")]
        public string Picture { get; set; } = string.Empty;

        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; }
    }

}
