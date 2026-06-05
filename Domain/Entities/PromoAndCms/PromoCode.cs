namespace Domain.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations;

public class PromoCode
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public int DiscountPercent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrderPromoCode> Orders { get; set; } = [];
}