namespace ModsenCatalog.BusinessLogic.Events
{
    public class CategoryDeletedEvent : DomainEvent
    {
        public string CategoryName { get; set; } = string.Empty;
    }
}
