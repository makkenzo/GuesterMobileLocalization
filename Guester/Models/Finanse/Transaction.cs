using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Guester.Models
{
    [MapTo("Transactions")]
    public class Transaction : RealmObject
    {
     
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        //[MapTo("operation_type")]
        //public int OperationTypeRaw { get; set; }
        //[Ignored]
        //public OperationType OperationType { get => (OperationType)OperationTypeRaw; set => OperationTypeRaw=(int)value; }

        [MapTo("account_to_id")]
        public Account AccountToId { get; set; } 


        [MapTo("transaction_type")]
        public int TransactionTypeRaw { get; set; }

        [Ignored] 
        public TransactionType TransactionType { get => (TransactionType)TransactionTypeRaw; set => TransactionTypeRaw = (int)value; }


        [Ignored]
        public string TransactionTypeToVisual { get => TransactionType switch 
        { 
            TransactionType.OpeningOfCashShift=>"Открытие кассовой смены", 
            TransactionType.Income=>"Приход", 
            TransactionType.ClosingOfCashShift=> "Закрытие кассовой смены",
            TransactionType.Expenditure=>"Расход",
            TransactionType.Translation=> "Транспортировка денег",
            TransactionType.Collection=> "Инкассация",
            _ =>""
        }; 
        }


        [MapTo("employee_id")]
        public Employer Employer { get; set; } 

     
        [MapTo("total_sum")]
        public decimal TotalSum { get; set; } = 0;

     
        [MapTo("account_id")]
        public Account Account { get; set; }

      
        [MapTo("transaction_category_id")]
        public string TransactionCategoryId { get; set; } = string.Empty;

        [MapTo("description")]
        public string Description { get; set; }

        [MapTo("is_z_report")]
        public bool IsZReport { get; set; } = false;

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; }=DateTimeOffset.Now;

      

        [Ignored]
        public string CreationDateToVisual { get => CreationDate.ToString("dd MMM yyyy hh:mm"); }


        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; } = DateTimeOffset.Now;

        [MapTo("user_id")]
        public string UserId { get; set; } = string.Empty;



        [MapTo("account_balance")]
        public string AccountBalance { get; set; } = "0";

        [MapTo("account_to_balance")]
        public string AccountToBalance { get; set; } = "0";



        [MapTo("shift_id")]
        public string ShiftId { get; set; } = string.Empty; 

    }
}
