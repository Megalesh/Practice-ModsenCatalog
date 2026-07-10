namespace ModsenCatalog.BusinessLogic.Events
{
    public class ReviewAddedEvent : DomainEvent
    {
        public string ProductName { get; set; } = string.Empty;
    }
}
