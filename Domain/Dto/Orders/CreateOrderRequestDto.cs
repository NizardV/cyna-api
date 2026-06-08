namespace Domain.Dto.Orders;

public class CreateOrderRequestDto
{
    public IEnumerable<CreateOrderItemDto> Items { get; set; } = [];
    public CreateOrderAddressDto Address { get; set; } = null!;
    public decimal Total { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
}

public class CreateOrderItemDto
{
    public int PricingPlanId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public int QuantityUsers { get; set; }
    public int QuantityDevices { get; set; }
}

public class CreateOrderAddressDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}
