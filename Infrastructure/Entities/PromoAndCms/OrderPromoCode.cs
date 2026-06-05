namespace Infrastructure.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations.Schema;
using OrdersAndSubscriptions;

public class OrderPromoCode
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PromoCodeId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal AppliedDiscountAmount { get; set; }

    public Order Order { get; set; } = null!;
    public PromoCode PromoCode { get; set; } = null!;
}