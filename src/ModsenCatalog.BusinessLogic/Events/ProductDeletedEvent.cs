namespace ModsenCatalog.BusinessLogic.Events
{
    public class ProductDeletedEvent : DomainEvent
    {
        public string ProductName { get; set; } = string.Empty;
    }
}
