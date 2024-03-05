using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    [MapTo("Shifts")]
    public class Shift : RealmObject
    {
        
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

       
        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;


      
        [MapTo("sales_point_id")]
        public string SalePointId { get; set; } = string.Empty; 

   
       

       
        [MapTo("start_date_time")]
        public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.Now;

    
        [MapTo("end_date_time")]
        public DateTimeOffset EndDateTime { get; set; } = DateTimeOffset.Now;


        [MapTo("durations")]
        public long Durations { get; set; } = 0;

 
        [MapTo("is_closed")]
        public bool IsClosed { get; set; } = false;


        [MapTo("personal_shift")]
        public IList<PersonalShift> PersonalShifts { get; }



        [MapTo("linked_shifts_id")]
        public IList<Shift> LinkedShiftsId { get; }


        [MapTo("shift_payments")]
        public IList<ShiftPayment> ShiftPayments { get;  }

    }


    public class PersonalShift : EmbeddedObject
    {

     
        
        [MapTo("employee_id")]
      
        public Employer Employer { get; set; } 

        [MapTo("transactions_id")]
       
        public IList<Transaction> Transactions { get; }


        [Ignored]
        public string LastOpenShiftDate { get => Transactions.Where(x => x.TransactionType == TransactionType.OpeningOfCashShift).LastOrDefault()?.CreationDate.ToString("dd MMM yyyy hh:mm"); }

        [MapTo("_id")]

        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [MapTo("start_date_time")]
       
        public DateTimeOffset StartDateTime { get; set; } = DateTimeOffset.Now;

        [MapTo("end_date_time")]
   
        public DateTimeOffset EndDateTime { get; set; } = DateTimeOffset.Now;

        [MapTo("durations")]
     
        public long Durations { get; set; } = 0;

        [MapTo("is_closed")]
       
        public bool IsClosed { get; set; } = false;
    }



    public class ShiftPayment : EmbeddedObject
    {
        [MapTo("_id")]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();


        [MapTo("payment_method_id")]
        public string PaymentMethodId { get; set; } = string.Empty;
        [MapTo("sum")]
        public  string Sum { get; set; }=string.Empty; //string
    }



}
