using System.Windows.Input;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI.ViewModels;
internal class AccessDatabaseWindowCommandViewModel(Action action) : ICommand
{
    private readonly Action _action = action;

    public void Execute(object o)
    {
        _action();
    }

    public bool CanExecute(object o)
    {
        return true;
    }

    public event EventHandler CanExecuteChanged
    {
        add { }
        remove { }
    }
}
