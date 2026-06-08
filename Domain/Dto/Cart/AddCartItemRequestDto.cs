namespace Domain.Dto.Cart;

public class AddCartItemRequestDto
{
    public int PricingPlanId { get; set; }
    public int QuantityUsers { get; set; }
    public int QuantityDevices { get; set; }
}
