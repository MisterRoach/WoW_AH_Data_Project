using System.ComponentModel;

namespace WoWAHDataProject.GUI;

public class DataBaseAccessTrackListitems : INotifyPropertyChanged
{
    private bool isChecked;
    private string columnName;
    public bool IsChecked
    {
        get => isChecked;
        set
        {
            if (isChecked == value)
            {
                return;
            }

            isChecked = value; RaisePropertyChanged(nameof(IsChecked));
        }
    }
    public string ColumnName
    {
        get => columnName;
        set
        {
            if (columnName == value)
            {
                return;
            }

            columnName = value; RaisePropertyChanged(nameof(ColumnName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
