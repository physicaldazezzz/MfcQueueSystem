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

        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>
        {
            // Groups
            { "Паспортный стол", "Passport Office" },
            { "Росреестр", "Property Registration" },
            { "Налоги", "Taxes" },
            { "Социальные услуги", "Social Services" },
            { "Бизнес", "Business" },
            { "Прочее", "Other" },

            // Services (Examples)
            { "Получение паспорта РФ", "Get Russian Passport" },
            { "Загранпаспорт", "International Passport" },
            { "Регистрация права собственности", "Property Rights Registration" },
            { "Выписка из ЕГРН", "EGRN Extract" },
            { "ИНН", "Tax ID (INN)" },
            { "Регистрация ИП", "Sole Proprietor Registration" },
            { "Детские пособия", "Child Benefits" },
            { "СНИЛС", "SNILS Insurance" },
            { "Водительское удостоверение", "Driver's License" },
            { "Справка об отсутствии судимости", "Criminal Record Certificate" }
        };

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
                if (TxtPhysDesc != null) TxtPhysDesc.Text = "Личные документы, справки";
                if (TxtLegalTitle != null) TxtLegalTitle.Text = "Юридическое лицо";
                if (TxtLegalDesc != null) TxtLegalDesc.Text = "Бизнес, регистрация, налоги";
                if (TxtHaveBooking != null) TxtHaveBooking.Text = "У меня есть запись по времени";
                if (TxtBookingTitle != null) TxtBookingTitle.Text = "Активация записи";
                if (TxtBookingLabel != null) TxtBookingLabel.Text = "Введите код из SMS/Сайта:";
                if (BtnBookingConfirm != null) BtnBookingConfirm.Content = "АКТИВИРОВАТЬ";
                if (TxtMainHeader != null) TxtMainHeader.Text = "ЗАПИСЬ НА ПРИЕМ";
                if (TxtBackBtn != null) TxtBackBtn.Text = "🡠 Назад";
                if (TxtRegTitle != null) TxtRegTitle.Text = "Регистрация в очереди";
                if (TxtRegNameLabel != null) TxtRegNameLabel.Text = "Введите ваше ФИО:";
                if (TxtRegCatLabel != null) TxtRegCatLabel.Text = "Категория:";
                if (BtnRegCancel != null) BtnRegCancel.Content = "Отмена";
                if (BtnRegConfirm != null) BtnRegConfirm.Content = "ПОЛУЧИТЬ ТАЛОН";
                if (BtnBookingCancel != null) BtnBookingCancel.Content = "Отмена";
                if (TxtTicketTitle != null) TxtTicketTitle.Text = "ВАШ ТАЛОН";
                if (TxtTicketAhead != null) TxtTicketAhead.Text = "Перед вами:";
                if (BtnTicketClose != null) BtnTicketClose.Content = "Закрыть";
                if (TxtSearchPlaceholder != null) TxtSearchPlaceholder.Text = "🔍 Поиск услуги...";
            }
            else // ENGLISH
            {
                if (TxtWhoAreYou != null) TxtWhoAreYou.Text = "Who are you?";
                if (TxtPhysTitle != null) TxtPhysTitle.Text = "Individual";
                if (TxtPhysDesc != null) TxtPhysDesc.Text = "Personal documents, certificates";
                if (TxtLegalTitle != null) TxtLegalTitle.Text = "Legal Entity";
                if (TxtLegalDesc != null) TxtLegalDesc.Text = "Business, registration, taxes";
                if (TxtHaveBooking != null) TxtHaveBooking.Text = "I have a scheduled appointment";
                if (TxtBookingTitle != null) TxtBookingTitle.Text = "Check-in";
                if (TxtBookingLabel != null) TxtBookingLabel.Text = "Enter booking code:";
                if (BtnBookingConfirm != null) BtnBookingConfirm.Content = "ACTIVATE";
                if (TxtMainHeader != null) TxtMainHeader.Text = "NEW TICKET";
                if (TxtBackBtn != null) TxtBackBtn.Text = "🡠 Back";
                if (TxtRegTitle != null) TxtRegTitle.Text = "New Ticket Registration";
                if (TxtRegNameLabel != null) TxtRegNameLabel.Text = "Enter your Name:";
                if (TxtRegCatLabel != null) TxtRegCatLabel.Text = "Category:";
                if (BtnRegCancel != null) BtnRegCancel.Content = "Cancel";
                if (BtnRegConfirm != null) BtnRegConfirm.Content = "GET TICKET";
                if (BtnBookingCancel != null) BtnBookingCancel.Content = "Cancel";
                if (TxtTicketTitle != null) TxtTicketTitle.Text = "YOUR TICKET";
                if (TxtTicketAhead != null) TxtTicketAhead.Text = "People ahead:";
                if (BtnTicketClose != null) BtnTicketClose.Content = "Close";
                if (TxtSearchPlaceholder != null) TxtSearchPlaceholder.Text = "🔍 Search service...";
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
                ticket.Priority = 20;       // Приоритет (как по записи - выше всех)
                ticket.TimeCreated = DateTime.Now; // Время прихода - сейчас

                db.SaveChanges();

                // Печатаем талон
                PrintTicket(ticket, 0); // 0 человек перед ним (условно), так как высокий приоритет
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
                ComboBenefits.SelectedIndex = 0; // Сброс на "Нет льгот"
                RegistrationPopup.Visibility = Visibility.Visible;
            }
        }

        private void CancelReg_Click(object sender, RoutedEventArgs e) => RegistrationPopup.Visibility = Visibility.Collapsed;
        private void ClosePopup_Click(object sender, RoutedEventArgs e) => TicketPopup.Visibility = Visibility.Collapsed;

        private void ConfirmReg_Click(object sender, RoutedEventArgs e)
        {
            string fio = InputName.Text.Trim();
            if (string.IsNullOrEmpty(fio)) { MessageBox.Show(_currentLang == "EN" ? "Name required" : "ФИО обязательно"); return; }

            using (var db = new Mfc111Context())
            {
                var service = db.Services.Find(_tempServiceId);
                string prefix = service.ServiceName.Substring(0, 1).ToUpper();
                int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today && t.Status != "Booked") + 1;
                string ticketNum = $"{prefix}-{countToday:D3}";
                
                // Получаем приоритет из ComboBox
                int priority = 1; // Default
                if (ComboBenefits.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    int.TryParse(item.Tag.ToString(), out priority);
                    if (priority == 0) priority = 1;
                }

                var t = new Ticket
                {
                    TicketNumber = ticketNum,
                    ServiceId = _tempServiceId,
                    TimeCreated = DateTime.Now,
                    Status = "Waiting",
                    ClientName = fio,
                    Priority = priority
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

            // Если приоритет > 1 - показываем VIP или ПО ЗАПИСИ
            if (ticket.Priority > 1)
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
