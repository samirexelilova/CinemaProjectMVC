using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using StreamitMVC.DAL;
using StreamitMVC.Models;

namespace StreamitMVC.Services
{
    public class ReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderService> _logger;
        private readonly IConfiguration _configuration;


            public ReminderService(IServiceScopeFactory scopeFactory, ILogger<ReminderService> logger, IConfiguration configuration)
            {
                _scopeFactory = scopeFactory;
                _logger = logger;
                _configuration = configuration;
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var now = DateTime.Now;
                    var oneHourLater = now.AddHours(1);

                    var upcomingTickets = await context.Tickets
                        .Include(t => t.Session)
                            .ThenInclude(s => s.Movie)
                        .Include(t => t.Booking)
                            .ThenInclude(b => b.User)
                        .Where(t => !t.Session.IsDeleted &&
                                    !t.Booking.IsDeleted &&
                                    t.Session.StartTime > now &&
                                    t.Session.StartTime <= oneHourLater &&
                                    !t.IsReminderSent)
                        .ToListAsync();

                    foreach (var ticket in upcomingTickets)
                    {
                        var user = ticket.Booking.User;
                        if (user != null)
                        {
                            await SendReminderEmailAsync(user, ticket.Session);
                            ticket.IsReminderSent = true;
                        }
                    }

                    await context.SaveChangesAsync();

                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            private async Task SendReminderEmailAsync(AppUser user, Session session)
            {
                var smtpHost = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:Port");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];
                var smtpFrom = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? "Cinema", smtpFrom));
                message.To.Add(new MailboxAddress(user.UserName ?? "Customer", user.Email));
                message.Subject = "🎬 Kino Xatırlatma: Seans 1 saata başlayacaq!";

                var body = new BodyBuilder();
                body.HtmlBody = $@"
            <h3>Salam {user.Name ?? user.Surname}!</h3>
            <p>Sizin <strong>{session.Movie.Name}</strong> filmi üçün aldığınız biletin seansı 1 saata başlayacaq.</p>
            <p><strong>Başlama vaxtı:</strong> {session.StartTime:dd MMMM yyyy, HH:mm}</p>
            <p>Zəhmət olmasa vaxtında zalda olun!</p>
            <br/>
            <p>Təşəkkür edirik!</p>";

                message.Body = body.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

    }
