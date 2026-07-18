using Microsoft.Extensions.DependencyInjection;
using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.Presentation.States;

public class MenuContext
{
    private readonly IServiceProvider _serviceProvider;
    private IMenuState _currentState;
    public User? CurrentUser { get; private set; }

    public bool ShouldExit { get; private set; } = false;

    public MenuContext(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _currentState = new MainMenuState(this);
    }

    public IMenuState CurrentState => _currentState;

    public void SetState(IMenuState newState)
    {
        _currentState = newState;
        Console.Clear();
    }

    public void SetCurrentUser(User? user)
    {
        CurrentUser = user;
    }

    public void Exit()
    {
        ShouldExit = true;
    }

    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}