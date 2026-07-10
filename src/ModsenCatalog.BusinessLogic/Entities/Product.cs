namespace ModsenCatalog.BusinessLogic.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public double AverageRating { get; set; } = 0.0;

    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}