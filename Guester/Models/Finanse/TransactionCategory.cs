
namespace Guester.Models
{
    [MapTo("TransactionsCategories")]
    public class TransactionCategory : RealmObject
    {
        [MapTo("_id"),PrimaryKey]
        public string Id { get; set; }=ObjectId.GenerateNewId().ToString();

        [MapTo("brand_id")]
        public string BrandId { get; set; } = string.Empty;

        [MapTo("name")]
        public string Name { get; set; }

        [MapTo("parent_id")]
        public string ParentId { get; set; } = string.Empty;

        [MapTo("is_income")]
        public bool IsIncome { get; set; } = false;

        [MapTo("is_exprense")]
        public bool IsExprense { get; set; } = false;

        [MapTo("is_show_on_terminal")]
        public bool IsShowOnTerminal { get; set; } = false;

        [MapTo("creation_date")]
        public DateTimeOffset CreationDate { get; set; }

        [MapTo("modify_date")]
        public DateTimeOffset ModifyDate { get; set; }

        [MapTo("user_id")]
        public string UserId { get; set; } = string.Empty;
        [MapTo("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}
