using System;
using System.Collections.Generic;
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

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for SheetMergerWindow.xaml
    /// </summary>
    public partial class SheetMergerWindow : Window
    {
        // =======================================================
        public readonly MainWindow ParentWindow;
        // =======================================================
        public SheetMergerWindow(MainWindow parent)
        {
            ParentWindow = parent;
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ParentWindow.menu_tools_mergesheets.IsEnabled = true;
        }
        // =======================================================
    }
}
