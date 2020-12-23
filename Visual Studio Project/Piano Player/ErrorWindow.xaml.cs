using System;
using System.Windows;

namespace Piano_Player
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow() { InitializeComponent(); }

        public static void ShowExceptionWindow(string message, Exception e)
        {
            ErrorWindow wnd = new ErrorWindow();
            message += "\n\nMessage: " + e.Message + "\n\n" + e.StackTrace;
            wnd.edit_text.Text = message;
            wnd.ShowDialog();
        }
    }
}
