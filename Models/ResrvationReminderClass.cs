//using System;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.EntityFrameworkCore;
//using MvcCoreUploadAndDisplayImage_Demo.Data;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI.Services;

//public class ReservationReminderService : BackgroundService
//{
//    private readonly IServiceProvider _serviceProvider;

//    public ReservationReminderService(IServiceProvider serviceProvider)
//    {
//        _serviceProvider = serviceProvider;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            await CheckForUpcomingViewings(stoppingToken);

//            // Run this task every 15 minutes
//            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
//        }
//    }

//    private async Task CheckForUpcomingViewings(CancellationToken cancellationToken)
//    {
//        using (var scope = _serviceProvider.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>(); // Assuming you have an email service

//            var now = DateTime.Now;
//            var oneHourFromNow = now.AddHours(1);

//            // Fetch reservations that have viewings in the next hour
//            var upcomingReservations = await context.Reservations
//                .Include(r => r.Profile)
//                .Where(r =>
//                    r.ViewingDate.Hours == now.Day &&  // Same day
//                  (r.ReservationDate + r.ViewingDate) >= now && // Viewing has not yet passed
//                   (r.ReservationDate + r.ViewingDate) <= oneHourFromNow) // Viewing time is within the next hour
//                .ToListAsync(cancellationToken);

//            foreach (var reservation in upcomingReservations)
//            {
//                // Send reminder email
//                var email = reservation.Profile.Email;
//                var viewingTime = reservation.ViewingDate + reservation.ViewingDate;
//                var subject = "Reminder: Upcoming Apartment Viewing";
//                var message = $"Dear {reservation.Profile.FirstName},\n\nThis is a reminder for your upcoming apartment viewing at {viewingTime}.\n\nThank you.";

//                await emailService.SendEmailAsync(email, subject, message);
//            }
//        }
//    }
//}
