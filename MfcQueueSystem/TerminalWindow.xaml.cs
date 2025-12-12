using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class TerminalWindow : Window
    {
        private DispatcherTimer? _timer;
        private List<Service> _allServices = new List<Service>();

        private string _targetType = "PHYS"; // PHYS или LEGAL
        private string _currentLang = "RU";  // RU или EN
        private int _tempServiceId;

        // ==========================================
        // СЛОВАРЬ ПЕРЕВОДОВ (ВСЕ УСЛУГИ ИЗ БД)
        // ==========================================
        private readonly Dictionary<string, string> _translations = new Dictionary<string, string>
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

        // --- МЕТОД ПЕРЕВОДА ---
        private string Translate(string text)
        {
            if (_currentLang == "RU") return text;
            return _translations.ContainsKey(text) ? _translations[text] : text;
        }

        // --- ЛОГИКА СМЕНЫ ЯЗЫКА ---
        private void LangRu_Click(object sender, RoutedEventArgs e) => SetLanguage("RU");
        private void LangEn_Click(object sender, RoutedEventArgs e) => SetLanguage("EN");

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

                if (TxtMainHeader != null) TxtMainHeader.Text = "ЗАПИСЬ НА ПРИЕМ";
                if (TxtSearchPlaceholder != null) TxtSearchPlaceholder.Text = "🔍 Поиск услуги...";
                if (TxtBackBtn != null) TxtBackBtn.Text = "🡠 Назад";

                if (TxtRegTitle != null) TxtRegTitle.Text = "Регистрация в очереди";
                if (TxtRegNameLabel != null) TxtRegNameLabel.Text = "Введите ваше ФИО:";
                if (TxtRegCatLabel != null) TxtRegCatLabel.Text = "Категория:";
                if (CheckBenefit != null) CheckBenefit.Content = "У меня есть льготы";
                if (BtnRegCancel != null) BtnRegCancel.Content = "Отмена";
                if (BtnRegConfirm != null) BtnRegConfirm.Content = "ПОЛУЧИТЬ ТАЛОН";

                if (TxtTicketTitle != null) TxtTicketTitle.Text = "ВАШ ТАЛОН";
                if (TxtTicketAhead != null) TxtTicketAhead.Text = "Перед вами:";
                if (BtnTicketClose != null) BtnTicketClose.Content = "Забрать талон";
            }
            else // ENGLISH
            {
                if (TxtWhoAreYou != null) TxtWhoAreYou.Text = "Who are you?";
                if (TxtPhysTitle != null) TxtPhysTitle.Text = "Individual";
                if (TxtPhysDesc != null) TxtPhysDesc.Text = "Personal documents";
                if (TxtLegalTitle != null) TxtLegalTitle.Text = "Legal Entity / Business";
                if (TxtLegalDesc != null) TxtLegalDesc.Text = "Business, taxes, IP";

                if (TxtMainHeader != null) TxtMainHeader.Text = "APPOINTMENT";
                if (TxtSearchPlaceholder != null) TxtSearchPlaceholder.Text = "🔍 Search service...";
                if (TxtBackBtn != null) TxtBackBtn.Text = "🡠 Back";

                if (TxtRegTitle != null) TxtRegTitle.Text = "Registration";
                if (TxtRegNameLabel != null) TxtRegNameLabel.Text = "Enter Full Name:";
                if (TxtRegCatLabel != null) TxtRegCatLabel.Text = "Category:";
                if (CheckBenefit != null) CheckBenefit.Content = "I have benefits (Priority)";
                if (BtnRegCancel != null) BtnRegCancel.Content = "Cancel";
                if (BtnRegConfirm != null) BtnRegConfirm.Content = "GET TICKET";

                if (TxtTicketTitle != null) TxtTicketTitle.Text = "YOUR TICKET";
                if (TxtTicketAhead != null) TxtTicketAhead.Text = "People ahead:";
                if (BtnTicketClose != null) BtnTicketClose.Content = "Take Ticket";
            }

            if (MainContentGrid.Visibility == Visibility.Visible) UpdateCategories();
        }

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

        // --- УМНАЯ ФИЛЬТРАЦИЯ С ПЕРЕВОДОМ ---
        private void UpdateCategories()
        {
            var relevantServices = _allServices.Where(s => s.TargetType == _targetType || s.TargetType == "BOTH");

            // Получаем уникальные категории, сразу переводя их
            var cats = relevantServices
                .Select(s => Translate(s.ServiceGroup))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

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
                // Поиск делаем умным: ищем и в русском названии, и в английском переводе
                string q = searchText.ToLower();
                filtered = filtered.Where(s => s.ServiceName.ToLower().Contains(q) || Translate(s.ServiceName).ToLower().Contains(q));
            }

            if (category != null)
            {
                // Сравниваем переведенную категорию из базы с тем, что выбрал пользователь
                filtered = filtered.Where(s => Translate(s.ServiceGroup) == category);
            }

            // Создаем проекцию для отображения (чтобы кнопки показали переведенный текст)
            var displayList = filtered.Select(s => new {
                s.ServiceId,
                ServiceName = Translate(s.ServiceName), // <-- ПЕРЕВОД ПРИМЕНЯЕТСЯ ТУТ
                s.ServiceGroup
            }).ToList();

            ServicesList.ItemsSource = displayList;
        }

        // --- ПОЛУЧЕНИЕ ТАЛОНА ---
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
            if (string.IsNullOrEmpty(fio))
            {
                MessageBox.Show(_currentLang == "EN" ? "Please enter name" : "Введите ФИО");
                return;
            }

            using (var db = new Mfc111Context())
            {
                var service = db.Services.Find(_tempServiceId);
                if (service == null) return;

                string prefix = service.ServiceName.Substring(0, 1).ToUpper();
                int countToday = db.Tickets.Count(t => t.TimeCreated.Date == DateTime.Today) + 1;
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

                int ahead = db.Tickets.Count(x => x.Status == "Waiting" && x.TicketId != t.TicketId &&
                                             (x.Priority > t.Priority || (x.Priority == t.Priority && x.TimeCreated < t.TimeCreated)));

                // Заполняем чек (используем перевод названия услуги)
                TicketNumberText.Text = ticketNum;
                TicketServiceText.Text = Translate(service.ServiceName); // <-- ПЕРЕВОД
                TicketInfoText.Text = fio;
                TicketPriorityText.Visibility = isVip ? Visibility.Visible : Visibility.Collapsed;
                if (isVip) TicketPriorityText.Text = _currentLang == "EN" ? "★ PRIORITY" : "★ ЛЬГОТНАЯ ОЧЕРЕДЬ";

                string unit = _currentLang == "EN" ? "pers." : "чел.";
                TicketCountText.Text = $"{ahead} {unit}";
                TicketTimeText.Text = DateTime.Now.ToString("dd.MM HH:mm");

                RegistrationPopup.Visibility = Visibility.Collapsed;
                TicketPopup.Visibility = Visibility.Visible;

                Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    PrintDialog pd = new PrintDialog();
                    if (pd.ShowDialog() == true) pd.PrintVisual(TicketCard, ticketNum);
                }));
            }
        }
    }
}