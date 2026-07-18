using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.Presentation.States;

public interface IMenuState
{
    void DisplayMenu();

    IMenuState? HandleInput(string input);
}