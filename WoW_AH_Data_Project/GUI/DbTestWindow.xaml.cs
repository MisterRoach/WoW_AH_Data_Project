using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WoWAHDataProject.Database;
using Forms = System.Windows.Forms;

namespace WoWAHDataProject.GUI
{
    /// <summary>
    /// Interaction logic for DbTestWindow.xaml
    /// </summary>
    public partial class DbTestWindow : Window
    {
        public DbTestWindow(SqliteConnection conn)
        {
            InitializeComponent();
            DataContext context = new DataContext();

            //using (SqliteConnection conn = new SqliteConnection(DatabaseMain.connString))
            //{
                //conn.Open();

                SqliteCommand command = new SqliteCommand("Select * from regularMarketValues", conn);
                DataTable dt = new DataTable("regularMarketValues");
                dt.Load(command.ExecuteReader());
                myDataGrid.ItemsSource = dt.AsDataView();

            //    conn.Close();
            
            //}
        }

        private void myDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Forms.MessageBox.Show("Right Clicked");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuItem).Header)
            {
                case "Open":
                    Forms.MessageBox.Show("Clicked open");
                    break;
                case "Delete":
                    Forms.MessageBox.Show("Delete");
                    break;
                case "Add":
                    Forms.MessageBox.Show("Add");
                    break;
                default:
                    break;
            }

        }
    }
}
