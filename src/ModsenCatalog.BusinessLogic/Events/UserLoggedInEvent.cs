namespace ModsenCatalog.BusinessLogic.Events
{
    public class UserLoggedInEvent : DomainEvent
    {
        public string Username { get; set; } = string.Empty;
    }
}
