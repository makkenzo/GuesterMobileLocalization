

using MongoDB.Bson.Serialization.Attributes;

namespace Guester.Models
{
    [MapTo("Brands")]
    public class Brand : RealmObject
    {
        [MapTo("_id"), PrimaryKey]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
       
        [MapTo("buisiness_id")]
        public string BuisinessId { get; set; } = string.Empty;


        [MapTo("fiscal_print_on")]
        public bool FiscalPrintOn { get; set; } = false;

        [MapTo("name")]
        public string Name { get; set; } = string.Empty;

        [MapTo("url")]
        public string Url { get; set; } = string.Empty;

        [MapTo("logo")]
        public string Logo { get; set; } = string.Empty;

        [MapTo("time_zone")]
        public int TimeZone { get; set; } = 0;

        [MapTo("currency")]
        public int CurrencyRaw { get; set; }

        [Ignored]
        public Currency Currency { get => (Currency)CurrencyRaw; set => CurrencyRaw = (int)value; }

        [MapTo("language")]
        public int LanguageRaw { get; set; }

        [Ignored]
        public Language Language { get => (Language)LanguageRaw; set => LanguageRaw = (int)value; }

        [MapTo("in_store")]
        public bool InStore { get; set; } = false;

        [MapTo("in_delivery")]
        public bool InDelivery { get; set; } = false;

        [MapTo("is_store_map")]
        public bool IsStoreMap { get; set; } = false;

        [MapTo("is_guests_count")]
        public bool IsGuestsCount { get; set; } = false;

        [MapTo("is_waiter_change")]
        public bool IsWaiterChange { get; set; } = false;

        [MapTo("is_round")]
        public bool IsRound { get; set; } = false;

        [MapTo("is_percentage_for_service")]
        public bool IsPercentageForService { get; set; } = false;

        [MapTo("percentage_for_service")]
        public int PercentageForService { get; set; } = 0;

 
        [MapTo("tax_id")]
        public string TaxId { get; set; } = string.Empty;

        [MapTo("is_use_production")]
        public bool IsUseProduction { get; set; } = false;

        [MapTo("is_shifts")]
        public bool IsShifts { get; set; } = false;

        [MapTo("is_tax")]
        public bool IsTax { get; set; } = false;

        [MapTo("is_fiscal")]
        public bool IsFiscal { get; set; } = false;

        [MapTo("is_default_print_fiscal")]
        public bool IsDefaultPrintFiscal { get; set; } = false;

        [MapTo("is_work_time")]
        public bool IsWorkTime { get; set; } = false;

        [MapTo("stock_reporting")]
        public int StockReportingRaw { get; set; }

        [Ignored]
        public StockReporting StockReporting { get => (StockReporting)StockReportingRaw; set => StockReportingRaw = (int)value; }

        [MapTo("notification_mail")]
        public string NotificationMail { get; set; } = string.Empty;

        [MapTo("online_orders_mail")]
        public string OnlineOrdersMail { get; set; } = string.Empty;

        [MapTo("is_delivery")]
        public bool IsDelivery { get; set; } = false;

  
        [MapTo("delivery_tax_id")]
        public string DeliveryTaxId { get; set; } = string.Empty;

        [MapTo("is_complete")]
        public bool IsComplete { get; set; } = false;

        [MapTo("is_delivered")]
        public bool IsDelivered { get; set; } = false;



        [MapTo("is_bill")]
        public bool IsBill { get; set; } = false;

        [MapTo("is_double_receipt")]
        public bool IsDoubleReceipt { get; set; } = false;

        [MapTo("is_enter_client_on_magnet_card")]
        public bool IsEnterClientOnMagnetCard { get; set; } = false;

        [MapTo("is_print_fiscal_dublicate")]
        public bool IsPrintFiscalDublicate { get; set; } = false;

        [MapTo("is_no_session_close_on_bill")]
        public bool IsNoSessionCloseOnBill { get; set; } = false;

        [MapTo("is_sms_confirmation_bonus")]
        public bool IsSMSConfirmationBonus { get; set; } = false;

        [MapTo("is_automatic_print_order")]
        public bool IsAutomaticPrintOrder { get; set; } = false;

        [MapTo("on_product_delete")]
        public bool OnProductDelete { get; set; } = false;

        [MapTo("on_add_discount")]
        public bool OnAddDiscount { get; set; } = false;

        [MapTo("on_cancel_action")]
        public bool OnCancelAction { get; set; } = false;

        [MapTo("on_sales_report")]
        public bool OnSalesReport { get; set; } = false;

        [MapTo("on_fiscal_transaction")]
        public bool OnFiscalTransaction { get; set; } = false;

        [MapTo("on_zx_report")]
        public bool OnZXReport { get; set; } = false;

        [MapTo("on_receipt_separation")]
        public bool OnReceiptSeparation { get; set; } = false;

        [MapTo("on_close_receipt")]
        public bool OnCloseReceipt { get; set; } = false;

        [MapTo("on_add_client")]
        public bool OnAddClient { get; set; } = false;

        [MapTo("on_receipt_archive")]
        public bool OnReceiptArchive { get; set; } = false;

        [MapTo("on_supply")]
        public bool OnSupply { get; set; } = false;

        [MapTo("is_print_receipt_on_close")]
        public bool IsPrintReceiptOnClose { get; set; } = false;

        [MapTo("is_print_receipt_number")]
        public bool IsPrintReceiptNumber { get; set; } = false;

        [MapTo("is_print_comment")]
        public bool IsPrintComment { get; set; } = false;

        [MapTo("is_print_client_info")]
        public bool IsPrintClientInfo { get; set; } = false;

        [MapTo("is_print_tax_total")]
        public bool IsPrintTaxTotal { get; set; } = false;

        [MapTo("is_print_prediction")]
        public bool IsPrintPrediction { get; set; } = false;

        [MapTo("is_print_on_wifi")]
        public bool IsPrintOnWifi { get; set; } = false;

        [MapTo("sid")]
        public string SID { get; set; } = string.Empty;

        [MapTo("password")]
        public string Password { get; set; } = string.Empty;// пароль админа

        [MapTo("is_print_address")]
        public bool IsPrintAddress { get; set; } = false;

        [MapTo("city")]
        public string City { get; set; } = string.Empty;

        [MapTo("zip")]
        public string Zip { get; set; } = string.Empty;

        [MapTo("address")]
        public string Address { get; set; } = string.Empty;

        [MapTo("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MapTo("receipt_language")]
        public int ReceiptLanguageRaw { get; set; }

        [Ignored]
        public Language ReceiptLanguage { get => (Language)ReceiptLanguageRaw; set => ReceiptLanguageRaw = (int)value; }

        [MapTo("on_receipt_header_print")]
        public bool OnReceiptHeaderPrint { get; set; } = false;

        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
