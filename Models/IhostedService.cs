using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MvcCoreUploadAndDisplayImage_Demo.Data;

//public class ReservationCleanupService : IHostedService, IDisposable
//{
//    private readonly IServiceScopeFactory _scopeFactory;
//    private readonly ILogger<ReservationCleanupService> _logger;
//    private Timer _timer;

//    public ReservationCleanupService(IServiceScopeFactory scopeFactory, ILogger<ReservationCleanupService> logger)
//    {
//        _scopeFactory = scopeFactory;
//        _logger = logger;
//    }

//    public Task StartAsync(CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Reservation Cleanup Service is starting.");

//        // Set up the timer to execute cleanup and reminders periodically (e.g., every 30 minutes)
//        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

//        return Task.CompletedTask;
//    }

//    private async void DoWork(object state)
//    {
//        try
//        {
//            using (var scope = _scopeFactory.CreateScope())
//            {
//                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                var emailService = scope.ServiceProvider.GetRequiredService<IEmailSender>(); // Assuming you're using an email service

//                var now = DateTime.Now;
//                var oneHourFromNow = now.AddHours(1);

//                // ** 1. Cleanup Logic: Remove reservations whose date/time has passed **
//                var expiredReservations = await context.Reservations
//                    .Where(r => (r.ReservationDate + r.ViewingDate) < now)
//                    .ToListAsync();

//                if (expiredReservations.Any())
//                {
//                    context.Reservations.RemoveRange(expiredReservations);
//                    await context.SaveChangesAsync();

//                    _logger.LogInformation($"Cleaned up {expiredReservations.Count} expired reservations.");
//                }

//                // ** 2. Reminder Logic: Send reminder emails for upcoming reservations within the next hour **
//                var reservationsToRemind = await context.Reservations
//                    .Include(r => r.Profile)
//                    .Where(r => (r.ReservationDate + r.ViewingDate) >= now &&
//                                (r.ReservationDate + r.ViewingDate) <= oneHourFromNow)
//                    .ToListAsync();

//                foreach (var reservation in reservationsToRemind)
//                {
//                    var email = reservation.Profile.Email;
//                    var message = $"Reminder: You have a viewing scheduled at {reservation.ReservationDate.ToShortDateString()} {reservation.ViewingDate}.";

//                    await emailService.SendEmailAsync(email, "Viewing Reminder", message);

//                    _logger.LogInformation($"Reminder sent to {email} for viewing at {reservation.ViewingDate}");
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error occurred while cleaning up reservations or sending reminders.");
//        }
//    }

//    public Task StopAsync(CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Reservation Cleanup Service is stopping.");

//        _timer?.Change(Timeout.Infinite, 0);

//        return Task.CompletedTask;
//    }

//    public void Dispose()
//    {
//        _timer?.Dispose();
//    }
//}
