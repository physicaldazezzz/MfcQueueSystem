using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media; // Нужно для печати
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class TerminalWindow : Window
    {
        private DispatcherTimer? _timer;
        private int _selectedServiceId = 0;
        private List<Service> _allServices = new List<Service>();

        public TerminalWindow()
        {
            InitializeComponent();
            LoadServices();
            StartClock();
        }

        private void LoadServices()
        {
            using (var db = new Mfc111Context())
            {
                _allServices = db.Services.ToList();
                ServicesList.ItemsSource = _allServices;

                var categories = _allServices.Select(s => s.ServiceGroup).Distinct().OrderBy(s => s).ToList();
                categories.Insert(0, "Все услуги");
                CategoriesList.ItemsSource = categories;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterServices(SearchBox.Text, null);
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string category)
            {
                FilterServices(SearchBox.Text, category == "Все услуги" ? null : category);
            }
        }

        private void FilterServices(string searchText, string? category)
        {
            var filtered = _allServices.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(searchText))
                filtered = filtered.Where(s => s.ServiceName.ToLower().Contains(searchText.ToLower()));
            if (category != null)
                filtered = filtered.Where(s => s.ServiceGroup == category);
            ServicesList.ItemsSource = filtered.ToList();
        }

        // 1. НАЖАТИЕ НА КНОПКУ УСЛУГИ
        private void ServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int serviceId)
            {
                _selectedServiceId = serviceId;
                InputName.Text = "";
                CheckBenefit.IsChecked = false;
                RegistrationPopup.Visibility = Visibility.Visible;
            }
        }

        private void CancelReg_Click(object sender, RoutedEventArgs e) => RegistrationPopup.Visibility = Visibility.Collapsed;
        private void ClosePopup_Click(object sender, RoutedEventArgs e) => TicketPopup.Visibility = Visibility.Collapsed;

        // 2. ПОДТВЕРЖДЕНИЕ И ПЕЧАТЬ
        private void ConfirmReg_Click(object sender, RoutedEventArgs e)
        {
            string fio = InputName.Text.Trim();
            if (string.IsNullOrEmpty(fio))
            {
                MessageBox.Show("Пожалуйста, введите ФИО.");
                return;
            }

            bool isPrivileged = CheckBenefit.IsChecked == true;
            int priority = isPrivileged ? 2 : 1;

            using (var db = new Mfc111Context())
            {
                var service = db.Services.Find(_selectedServiceId);
                if (service == null) return;

                string prefix = service.ServiceName.Substring(0, 1).ToUpper();
                int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today) + 1;
                string ticketNum = $"{prefix}-{countToday:D3}";

                var newTicket = new Ticket
                {
                    TicketNumber = ticketNum,
                    ServiceId = _selectedServiceId,
                    TimeCreated = DateTime.Now,
                    Status = "Waiting",
                    ClientName = fio,
                    Priority = priority
                };

                db.Tickets.Add(newTicket);
                db.SaveChanges();

                db.QueueLogs.Add(new QueueLog
                {
                    TicketId = newTicket.TicketId,
                    EventTime = DateTime.Now,
                    EventType = "Created",
                    Note = isPrivileged ? "Льгота" : "Обычный"
                });
                db.SaveChanges();

                int peopleAhead = db.Tickets.Count(t =>
                    t.Status == "Waiting" &&
                    t.TicketId != newTicket.TicketId &&
                    (t.Priority > priority || (t.Priority == priority && t.TimeCreated < newTicket.TimeCreated))
                );

                // Заполняем данные на экране
                TicketNumberText.Text = ticketNum;
                TicketServiceText.Text = service.ServiceName;
                TicketInfoText.Text = fio;
                TicketPriorityText.Visibility = isPrivileged ? Visibility.Visible : Visibility.Collapsed;
                TicketCountText.Text = $"{peopleAhead} чел.";
                TicketTimeText.Text = DateTime.Now.ToString("dd.MM HH:mm");

                // Переключаем окна
                RegistrationPopup.Visibility = Visibility.Collapsed;
                TicketPopup.Visibility = Visibility.Visible;

                // !!! ЗАПУСКАЕМ ПЕЧАТЬ !!!
                // Даем системе немного времени на отрисовку попапа перед печатью
                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    PrintTicket();
                }));
            }
        }

        // --- ЛОГИКА ПЕЧАТИ ---
        private void PrintTicket()
        {
            PrintDialog printDialog = new PrintDialog();

            // Открываем стандартное окно выбора принтера Windows
            if (printDialog.ShowDialog() == true)
            {
                // Печатаем только элемент "TicketCard" (наш Border с чеком)
                // Можем добавить описание задачи печати
                printDialog.PrintVisual(TicketCard, $"Талон {TicketNumberText.Text}");
            }
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm");
            _timer.Start();
        }
    }
}