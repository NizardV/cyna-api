namespace Infrastructure.Entities.AddressAndPayment;

using Domain.Entities;

using Infrastructure.Entities.OrdersAndSubscriptions;

using User = Entities.User;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty; // ISO 2 (FR, US…)
    public string? Phone { get; set; }
    public bool IsDefault { get; set; } = false;

    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}