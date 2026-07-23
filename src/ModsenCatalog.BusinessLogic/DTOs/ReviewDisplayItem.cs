using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.Presentation.States;

public class ReviewDisplayItem
{
    public Review Review { get; set; }
    public string ProductName { get; set; }

    public ReviewDisplayItem(Review review, string productName)
    {
        Review = review;
        ProductName = productName;
    }
}