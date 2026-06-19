namespace Domain.Entities.AddressAndPayment;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities;

using OrdersAndSubscriptions;

public class Address
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public string? Region { get; set; }

    [Required, MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    public string Country { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public bool IsDefault { get; set; } = false;

    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}