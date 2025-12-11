using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class EmployeeWindow : Window
    {
        private Ticket? _currentTicket = null;
        private int _myId; // ID текущего сотрудника

        public EmployeeWindow()
        {
            InitializeComponent();
            _myId = AppSession.CurrentEmployeeId; // Получаем ID из сессии авторизации
            LoadMyState();
        }

        private void LoadMyState()
        {
            using (var db = new Mfc111Context())
            {
                // Загружаем данные о сотруднике и его окне
                var me = db.Employees.Include(e => e.Window).FirstOrDefault(e => e.EmployeeId == _myId);

                if (me != null)
                {
                    // Выводим имя и окно в новые текстовые блоки
                    OperatorNameText.Text = me.FullName;
                    CurrentWindowText.Text = me.WindowId != null ? $"№ {me.Window.WindowNumber}" : "Не назначено";
                    this.Title = $"МФЦ | Оператор: {me.FullName}";
                }

                // ПРОВЕРКА ВОССТАНОВЛЕНИЯ СЕССИИ
                // Если сотрудник закрыл программу, не завершив талон, мы его возвращаем
                var activeTicket = db.Tickets
                                     .Include(t => t.Service)
                                     .FirstOrDefault(t => t.EmployeeId == _myId && t.Status == "InProgress");

                if (activeTicket != null)
                {
                    // Восстанавливаем состояние "В работе"
                    _currentTicket = activeTicket;
                    CurrentTicketText.Text = activeTicket.TicketNumber;
                    ServiceText.Text = activeTicket.Service.ServiceName;

                    BtnCall.IsEnabled = false;
                    BtnFinish.IsEnabled = true;
                }
                else
                {
                    // Состояние "Готов к работе"
                    BtnCall.IsEnabled = true;
                    BtnFinish.IsEnabled = false;
                }
            }
        }

        // Кнопка ВЫЗВАТЬ
        private void BtnCall_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new Mfc111Context())
            {
                // Ищем следующий талон:
                // 1. Статус Waiting
                // 2. Сортировка: Сначала высокий приоритет (2), потом обычный (1)
                // 3. Сортировка: Кто раньше пришел
                var nextTicket = db.Tickets
                    .Include(t => t.Service)
                    .Where(t => t.Status == "Waiting")
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.TimeCreated)
                    .FirstOrDefault();

                if (nextTicket == null)
                {
                    MessageBox.Show("Очередь пуста! Отдохните :)");
                    return;
                }

                // Берем талон в работу
                nextTicket.Status = "InProgress";
                nextTicket.EmployeeId = _myId; // Привязываем к себе

                // Получаем мое окно
                var me = db.Employees.Find(_myId);
                nextTicket.WindowId = me?.WindowId;
                nextTicket.TimeStart = DateTime.Now;

                // Пишем лог
                db.QueueLogs.Add(new QueueLog
                {
                    TicketId = nextTicket.TicketId,
                    EventTime = DateTime.Now,
                    EventType = "Called",
                    EmployeeId = _myId
                });

                db.SaveChanges();

                // Обновляем интерфейс
                _currentTicket = nextTicket;
                CurrentTicketText.Text = nextTicket.TicketNumber;
                ServiceText.Text = nextTicket.Service.ServiceName;

                BtnCall.IsEnabled = false;
                BtnFinish.IsEnabled = true;
            }
        }

        // Кнопка ЗАВЕРШИТЬ
        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTicket == null) return;

            using (var db = new Mfc111Context())
            {
                var ticket = db.Tickets.Find(_currentTicket.TicketId);
                if (ticket != null)
                {
                    ticket.Status = "Completed";
                    ticket.TimeEnd = DateTime.Now;

                    db.QueueLogs.Add(new QueueLog
                    {
                        TicketId = ticket.TicketId,
                        EventTime = DateTime.Now,
                        EventType = "Finished",
                        EmployeeId = _myId
                    });

                    db.SaveChanges();
                }

                // Сброс интерфейса
                CurrentTicketText.Text = "Свободен";
                ServiceText.Text = "Нажмите «Вызвать», чтобы пригласить следующего";

                BtnCall.IsEnabled = true;
                BtnFinish.IsEnabled = false;
                _currentTicket = null;
            }
        }
    }
}