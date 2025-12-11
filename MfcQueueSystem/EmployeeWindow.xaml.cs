using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class EmployeeWindow : Window
    {
        private Ticket? _currentTicket = null;
        private int _currentEmployeeId = 0;

        public EmployeeWindow()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (var db = new Mfc111Context())
            {
                EmployeesCombo.ItemsSource = db.Employees.Include(e => e.Window).ToList();
            }
        }

        private void EmployeesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesCombo.SelectedItem is Employee emp)
            {
                _currentEmployeeId = emp.EmployeeId;
                CurrentWindowText.Text = emp.WindowId != null ? $"№ {emp.Window.WindowNumber}" : "Нет окна";
                BtnCall.IsEnabled = true;
                BtnFinish.IsEnabled = false;
            }
        }

        private void BtnCall_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new Mfc111Context())
            {
                var nextTicket = db.Tickets
                    .Include(t => t.Service)
                    .Where(t => t.Status == "Waiting")
                    .OrderBy(t => t.TimeCreated)
                    .FirstOrDefault();

                if (nextTicket == null)
                {
                    MessageBox.Show("Очередь пуста!");
                    return;
                }

                nextTicket.Status = "InProgress";
                nextTicket.EmployeeId = _currentEmployeeId;
                var emp = db.Employees.Find(_currentEmployeeId);
                nextTicket.WindowId = emp?.WindowId;
                nextTicket.TimeStart = DateTime.Now;

                db.QueueLogs.Add(new QueueLog { TicketId = nextTicket.TicketId, EventTime = DateTime.Now, EventType = "Called", EmployeeId = _currentEmployeeId });
                db.SaveChanges();

                _currentTicket = nextTicket;
                CurrentTicketText.Text = nextTicket.TicketNumber;
                ServiceText.Text = nextTicket.Service.ServiceName;
                BtnCall.IsEnabled = false;
                BtnFinish.IsEnabled = true;
            }
        }

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
                    db.QueueLogs.Add(new QueueLog { TicketId = ticket.TicketId, EventTime = DateTime.Now, EventType = "Finished", EmployeeId = _currentEmployeeId });
                    db.SaveChanges();
                }

                CurrentTicketText.Text = "Свободен";
                ServiceText.Text = "Ожидание вызова...";
                BtnCall.IsEnabled = true;
                BtnFinish.IsEnabled = false;
                _currentTicket = null;
            }
            using (var db = new Mfc111Context())
            {
                // Ищем следующий талон:
                // 1. Статус "Ожидание"
                // 2. Сначала высокий Приоритет (по убыванию: 2, потом 1)
                // 3. Потом кто раньше пришел
                var nextTicket = db.Tickets
                    .Include(t => t.Service)
                    .Where(t => t.Status == "Waiting")
                    .OrderByDescending(t => t.Priority) // <--- ГЛАВНОЕ ИЗМЕНЕНИЕ
                    .ThenBy(t => t.TimeCreated)
                    .FirstOrDefault();

                if (nextTicket == null)
                {
                    MessageBox.Show("Очередь пуста!");
                    return;
                }
            }
        }
    }
};