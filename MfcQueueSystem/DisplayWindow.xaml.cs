using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using MfcQueueSystem.Models;

namespace MfcQueueSystem
{
    public partial class DisplayWindow : Window
    {
        private DispatcherTimer _timer;

        public DisplayWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Tick += (s, e) => UpdateBoard();
            _timer.Start();
            UpdateBoard();
        }

        public class BoardItem
        {
            public string TicketNumber { get; set; }
            public string WindowNumber { get; set; }
            public string ClientName { get; set; }
            public string ServiceName { get; set; }
            public string EstimatedTime { get; set; } // Для очереди (справа)
            public string AvgDuration { get; set; }   // НОВОЕ: Для активного вызова (слева)
        }

        private void UpdateBoard()
        {
            ClockText.Text = DateTime.Now.ToString("HH:mm");

            using (var db = new Mfc111Context())
            {
                // 1. Активные (Кого вызывают)
                var active = db.Tickets
                    .Include(t => t.Window)
                    .Include(t => t.Service) // Обязательно подгружаем услугу!
                    .Where(t => t.Status == "InProgress")
                    .OrderByDescending(t => t.TimeStart)
                    .Take(5)
                    .Select(t => new BoardItem
                    {
                        TicketNumber = t.TicketNumber,
                        WindowNumber = t.Window != null ? t.Window.WindowNumber.ToString() : "-",
                        ClientName = t.ClientName ?? "",
                        // Добавляем среднее время выполнения этой услуги
                        AvgDuration = $"~{t.Service.AvgServiceTimeMin} мин."
                    }).ToList();
                ActiveList.ItemsSource = active;

                // 2. Ожидающие
                var waitingData = db.Tickets
                    .Include(t => t.Service)
                    .Where(t => t.Status == "Waiting")
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.TimeCreated)
                    .Take(10)
                    .ToList();

                var waitingItems = waitingData.Select((t, index) => new BoardItem
                {
                    TicketNumber = t.TicketNumber,
                    ServiceName = t.Service.ServiceName,
                    EstimatedTime = DateTime.Now.AddMinutes((index + 1) * 10).ToString("HH:mm")
                }).ToList();

                WaitingList.ItemsSource = waitingItems;
            }
        }
    }
}