using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWAHDataProject.GUI
{
    public class View : INotifyPropertyChanged
    {
        private bool isChecked;
        private string columnName;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;
                isChecked = value; RaisePropertyChanged(nameof(IsChecked));
            }
        }
        public string ColumnName
        {
            get { return columnName; }
            set
            {
                if (columnName == value) return;
                columnName = value; RaisePropertyChanged(nameof(ColumnName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler eh = PropertyChanged;
            if(eh != null)
            {
                eh(this, new PropertyChangedEventArgs(propertyName));
            }
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
