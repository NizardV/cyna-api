namespace Domain.Entities.AddressAndPayment;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities;

public class PaymentMethod
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(255)]
    public string StripePaymentMethodId { get; set; } = string.Empty;

    public CardBrand? CardBrand { get; set; }

    [Required, MaxLength(4)]
    public string CardLast4 { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;

    public User User { get; set; } = null!;
}