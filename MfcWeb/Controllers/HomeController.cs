using Microsoft.AspNetCore.Mvc;
using MfcWeb.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MfcWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly Mfc111Context _db;

        public HomeController(Mfc111Context db)
        {
            _db = db;
        }

        // ГЛАВНАЯ: Каталог услуг
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

        // СТРАНИЦА ЗАПИСИ
        [HttpGet]
        public IActionResult Book(int id)
        {
            var service = _db.Services.Find(id);
            if (service == null) return RedirectToAction("Index");
            return View(service);
        }

        // СОХРАНЕНИЕ ЗАПИСИ
        [HttpPost]
        public IActionResult Book(int serviceId, string fio, string phone, DateTime date, string time)
        {
            // Парсим время из строки "14:30"
            var timeParts = time.Split(':');
            DateTime appointmentTime = date.Date.AddHours(int.Parse(timeParts[0])).AddMinutes(int.Parse(timeParts[1]));

            if (appointmentTime < DateTime.Now)
            {
                ViewBag.Error = "Нельзя записаться в прошлое!";
                var service = _db.Services.Find(serviceId);
                return View(service);
            }

            // Генерируем код (4 цифры)
            string code = new Random().Next(1000, 9999).ToString();

            // Создаем талон
            var s = _db.Services.Find(serviceId);
            string prefix = s.ServiceName.Substring(0, 1).ToUpper();

            var ticket = new Ticket
            {
                ServiceId = serviceId,
                TicketNumber = $"{prefix}-WEB",
                ClientName = fio,
                PhoneNumber = phone,
                AppointmentTime = appointmentTime,
                BookingCode = code,
                Status = "Booked", // Забронировано (ждет активации)
                TimeCreated = DateTime.Now,
                Priority = 2 // Приоритет для онлайн-записи
            };

            _db.Tickets.Add(ticket);
            _db.SaveChanges();

            // Лог
            _db.QueueLogs.Add(new QueueLog { TicketId = ticket.TicketId, EventTime = DateTime.Now, EventType = "OnlineBooking" });
            _db.SaveChanges();

            return View("Success", ticket);
        }
    }
}