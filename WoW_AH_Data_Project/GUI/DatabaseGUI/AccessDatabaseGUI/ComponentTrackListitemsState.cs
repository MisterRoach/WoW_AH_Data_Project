using System.ComponentModel;

namespace WoWAHDataProject.GUI.DatabaseGUI.AccessDatabaseGUI;

public class DataBaseAccessTrackListitems : INotifyPropertyChanged
{
    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value; RaisePropertyChanged(nameof(IsChecked));
        }
    }
    private string _columnName;
    public string ColumnName
    {
        get => _columnName;
        set
        {
            if (_columnName == value)
            {
                return;
            }

            _columnName = value; RaisePropertyChanged(nameof(ColumnName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
