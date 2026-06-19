namespace Domain.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations.Schema;

using Domain.Entities.OrdersAndSubscriptions;

using Infrastructure.Entities.PromoAndCms;

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