using System.Linq;
using System.Windows;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    // Глобальная память для сессии (храним ID того, кто вошел)
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
            // Проверка: введены ли данные
            if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(TxtPass.Password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            using (var db = new Mfc111Context())
            {
                // Ищем сотрудника в твоей готовой базе
                var emp = db.Employees.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPass.Password);

                if (emp != null)
                {
                    // 1. Запоминаем ID сотрудника (чтобы потом знать, чье окно открывать)
                    AppSession.CurrentEmployeeId = emp.EmployeeId;

                    // 2. Открываем рабочее окно
                    EmployeeWindow workWindow = new EmployeeWindow();
                    workWindow.Show();

                    // 3. Закрываем окно логина
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