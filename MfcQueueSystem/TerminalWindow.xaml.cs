using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class TerminalWindow : Window
    {
        private string _selectedType = "Physical"; // Тип заявителя
        private string _selectedCategory = "";

        public TerminalWindow()
        {
            InitializeComponent();
        }

        // --- НАВИГАЦИЯ ---
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainTabControl.SelectedIndex > 0)
                MainTabControl.SelectedIndex--;
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 0; // На выбор языка
        }

        // --- ШАГ 1: Язык ---
        private void Lang_Ru_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 1;
        private void Lang_En_Click(object sender, RoutedEventArgs e) => MainTabControl.SelectedIndex = 1; // Пока тоже самое

        // --- ШАГ 2: Тип заявителя ---
        private void Type_Phys_Click(object sender, RoutedEventArgs e)
        {
            _selectedType = "Physical";
            LoadCategories();
            MainTabControl.SelectedIndex = 2;
        }

        private void Type_Legal_Click(object sender, RoutedEventArgs e)
        {
            _selectedType = "Legal";
            LoadCategories();
            MainTabControl.SelectedIndex = 2;
        }

        private void LoadCategories()
        {
            using (var db = new Mfc111Context())
            {
                // Грузим категории только для выбранного типа
                var cats = db.Services
                    .Where(s => s.ApplicantType == _selectedType)
                    .Select(s => s.ServiceGroup)
                    .Distinct()
                    .ToList();
                CategoriesList.ItemsSource = cats;
            }
        }

        // --- ШАГ 3: Категория ---
        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string cat)
            {
                _selectedCategory = cat;
                LoadServices();
                MainTabControl.SelectedIndex = 3;
            }
        }

        private void LoadServices()
        {
            using (var db = new Mfc111Context())
            {
                var services = db.Services
                    .Where(s => s.ApplicantType == _selectedType && s.ServiceGroup == _selectedCategory)
                    .ToList();
                ServicesList.ItemsSource = services;
            }
        }

        // --- ШАГ 4: Выбор услуги и Печать ---
        private void Service_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int serviceId)
            {
                using (var db = new Mfc111Context())
                {
                    var service = db.Services.Find(serviceId);

                    // Генерация номера
                    string prefix = service.ServiceName.Substring(0, 1).ToUpper();
                    int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today) + 1;
                    string ticketNum = $"{prefix}-{countToday:D3}";

                    // Сохраняем в БД
                    var newTicket = new Ticket
                    {
                        TicketNumber = ticketNum,
                        ServiceId = serviceId,
                        TimeCreated = DateTime.Now,
                        Status = "Waiting",
                        ClientName = "Терминал", // Без ввода ФИО для ускорения (или можно добавить шаг)
                        Priority = 1
                    };
                    db.Tickets.Add(newTicket);
                    db.SaveChanges();

                    // Лог
                    db.QueueLogs.Add(new QueueLog { TicketId = newTicket.TicketId, EventTime = DateTime.Now, EventType = "Created", Note = "Terminal Wizard" });
                    db.SaveChanges();

                    // Расчет ожидания
                    int peopleAhead = db.Tickets.Count(t => t.Status == "Waiting");
                    int waitMin = (peopleAhead * 10) + 5;

                    // ЗАПОЛНЯЕМ ТАЛОН НА ЭКРАНЕ
                    TicketNum.Text = ticketNum;
                    TicketService.Text = service.ServiceName;
                    TicketTime.Text = DateTime.Now.ToString("dd.MM HH:mm");
                    TicketWait.Text = $"~{waitMin} мин.";

                    // Переход на финиш
                    MainTabControl.SelectedIndex = 4;

                    // ПЕЧАТЬ
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        PrintDialog pd = new PrintDialog();
                        if (pd.ShowDialog() == true)
                        {
                            pd.PrintVisual(TicketCard, "Ticket");
                        }
                    }));
                }
            }
        }
    }
}