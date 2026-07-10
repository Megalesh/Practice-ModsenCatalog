namespace ModsenCatalog.BusinessLogic.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    public User? User { get; set; }
    public Product? Product { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}