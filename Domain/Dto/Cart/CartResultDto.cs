namespace Domain.Dto.Cart;

public class CartResultDto
{
    public int CartId { get; set; }
    public string StripeClientSecret { get; set; } = string.Empty;
    public CartItemResultDto Item { get; set; } = null!;
    public CartSummaryDto CartSummary { get; set; } = null!;
}

public class CartItemResultDto
{
    public int Id { get; set; }
    public int PricingPlanId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public int QuantityUsers { get; set; }
    public int QuantityDevices { get; set; }
    public decimal UnitPriceUsers { get; set; }
    public decimal UnitPriceDevices { get; set; }
    public decimal LineTotal { get; set; }
    public int MaxUsersCheckout { get; set; }
    public int MaxDevicesCheckout { get; set; }
    public IEnumerable<CartTierDto> PricingTiers { get; set; } = [];
}

public class CartTierDto
{
    public string UnitType { get; set; } = string.Empty;
    public int MinQty { get; set; }
    public int MaxQty { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CartSummaryDto
{
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}
