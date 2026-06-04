namespace Infrastructure.Entities.AddressAndPayment;

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StripePaymentMethodId { get; set; } = string.Empty;
    public CardBrand? CardBrand { get; set; }
    public string CardLast4 { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    public User User { get; set; } = null!;
}