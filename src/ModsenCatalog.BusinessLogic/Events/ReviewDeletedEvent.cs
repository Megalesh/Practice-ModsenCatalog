namespace ModsenCatalog.BusinessLogic.Events
{
    public class ReviewDeletedEvent : DomainEvent
    {
        public string ProductName { get; set; } = string.Empty;
    }
}
