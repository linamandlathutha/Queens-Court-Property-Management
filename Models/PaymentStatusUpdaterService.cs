using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcCoreUploadAndDisplayImage_Demo.Data;
using MvcCoreUploadAndDisplayImage_Demo.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

public class PaymentStatusUpdaterService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentStatusUpdaterService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                // Load SMTP settings from appsettings.json
                var smtpServer = configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = configuration["EmailSettings:SmtpPassword"];
                var fromAddress = configuration["EmailSettings:FromAddress"];

                var now = DateTime.Now;

                // Find overdue records
                var overdueRecords = context.Histories
                    .Where(h => h.DueDate < now && h.PaymentStatus == PaymentStatus.Paid)
                    .ToList();

                foreach (var record in overdueRecords)
                {
                    // Update the status to Overdue
                    record.PaymentStatus = PaymentStatus.Overdue;

                    if (!string.IsNullOrEmpty(record.Email))
                    {
                        var subject = "Rent Payment Overdue Notification";
                        var messageBody = $@"
                        Dear {record.Email},

                        Your rent payment for apartment '{record.ApartmentName}' is now overdue. Please log in to your account and make the payment as soon as possible.

                        Thank you,
                        Queens Property Management";

                        try
                        {
                            // Set up SMTP client
                            using (var client = new SmtpClient(smtpServer, smtpPort))
                            {
                                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                                client.EnableSsl = true;

                                var mailMessage = new MailMessage
                                {
                                    From = new MailAddress(fromAddress),
                                    Subject = subject,
                                    Body = messageBody,
                                    IsBodyHtml = false
                                };
                                mailMessage.To.Add(record.Email);

                                await client.SendMailAsync(mailMessage);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send email to {record.Email}. Error: {ex.Message}");
                        }
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }

            // Run every minute for testing purposes
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }


}
