namespace Domain.Dto.Orders;

public class CreateOrderResultDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<CreateOrderResultItemDto> Items { get; set; } = [];
}

public class CreateOrderResultItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public int QuantityUsers { get; set; }
    public int QuantityDevices { get; set; }
    public decimal UnitPriceUsers { get; set; }
    public decimal UnitPriceDevices { get; set; }
    public decimal LineTotal { get; set; }
}
