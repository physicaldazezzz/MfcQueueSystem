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
    // Модель категории льготы
    public class BenefitCategory
    {
        public string NameRu { get; set; } = "";
        public string NameEn { get; set; } = "";
        public int Priority { get; set; }
    }

    public partial class TerminalWindow : Window
    {
        private DispatcherTimer? _timer;
        private List<Service> _allServices = new List<Service>();

        private string _targetType = "PHYS";
        private string _currentLang = "RU";
        private int _tempServiceId;

        // Список льгот
        private readonly List<BenefitCategory> _benefits = new List<BenefitCategory>
        {
            new BenefitCategory { NameRu = "Нет льгот", NameEn = "No benefits", Priority = 1 },
            // Федеральные (High Priority)
            new BenefitCategory { NameRu = "Участники СВО и члены семей", NameEn = "SVO Participants & Families", Priority = 15 },
            new BenefitCategory { NameRu = "Ветераны ВОВ, Инвалиды", NameEn = "WW2 Veterans, Disabled", Priority = 15 },
            new BenefitCategory { NameRu = "Герои России/СССР", NameEn = "Heroes of Russia/USSR", Priority = 15 },
            new BenefitCategory { NameRu = "Инвалиды I и II групп", NameEn = "Disabled Group I & II", Priority = 15 },
            new BenefitCategory { NameRu = "Дети-инвалиды", NameEn = "Disabled Children", Priority = 15 },
            new BenefitCategory { NameRu = "Жители блокадного Ленинграда", NameEn = "Siege of Leningrad Survivors", Priority = 15 },
            // Другие
            new BenefitCategory { NameRu = "Пенсионеры", NameEn = "Pensioners", Priority = 3 },
            new BenefitCategory { NameRu = "Многодетные семьи", NameEn = "Large Families", Priority = 5 }
        };

        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
             // --- КАТЕГОРИИ (ServiceGroup) ---
             {"Паспорта", "Passports"},
             {"Регистрация", "Registration"},
             {"Миграция", "Migration"},
             {"Транспорт", "Transport"},
             {"Транспорт Бизнес", "Transport (Business)"},
             {"ЗАГС", "Civil Registry (ZAGS)"},
             {"Образование", "Education"},
             {"Семья", "Family"},
             {"Пособия", "Benefits"},
             {"Недвижимость", "Real Estate"},
             {"Земля", "Land"},
             {"Жилье", "Housing"},
             {"Строительство", "Construction"},
             {"Пенсионный", "Pension Fund"},
             {"Соцзащита", "Social Protection"},
             {"ЖКХ", "Utilities (Housing)"},
             {"Налоги", "Taxes"},
             {"Налоги Бизнес", "Taxes (Business)"},
             {"Регистрация Бизнеса", "Business Registration"},
             {"Реестры", "Registries (EGRUL/EGRIP)"},
             {"Бизнес Старт", "Business Start"},
             {"Лицензирование", "Licensing"},
             {"Поддержка МСП", "SME Support"},
             {"Справки МВД", "Police Certificates"},
             {"Архив", "Archive"},
             {"Справки", "Certificates"},
             {"Природа", "Nature & Hunting"},
             {"Цифра", "Digital Services"},
             {"Банкротство", "Bankruptcy"},
             {"Здравоохранение", "Healthcare"},

             // --- УСЛУГИ (ServiceName) ---
             // Паспорта
             {"Выдача/замена паспорта РФ (по возрасту)", "Issue/replacement of RF passport (by age)"},
             {"Выдача/замена паспорта РФ (утеря/хищение)", "Issue/replacement of RF passport (lost/stolen)"},
             {"Выдача/замена паспорта РФ (смена ФИО)", "Issue/replacement of RF passport (name change)"},
             {"Загранпаспорт старого образца (5 лет) - взрослым", "International passport (5 years) - adults"},
             {"Загранпаспорт старого образца (5 лет) - детям", "International passport (5 years) - children"},
             {"Загранпаспорт нового образца (10 лет, биометрия)", "Biometric international passport (10 years)"},
             
             // Регистрация
             {"Регистрация по месту жительства (прописка)", "Permanent residence registration"},
             {"Снятие с регистрационного учета", "Deregistration of residence"},
             {"Регистрация по месту пребывания (временная)", "Temporary residence registration"},
             {"Миграционный учет иностранных граждан", "Migration registration of foreigners"},
             {"Вид на жительство (прием заявлений)", "Residence permit application"},
             {"Разрешение на временное проживание (РВП)", "Temporary residence permit (RVP)"},

             // Транспорт
             {"Замена водительского удостоверения (национального)", "Driver's license replacement (National)"},
             {"Получение международного ВУ", "International driving permit"},
             {"Карта водителя для тахографа (РФ)", "Tachograph driver card (RF)"},
             {"Карта водителя для тахографа (ЕСТР)", "Tachograph driver card (ESTR)"},
             {"Парковочное разрешение резидента", "Resident parking permit"},
             {"Парковочное разрешение инвалида", "Disabled parking permit"},
             {"Парковочное разрешение многодетной семьи", "Large family parking permit"},
             {"Разрешение на возврат ТС со штрафстоянки", "Vehicle release form (impound lot)"},
             {"Выдача разрешения на такси", "Taxi permit issuance"},
             {"Аннулирование разрешения на такси", "Taxi permit cancellation"},
             {"Спецразрешение на тяжеловесные грузы", "Heavy cargo special permit"},
             {"Разрешение на перевозку опасных грузов", "Dangerous goods transport permit"},

             // Семья и ЗАГС
             {"Регистрация рождения", "Birth registration"},
             {"Регистрация расторжения брака", "Divorce registration"},
             {"Регистрация смерти", "Death registration"},
             {"Выдача повторного свидетельства о рождении", "Duplicate birth certificate"},
             {"Выдача повторного свидетельства о браке", "Duplicate marriage certificate"},
             {"Установление отцовства", "Establishment of paternity"},
             {"Запись в детский сад", "Kindergarten enrollment"},
             {"Запись в первый класс", "School enrollment (1st grade)"},
             {"Запись в кружки и секции", "Enrollment in clubs/sections"},
             {"Оформление сертификата на материнский капитал", "Maternity capital certificate"},
             {"Распоряжение мат. капиталом (ипотека)", "Maternity capital use (Mortgage)"},
             {"Распоряжение мат. капиталом (образование)", "Maternity capital use (Education)"},
             {"Распоряжение мат. капиталом (выплата)", "Maternity capital use (Payout)"},
             {"Единовременное пособие при рождении ребенка", "Lump-sum birth allowance"},
             {"Ежемесячное пособие по уходу за ребенком до 1.5 лет", "Monthly child care allowance (up to 1.5 yrs)"},
             {"Выплаты на детей от 3 до 7 лет", "Child payments (3 to 7 years)"},
             {"Удостоверение многодетной семьи", "Large family certificate"},
             {"Предоставление земельных участков многодетным", "Land plot for large families"},

             // Недвижимость
             {"Регистрация права собственности (купля-продажа)", "Property registration (Sale)"},
             {"Регистрация права собственности (дарение)", "Property registration (Gift)"},
             {"Регистрация права собственности (наследство)", "Property registration (Inheritance)"},
             {"Регистрация ипотеки", "Mortgage registration"},
             {"Погашение регистрационной записи об ипотеке", "Mortgage record cancellation"},
             {"Постановка на кадастровый учет", "Cadastral registration"},
             {"Единая процедура (учет + регистрация)", "Unified procedure (Accounting + Reg)"},
             {"Предоставление сведений из ЕГРН (Об объекте)", "EGRN Extract (Object info)"},
             {"Предоставление сведений из ЕГРН (О правах)", "EGRN Extract (Rights info)"},
             {"Предоставление сведений из ЕГРН (Кадастровый план)", "EGRN Extract (Cadastral plan)"},
             {"Исправление технической ошибки в ЕГРН", "EGRN technical error correction"},
             {"Дачная амнистия (регистрация дома)", "Dacha amnesty (House registration)"},
             {"Отказ от права собственности на землю", "Land ownership renunciation"},

             // Пенсии и Соцзащита
             {"Оформление СНИЛС (АДИ-РЕГ)", "SNILS Registration"},
             {"Замена СНИЛС (смена фамилии)", "SNILS Replacement (Name change)"},
             {"Назначение страховой пенсии по старости", "Old-age insurance pension"},
             {"Перерасчет пенсии", "Pension recalculation"},
             {"Смена способа доставки пенсии (карта/почта)", "Change pension delivery method"},
             {"Справка о размере пенсии", "Pension amount certificate"},
             {"Установление статуса предпенсионера", "Pre-pensioner status"},
             {"Оформление инвалидности (прием документов)", "Disability registration"},
             {"Обеспечение средствами реабилитации", "Rehabilitation means provision"},
             {"Санаторно-курортное лечение (путевки)", "Sanatorium treatment vouchers"},
             {"Бесплатный проезд к месту лечения", "Free travel to treatment place"},
             {"Звание \"Ветеран труда\"", "Veteran of Labour title"},
             {"Социальная карта жителя (оформление)", "Social resident card"},
             {"Субсидии на оплату ЖКУ", "Housing utility subsidies"},
             {"Компенсация расходов по ЖКУ (льготники)", "Housing utility compensation"},

             // Налоги
             {"Постановка на учет (ИНН)", "Tax ID (INN) registration"},
             {"Прием декларации 3-НДФЛ", "3-NDFL Declaration submission"},
             {"Заявление на льготу по трансп./зем./имущ. налогу", "Tax relief application"},
             {"Доступ к Личному кабинету налогоплательщика", "Taxpayer account access"},
             {"Справка об исполнении обязанностей (долги)", "Tax debt certificate"},
             {"Регистрация ККТ (кассовой техники)", "Cash register registration"},
             {"Выдача дубликата ИНН", "Duplicate INN"},

             // Бизнес
             {"Регистрация физического лица как ИП", "Sole Proprietor Registration"},
             {"Регистрация создания Юридического Лица (ООО)", "LLC Registration"},
             {"Прекращение деятельности ИП", "Sole Proprietor Termination"},
             {"Ликвидация Юридического Лица", "LLC Liquidation"},
             {"Внесение изменений в ЕГРИП (ОКВЭД и пр.)", "Changes to EGRIP"},
             {"Внесение изменений в ЕГРЮЛ (Устав, Директор)", "Changes to EGRUL"},
             {"Выписка из ЕГРЮЛ/ЕГРИП", "Extract from EGRUL/EGRIP"},
             {"Подача уведомления о начале деятельности (Роспотребнадзор)", "Notice of business start"},
             {"Лицензия на розничную продажу алкоголя", "Alcohol retail license"},
             {"Лицензия на заготовку лома металлов", "Scrap metal license"},
             {"Лицензия на фармацевтическую деятельность", "Pharmaceutical license"},
             {"Лицензия на образовательную деятельность", "Educational activity license"},
             {"Субсидии для МСП (консультация)", "SME Subsidies consultation"},
             {"Регистрация в системе \"Честный знак\"", "Chestny ZNAK registration"},
             {"Открытие расчетного счета (партнеры)", "Bank account opening"},

             // Строительство и Земля
             {"Градостроительный план земельного участка (ГПЗУ)", "Urban development plan (GPZU)"},
             {"Разрешение на строительство (ИЖС)", "Construction permit (Individual)"},
             {"Уведомление о планируемом строительстве", "Construction notice"},
             {"Уведомление об окончании строительства", "Construction completion notice"},
             {"Присвоение адреса объекту", "Address assignment"},
             {"Перевод жилого помещения в нежилое", "Residential to non-residential conversion"},
             {"Согласование перепланировки", "Redevelopment approval"},
             {"Оформление прав на садовый дом (Амнистия)", "Garden house rights"},
             {"Утверждение схемы расположения ЗУ на кадастровом плане", "Land plot layout approval"},
             {"Предоставление земельного участка в аренду (торги)", "Land lease (Auction)"},
             {"Предоставление земельного участка без торгов", "Land lease (No auction)"},
             {"Изменение вида разрешенного использования ЗУ", "Change of land use type"},

             // Справки и Архив
             {"Справка о наличии/отсутствии судимости", "Criminal record certificate"},
             {"Справка об административных наказаниях (наркотики)", "Drug offense certificate"},
             {"Архивная справка о трудовом стаже", "Archival work history certificate"},
             {"Архивная справка о зарплате", "Archival salary certificate"},
             {"Архивная копия постановления администрации", "Archival decree copy"},
             {"Справка о составе семьи (выписка из домовой)", "Family composition certificate"},

             // Природа
             {"Выдача охотничьего билета", "Hunting ticket issuance"},
             {"Аннулирование охотничьего билета", "Hunting ticket cancellation"},
             {"Разрешение на добычу охотничьих ресурсов", "Hunting permit"},
             {"Лесная декларация (прием)", "Forest declaration"},
             {"Отчет об использовании лесов", "Forest usage report"},

             // Цифра и прочее
             {"Регистрация учетной записи ЕСИА (Госуслуги)", "Gosuslugi registration"},
             {"Подтверждение личности для ЕСИА", "Gosuslugi identity verification"},
             {"Восстановление доступа к ЕСИА", "Gosuslugi access recovery"},
             {"Распечатка QR-кода о вакцинации", "Vaccination QR-code printout"},
             {"Консультация по банкротству физлиц (внесудебное)", "Bankruptcy consultation"},
             {"Прием заявления о банкротстве физлица", "Bankruptcy application"},
             {"Запись на прием к врачу", "Doctor appointment"},
             {"Полис ОМС (оформление/замена)", "OMS Policy (Health insurance)"}
        };

        public TerminalWindow()
        {
            InitializeComponent();
            StartClock();
            LoadServicesToMemory();
            
            ComboBenefits.DisplayMemberPath = "Name";
            ComboBenefits.SelectedValuePath = "Priority";
            
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
            if (string.IsNullOrEmpty(text)) return "";
            
            // Try exact match first
            if (_translations.ContainsKey(text)) return _translations[text];
            
            // Fallback: if not found, maybe split words or return original? 
            // For now, return original if not in dictionary to avoid confusion.
            return text;
        }

        private void SetLanguage(string lang)
        {
            _currentLang = lang;
            bool isEn = lang == "EN";

            // Обновляем список льгот в ComboBox
            var displayBenefits = _benefits.Select(b => new 
            { 
                Name = isEn ? b.NameEn : b.NameRu, 
                Priority = b.Priority 
            }).ToList();
            
            int selectedIdx = ComboBenefits.SelectedIndex;
            ComboBenefits.ItemsSource = displayBenefits;
            ComboBenefits.SelectedIndex = selectedIdx >= 0 ? selectedIdx : 0;

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
                if (TxtTicketHeader != null) TxtTicketHeader.Text = "МФЦ МОИ ДОКУМЕНТЫ";
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
                if (TxtTicketHeader != null) TxtTicketHeader.Text = "MFC / MY DOCUMENTS";
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
                // ВАЖНО: Проверяем, что человек пришел в интервале [Время записи - 10 минут, Время записи + 10 минут]
                // Или просто "не просрочил более чем на 10 минут"
                
                var now = DateTime.Now;
                
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

                // Проверка времени: можно активировать только в интервале [Время - 30 мин ... Время + 10 мин]
                // Если опоздал больше чем на 10 минут -> бронь сгорает
                var appTime = ticket.AppointmentTime.Value;
                if (now > appTime.AddMinutes(10))
                {
                    // Просрочено -> Удаляем или меняем статус на "Missed"
                    ticket.Status = "Missed";
                    db.SaveChanges();
                    
                    MessageBox.Show(_currentLang == "EN" 
                        ? "Booking expired (more than 10 min late)." 
                        : "Ваша бронь сгорела (опоздание более 10 мин). Возьмите обычный талон.");
                    return;
                }
                
                // Если пришел слишком рано (более чем за 10 минут)
                if (now < appTime.AddMinutes(-10))
                {
                     MessageBox.Show(_currentLang == "EN" 
                        ? "Too early! Activation available 10 mins before." 
                        : "Слишком рано! Активация доступна за 10 минут до приема.");
                     return;
                }

                // АКТИВАЦИЯ
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
        
        // ... (Остальной код без изменений) ...

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
                if (ComboBenefits.SelectedValue is int p)
                {
                    priority = p;
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
