using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    [MapTo("Suppliers")]
    public class Supplier : RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        [MapTo("name")]
        public string Name { get; set; } = string.Empty;

        [MapTo("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MapTo("address")]
        public string Address { get; set; } = string.Empty;

        [MapTo("description")]
        public string Description { get; set; } = string.Empty;

        [MapTo("taxpayer_number")]
        public string TaxpayerNumber { get; set; } = string.Empty;

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
