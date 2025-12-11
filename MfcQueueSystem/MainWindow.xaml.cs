using System.Windows;

namespace MfcQueueSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenTerminal_Click(object sender, RoutedEventArgs e)
        {
            new TerminalWindow().Show();
        }

        private void OpenDisplay_Click(object sender, RoutedEventArgs e)
        {
            // Если DisplayWindow.xaml нет - создай его через Проект -> Добавить окно
            new DisplayWindow().Show();
        }

        private void OpenEmployee_Click(object sender, RoutedEventArgs e)
        {
         
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

        }
    }
}