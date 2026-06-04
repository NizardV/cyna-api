namespace Infrastructure.Entities.PromoAndCms;

using OrdersAndSubscriptions;

public class OrderPromoCode
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PromoCodeId { get; set; }
    public decimal AppliedDiscountAmount { get; set; }

    public Order Order { get; set; } = null!;
    public PromoCode PromoCode { get; set; } = null!;
}