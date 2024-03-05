using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    [MapTo("ReasonsForWriteOffs")]
    public class ReasonsForWriteOff : RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        [MapTo("name")]
        public string Name { get; set; } = string.Empty;

        [MapTo("description")]
        public string Description { get; set; } = string.Empty;

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; } = DateTime.Now;
        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; } = DateTime.Now;
        [MapTo("user_id")]
        public string UserID { get; set; }

        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}
