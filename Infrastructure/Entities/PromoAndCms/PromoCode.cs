namespace Infrastructure.Entities.PromoAndCms;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrderPromoCode> Orders { get; set; } = [];
}