using System.Linq;
using System.Windows;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    // Глобальная память для сессии
    public static class AppSession
    {
        public static int CurrentEmployeeId { get; set; }
    }

    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new Mfc111Context())
            {
                var emp = db.Employees.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPass.Password);
                if (emp != null)
                {
                    // Сохраняем сессию
                    AppSession.CurrentEmployeeId = emp.EmployeeId;

                    // Открываем окно работы
                    new EmployeeWindow().Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!");
                }
            }
        }
    }
}