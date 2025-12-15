using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore; // ДЛЯ INCLUDE
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class TerminalWindow : Window
    {
        private DispatcherTimer? _timer;
        private List<Service> _allServices = new List<Service>();

        private string _targetType = "PHYS";
        private string _currentLang = "RU";
        private int _tempServiceId;

        // СЛОВАРЬ (Сократил для примера, твой большой словарь останется работать, если он был)
        // ... (Сюда вставь свой большой словарь из прошлого шага, если хочешь перевод) ...
        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>();

        public TerminalWindow()
        {
            InitializeComponent();
            StartClock();
            LoadServicesToMemory();
            SetLanguage("RU");
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => ClockText.Text = DateTime.Now.ToString("HH:mm");
            _timer.Start();
        }

        private void LoadServicesToMemory()
        {
            using (var db = new Mfc111Context())
            {
                _allServices = db.Services.ToList();
            }
        }

        // --- ПЕРЕВОД ---
        private string Translate(string text)
        {
            if (_currentLang == "RU") return text;
            return _translations.ContainsKey(text) ? _translations[text] : text;
        }

        private void SetLanguage(string lang)
        {
            _currentLang = lang;
            bool isEn = lang == "EN";

            if (lang == "RU")
            {
                if (TxtWhoAreYou != null) TxtWhoAreYou.Text = "Кто вы?";
                if (TxtPhysTitle != null) TxtPhysTitle.Text = "Физическое лицо";
                if (TxtHaveBooking != null) TxtHaveBooking.Text = "У меня есть запись по времени";
                if (TxtBookingTitle != null) TxtBookingTitle.Text = "Активация записи";
                if (TxtBookingLabel != null) TxtBookingLabel.Text = "Введите код из SMS/Сайта:";
                if (BtnBookingConfirm != null) BtnBookingConfirm.Content = "АКТИВИРОВАТЬ";

                // ... Остальные тексты ...
            }
            else // ENGLISH
            {
                if (TxtWhoAreYou != null) TxtWhoAreYou.Text = "Who are you?";
                if (TxtPhysTitle != null) TxtPhysTitle.Text = "Individual";
                if (TxtHaveBooking != null) TxtHaveBooking.Text = "I have a scheduled appointment";
                if (TxtBookingTitle != null) TxtBookingTitle.Text = "Check-in";
                if (TxtBookingLabel != null) TxtBookingLabel.Text = "Enter booking code:";
                if (BtnBookingConfirm != null) BtnBookingConfirm.Content = "ACTIVATE";

                // ... Rest of texts ...
            }
            if (MainContentGrid.Visibility == Visibility.Visible) UpdateCategories();
        }

        private void LangRu_Click(object sender, RoutedEventArgs e) => SetLanguage("RU");
        private void LangEn_Click(object sender, RoutedEventArgs e) => SetLanguage("EN");

        // --- ЛОГИКА АКТИВАЦИИ ЗАПИСИ (НОВОЕ) ---
        private void ActivateBooking_Click(object sender, RoutedEventArgs e)
        {
            BookingCodeInput.Text = "";
            BookingPopup.Visibility = Visibility.Visible;
            BookingCodeInput.Focus();
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e) => BookingPopup.Visibility = Visibility.Collapsed;

        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            string code = BookingCodeInput.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;

            using (var db = new Mfc111Context())
            {
                // Ищем талон с таким кодом, который "Забронирован" и на СЕГОДНЯ
                var ticket = db.Tickets
                    .Include(t => t.Service)
                    .FirstOrDefault(t => t.BookingCode == code &&
                                         t.Status == "Booked" &&
                                         t.AppointmentTime != null);

                if (ticket == null)
                {
                    MessageBox.Show(_currentLang == "EN" ? "Code not found!" : "Код не найден или уже использован!");
                    return;
                }

                // Проверка даты (активировать можно только в день записи)
                if (ticket.AppointmentTime.Value.Date != DateTime.Today)
                {
                    MessageBox.Show(_currentLang == "EN"
                        ? $"Appointment is on {ticket.AppointmentTime.Value:dd.MM.yyyy}. Come back later!"
                        : $"Ваша запись на {ticket.AppointmentTime.Value:dd.MM.yyyy}. Приходите в назначенный день!");
                    return;
                }

                // АКТИВАЦИЯ
                // Генерируем нормальный номер (П-00X)
                string prefix = ticket.Service.ServiceName.Substring(0, 1).ToUpper();
                int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today && t.Status != "Booked") + 1;
                ticket.TicketNumber = $"{prefix}-{countToday:D3}";

                ticket.Status = "Waiting"; // Ставим в живую очередь
                ticket.Priority = 2;       // Приоритет (как льготник)
                ticket.TimeCreated = DateTime.Now; // Время прихода - сейчас

                db.SaveChanges();

                // Печатаем талон
                PrintTicket(ticket, 0); // 0 человек перед ним, так как он по записи (VIP)

                BookingPopup.Visibility = Visibility.Collapsed;
            }
        }

        // --- ЛОГИКА ОБЫЧНОЙ ВЫДАЧИ ---
        private void SelectPhys_Click(object sender, RoutedEventArgs e) { _targetType = "PHYS"; ShowServices(); }
        private void SelectLegal_Click(object sender, RoutedEventArgs e) { _targetType = "LEGAL"; ShowServices(); }

        private void ShowServices()
        {
            StartScreenGrid.Visibility = Visibility.Collapsed;
            MainContentGrid.Visibility = Visibility.Visible;
            SearchBox.Text = "";
            FilterServices("", null);
            UpdateCategories();
        }

        private void BackToStart_Click(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Visibility = Visibility.Collapsed;
            StartScreenGrid.Visibility = Visibility.Visible;
        }

        private void UpdateCategories()
        {
            var relevantServices = _allServices.Where(s => s.TargetType == _targetType || s.TargetType == "BOTH");
            var cats = relevantServices.Select(s => Translate(s.ServiceGroup)).Distinct().OrderBy(s => s).ToList();
            string allLabel = _currentLang == "EN" ? "All Services" : "Все услуги";
            cats.Insert(0, allLabel);
            CategoriesList.ItemsSource = cats;
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string category)
            {
                string allLabel = _currentLang == "EN" ? "All Services" : "Все услуги";
                FilterServices(SearchBox.Text, category == allLabel ? null : category);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => FilterServices(SearchBox.Text, null);

        private void FilterServices(string searchText, string? category)
        {
            var filtered = _allServices.Where(s => s.TargetType == _targetType || s.TargetType == "BOTH");
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string q = searchText.ToLower();
                filtered = filtered.Where(s => s.ServiceName.ToLower().Contains(q) || Translate(s.ServiceName).ToLower().Contains(q));
            }
            if (category != null) filtered = filtered.Where(s => Translate(s.ServiceGroup) == category);

            var displayList = filtered.Select(s => new {
                s.ServiceId,
                ServiceName = Translate(s.ServiceName),
                s.ServiceGroup
            }).ToList();
            ServicesList.ItemsSource = displayList;
        }

        private void ServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int serviceId)
            {
                _tempServiceId = serviceId;
                InputName.Text = "";
                CheckBenefit.IsChecked = false;
                RegistrationPopup.Visibility = Visibility.Visible;
            }
        }

        private void CancelReg_Click(object sender, RoutedEventArgs e) => RegistrationPopup.Visibility = Visibility.Collapsed;
        private void ClosePopup_Click(object sender, RoutedEventArgs e) => TicketPopup.Visibility = Visibility.Collapsed;

        private void ConfirmReg_Click(object sender, RoutedEventArgs e)
        {
            string fio = InputName.Text.Trim();
            if (string.IsNullOrEmpty(fio)) { MessageBox.Show("ФИО / Name required"); return; }

            using (var db = new Mfc111Context())
            {
                var service = db.Services.Find(_tempServiceId);
                string prefix = service.ServiceName.Substring(0, 1).ToUpper();
                int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today && t.Status != "Booked") + 1;
                string ticketNum = $"{prefix}-{countToday:D3}";
                bool isVip = CheckBenefit.IsChecked == true;

                var t = new Ticket
                {
                    TicketNumber = ticketNum,
                    ServiceId = _tempServiceId,
                    TimeCreated = DateTime.Now,
                    Status = "Waiting",
                    ClientName = fio,
                    Priority = isVip ? 2 : 1
                };
                db.Tickets.Add(t);
                db.SaveChanges();

                db.QueueLogs.Add(new QueueLog { TicketId = t.TicketId, EventTime = DateTime.Now, EventType = "Created", Note = "Terminal" });
                db.SaveChanges();

                int ahead = db.Tickets.Count(x => x.Status == "Waiting" && x.TicketId != t.TicketId && (x.Priority > t.Priority || (x.Priority == t.Priority && x.TimeCreated < t.TimeCreated)));

                PrintTicket(t, ahead);
                RegistrationPopup.Visibility = Visibility.Collapsed;
            }
        }

        // --- ОБЩИЙ МЕТОД ПЕЧАТИ ---
        private void PrintTicket(Ticket ticket, int peopleAhead)
        {
            TicketNumberText.Text = ticket.TicketNumber;
            TicketServiceText.Text = Translate(ticket.Service.ServiceName);
            TicketInfoText.Text = ticket.ClientName;

            // Если приоритет 2 - показываем VIP или ПО ЗАПИСИ
            if (ticket.Priority == 2)
            {
                TicketPriorityText.Visibility = Visibility.Visible;
                // Если есть код брони - значит по записи
                if (!string.IsNullOrEmpty(ticket.BookingCode))
                    TicketPriorityText.Text = _currentLang == "EN" ? "★ PRE-BOOKED" : "★ ПО ЗАПИСИ";
                else
                    TicketPriorityText.Text = _currentLang == "EN" ? "★ PRIORITY" : "★ ЛЬГОТНАЯ ОЧЕРЕДЬ";
            }
            else
            {
                TicketPriorityText.Visibility = Visibility.Collapsed;
            }

            string unit = _currentLang == "EN" ? "pers." : "чел.";
            TicketCountText.Text = $"{peopleAhead} {unit}";
            TicketTimeText.Text = DateTime.Now.ToString("dd.MM HH:mm");

            TicketPopup.Visibility = Visibility.Visible;

            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true) pd.PrintVisual(TicketCard, ticket.TicketNumber);
            }));
        }
    }
}