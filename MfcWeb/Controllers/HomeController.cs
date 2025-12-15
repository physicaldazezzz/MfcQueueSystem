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

        // �������: ������� �����
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

        // �������� ������
        [HttpGet]
        public IActionResult Book(int id)
        {
            var service = _db.Services.Find(id);
            if (service == null) return RedirectToAction("Index");
            return View(service);
        }

        // ���������� ������
        [HttpPost]
        public IActionResult Book(int serviceId, string fio, string phone, DateTime date, string time)
        {
            // ������ ����� �� ������ "14:30"
            var timeParts = time.Split(':');
            DateTime appointmentTime = date.Date.AddHours(int.Parse(timeParts[0])).AddMinutes(int.Parse(timeParts[1]));

            if (appointmentTime < DateTime.Now)
            {
                ViewBag.Error = "������ ���������� � �������!";
                var service = _db.Services.Find(serviceId);
                return View(service);
            }

            // ���������� ��� (4 �����)
            string code = new Random().Next(1000, 9999).ToString();

            // ������� �����
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
                Status = "Booked", // ������������� (���� ���������)
                TimeCreated = DateTime.Now,
                Priority = 2 // ��������� ��� ������-������
            };

            _db.Tickets.Add(ticket);
            _db.SaveChanges();

            // ���
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