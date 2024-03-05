namespace Guester.Models;


public class Taxes : RealmObject
{
    [MapTo("_id"), PrimaryKey]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [MapTo("brand_id")]
    public string BrandId { get; set; } = string.Empty;

    [MapTo("name")]
    public string Name { get; set; }

    [MapTo("percent")]
    public double Percent { get; set; } = 0;

    [MapTo("excise_percent")]
    public double ExcisePercent { get; set; } = 0;

    [MapTo("tax_type")]
    public int TaxTypeRaw { get; set; }

    [Ignored]
    public TaxType TaxType { get => (TaxType)TaxTypeRaw; set => TaxTypeRaw = (int)value; }

    [MapTo("is_fiscal")]
    public bool IsFiscal { get; set; } = false;


    [MapTo("is_deleted")]
    public bool IsDeleted { get; set; }
}
