namespace Infrastructure.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<User> Users { get; set; } = [];
}
