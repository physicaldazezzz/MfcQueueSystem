using Microsoft.AspNetCore.Mvc;
using MfcWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace MfcWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly Mfc111Context _db;

        public HomeController(Mfc111Context db)
        {
            _db = db;
        }

        // Главная: Список услуг
        public IActionResult Index(string search = "")
        {
            var query = _db.Services.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.ServiceName.Contains(search));
            }

            var services = query.OrderBy(s => s.ServiceGroup).ThenBy(s => s.ServiceName).ToList();
            return View(services);
        }

        // Страница записи (GET)
        [HttpGet]
        public IActionResult Book(int id)
        {
            var service = _db.Services.Find(id);
            if (service == null) return RedirectToAction("Index");
            return View(service);
        }

        // API для получения слотов
        [HttpGet]
        public IActionResult GetTimeSlots(int serviceId, DateTime date)
        {
            // Генерируем слоты с 9:00 до 18:00, каждые 30 минут
            var slots = new List<object>();
            var start = date.Date.AddHours(9); // 09:00
            var end = date.Date.AddHours(18); // 18:00
            
            // Получаем уже занятые слоты ГЛОБАЛЬНО (вне зависимости от услуги)
            var busySlots = _db.Tickets
                .Where(t => t.AppointmentTime != null && 
                            t.AppointmentTime.Value.Date == date.Date &&
                            (t.Status == "Booked" || t.Status == "Waiting"))
                .Select(t => t.AppointmentTime.Value)
                .ToList();

            // Текущее время для проверки прошедших слотов
            var now = DateTime.Now;

            for (var time = start; time < end; time = time.AddMinutes(30))
            {
                bool isPast = time < now; // Если время уже прошло
                bool isBusy = busySlots.Any(t => t.Hour == time.Hour && t.Minute == time.Minute);

                slots.Add(new {
                    Time = time.ToString("HH:mm"),
                    IsAvailable = !isPast && !isBusy,
                    IsPast = isPast,
                    IsBusy = isBusy
                });
            }

            return Json(slots);
        }

        // Подтверждение записи (POST)
        [HttpPost]
        public IActionResult Book(int serviceId, string fio, string phone, DateTime date, string time)
        {
            var service = _db.Services.Find(serviceId);
            if (service == null) return RedirectToAction("Index");

            // Проверка входных данных
            if (string.IsNullOrEmpty(time))
            {
                 ViewBag.Error = "Пожалуйста, выберите время!";
                 return View(service);
            }

            var timeParts = time.Split(':');
            DateTime appointmentTime = date.Date.AddHours(int.Parse(timeParts[0])).AddMinutes(int.Parse(timeParts[1]));

            if (appointmentTime < DateTime.Now)
            {
                ViewBag.Error = "Запись невозможна в прошлом!";
                return View(service);
            }

            // Финальная проверка на занятость (Double Check) - ГЛОБАЛЬНАЯ
            bool isSlotTaken = _db.Tickets.Any(t => 
                t.AppointmentTime == appointmentTime && 
                (t.Status == "Booked" || t.Status == "Waiting"));

            if (isSlotTaken)
            {
                ViewBag.Error = "К сожалению, это время только что заняли. Выберите другое.";
                return View(service);
            }

            // Создание талона
            string code = new Random().Next(1000, 9999).ToString();
            string prefix = service.ServiceName.Substring(0, 1).ToUpper();

            var ticket = new Ticket
            {
                ServiceId = serviceId,
                TicketNumber = $"{prefix}-WEB",
                ClientName = fio,
                PhoneNumber = phone,
                AppointmentTime = appointmentTime,
                BookingCode = code,
                Status = "Booked",
                TimeCreated = DateTime.Now,
                Priority = 20
            };

            _db.Tickets.Add(ticket);
            _db.SaveChanges();

            _db.QueueLogs.Add(new QueueLog { TicketId = ticket.TicketId, EventTime = DateTime.Now, EventType = "OnlineBooking" });
            _db.SaveChanges();

            return View("Success", ticket);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
