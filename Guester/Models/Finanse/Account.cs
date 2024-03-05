using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    [MapTo("Accounts")]
    public class Account : RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

    
    

        [MapTo("name")]
        public string Name { get; set; }

    
        [MapTo("currency_id")]
        public string CurrencyId { get; set; } = string.Empty;

        [MapTo("is_cash")]
        public bool IsCash { get; set; } = false;


        [MapTo("balance")]
        public string Balance { get; set; } = string.Empty;

        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; } = false;
     
        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; }

        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; }

        [MapTo("user_id")]
        public string UserId { get; set; } = string.Empty;


    }
}
