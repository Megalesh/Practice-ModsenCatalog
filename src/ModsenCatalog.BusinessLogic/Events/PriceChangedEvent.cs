namespace ModsenCatalog.BusinessLogic.Events
{
    public class PriceChangedEvent : DomainEvent
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
    }
}
