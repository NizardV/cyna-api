namespace Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;

public class Company
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = [];
}