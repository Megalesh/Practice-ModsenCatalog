using System;
using System.Collections.Generic;
using System.Text;

namespace ModsenCatalog.BusinessLogic.Events
{
    public class UserRoleChangedEvent : DomainEvent
    {
        public string Username { get; set; } = string.Empty;
        public string OldRole { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
