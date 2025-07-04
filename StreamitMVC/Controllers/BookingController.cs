using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.Services.Interfaces;
using StreamitMVC.ViewModels;
using Stripe.Checkout;
using MimeKit;
using MailKit.Net.Smtp;
using QuestPDF.Helpers;
using QuestPDF.Fluent;
using System.Drawing;
using System.Drawing.Imaging;
using QRCoder;
using System.Linq;
using Stripe;
using StreamitMVC.Utilities.Enums;

namespace StreamitMVC.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IPricingService _pricingService;

        public BookingController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager, IConfiguration configuration, IPricingService pricingService)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _configuration = configuration;
            _pricingService = pricingService;
        }
        private async Task<byte[]> GenerateTicketPdfAsync(Ticket ticket)
        {
            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == ticket.SessionId);

            var seat = await _context.Seats
                .FirstOrDefaultAsync(s => s.Id == ticket.SeatId);

            if (session == null || seat == null)
            {
                throw new Exception("Sessiya və ya oturacaq tapılmadı.");
            }

            byte[] qrCodeImageBytes = null;

            try
            {
                string qrText = $"TicketId:{ticket.Id};Session:{ticket.SessionId};Seat:{seat.RowNumber}-{seat.SeatNumber};Movie:{session.Movie.Name};Date:{session.StartTime:yyyy-MM-dd-HH-mm}";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                qrCodeImageBytes = qrCode.GetGraphic(12);

                Console.WriteLine($"QR kod uğurla yaradıldı. Ölçü: {qrCodeImageBytes.Length} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QR kod yaratma xətası: {ex.Message}");
                qrCodeImageBytes = null;
            }
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(16));

                    page.Content().PaddingTop(40).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().AlignLeft().Text("🎬 KİNO BİLETİ")
                            .Bold().FontSize(28).FontColor(Colors.Blue.Medium);

                        col.Item().LineHorizontal(2).LineColor(Colors.Grey.Medium);

                        col.Item().AlignLeft().Text(text =>
                        {
                            text.Span("🎞️ Film: ").Bold();
                            text.Span(session.Movie.Name).FontSize(20);
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("📅 Tarix və Vaxt: ").Bold();
                            text.Span(session.StartTime.ToString("dd MMMM yyyy, HH:mm"));
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("🏛️ Zal: ").Bold();
                            text.Span(session.Hall.Name);
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("💺 Oturacaq: ").Bold();
                            text.Span($"Sıra {seat.RowNumber} - Nömrə {seat.SeatNumber}");
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("🎟️ Bilet №: ").Bold();
                            text.Span(ticket.Id.ToString());
                        });

                        col.Item().Text(text =>
                        {
                            text.Span("💰 Qiymət: ").Bold();
                            text.Span($"{ticket.Price:C}");
                        });

                        col.Item().PaddingTop(80);

                        col.Item().AlignRight().Column(inner =>
                        {
                            inner.Spacing(5);

                            if (qrCodeImageBytes != null && qrCodeImageBytes.Length > 0)
                            {
                                inner.Item().Text(" QR Kod:").Bold().FontSize(12);
                                inner.Item().AlignBottom().AlignCenter().Container().Width(100).Height(100).Image(qrCodeImageBytes);

                                inner.Item().AlignCenter().Text("Zalda bu kodu təqdim edin").FontSize(10).FontColor(Colors.Grey.Medium);

                            }
                            else
                            {
                                inner.Item().Text("⚠️ QR kod yaradıla bilmədi")
                                    .FontColor(Colors.Red.Medium);
                            }
                        });
                    });
                });
            });

            using var msDoc = new MemoryStream();
            document.GeneratePdf(msDoc);
            return msDoc.ToArray();
        }


        private async Task SendTicketEmailAsync(AppUser user, Ticket ticket, byte[] pdfBytes)
        {
            try
            {
                Console.WriteLine($"Email göndərmə başlayır: {user.Email}");

                var smtpHost = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration.GetValue<int>("EmailSettings:Port");
                var smtpUser = _configuration["EmailSettings:Username"];
                var smtpPass = _configuration["EmailSettings:Password"];
                var smtpFrom = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
                {
                    Console.WriteLine("Email konfiqurasiyası tam deyil!");
                    return;
                }

                Console.WriteLine($"SMTP konfiqurasiyası: {smtpHost}:{smtpPort}");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName ?? "Cinema", smtpFrom));

                message.To.Add(new MailboxAddress(user.UserName ?? "Customer", user.Email));
                message.Subject = "Biletiniz - Kino sistem";

                var body = new BodyBuilder();
                body.HtmlBody = @"
            <h2>Salam!</h2>
            <p>Biletiniz elektron formada əlavə edilmişdir.</p>
            <p>Zəhmət olmasa zalda bu PDF-dəki QR kodu təqdim edin.</p>
            <p>Təşəkkürlər!</p>";

                if (pdfBytes != null && pdfBytes.Length > 0)
                {
                    body.Attachments.Add($"Bilet-{ticket.Id}.pdf", pdfBytes, new ContentType("application", "pdf"));
                    Console.WriteLine($"PDF attachment əlavə edildi: {pdfBytes.Length} bytes");
                }
                else
                {
                    Console.WriteLine("PDF bytes boş və ya null!");
                    return;
                }

                message.Body = body.ToMessageBody();

                using var client = new SmtpClient();

                try
                {
                    Console.WriteLine("SMTP serverə qoşulma...");
                    await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                    Console.WriteLine("Autentifikasiya...");
                    await client.AuthenticateAsync(smtpUser, smtpPass);

                    Console.WriteLine("Email göndərilir...");
                    await client.SendAsync(message);

                    Console.WriteLine("Email uğurla göndərildi!");
                }
                catch (Exception smtpEx)
                {
                    Console.WriteLine($"SMTP xətası: {smtpEx.Message}");
                    throw;
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email göndərmə xətası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw;
            }
        }


        public async Task<IActionResult> SelectSeats(int sessionId, string error = null)
        {

            var session = await _context.Sessions
            .Include(s => s.Movie)
            .Include(s => s.Hall)
                .ThenInclude(h => h.Seats)
                    .ThenInclude(seat => seat.SeatType)
            .Include(s => s.Hall)
                .ThenInclude(h => h.HallType)
                    .ThenInclude(ht => ht.HallPrices)
            .Include(s => s.Cinema)
            .Include(s => s.HallPrice)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
            if (session == null)
                return NotFound("Seans tapılmadı");

            var currentTime = DateTime.Now;
            var oneWeekLater = currentTime.AddDays(7);

            if (session.StartTime < currentTime)
            {
                TempData["Error"] = "Bu seans artıq başlayıb və ya bitib";
                return RedirectToAction("Index", "Home");
            }

            if (session.StartTime > oneWeekLater)
            {
                TempData["Error"] = "Bu seans çox uzaq gələcəkdədir";
                return RedirectToAction("Index", "Home");
            }

            if (session.StartTime <= currentTime.AddMinutes(30))
            {
                TempData["Error"] = "Bu seansa bilet satışı başa çatıb";
                return RedirectToAction("Index", "Home");
            }

            var takenSeatIds = await GetTakenSeatsForSession(sessionId);

            var model = new SelectSeatsVM
            {
                SessionId = session.Id,
                HallName = session.Hall.Name,
                Rows = session.Hall.Rows,
                SeatsPerRow = session.Hall.SeatsPerRow,
                Movie = session.Movie,
                Session = session,
                Seats = session.Hall.Seats.Select(seat => new SeatViewModel
                {
                    Id = seat.Id,
                    Row = seat.RowNumber,
                    Number = seat.SeatNumber,
                    Type = seat.SeatType.Name,
                    Color = seat.SeatType.Color,
                    IsTaken = takenSeatIds.Contains(seat.Id),
                    IsSold = false
                }).OrderBy(s => s.Row).ThenBy(s => s.Number).ToList(),
                ErrorMessage = error
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSeats(int sessionId, List<int> selectedSeatIds)
        {
            if (selectedSeatIds == null || !selectedSeatIds.Any())
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Zəhmət olmasa ən azı bir oturacaq seçin" });
            }

            if (selectedSeatIds.Count > 6)
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Maksimum 6 oturacaq seçə bilərsiniz" });
            }

            var session = await _context.Sessions
         .Include(s => s.Hall)
             .ThenInclude(h => h.HallType)
                 .ThenInclude(ht => ht.HallPrices)
         .Include(s => s.HallPrice)
         .Include(s => s.Movie)
         .Include(s => s.Cinema)
         .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                return RedirectToAction("Index", "Home", new { error = "Seans tapılmadı" });
            }

            var currentTime = DateTime.Now;
            if (session.StartTime <= currentTime)
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Bu seans artıq başlayıb. Bilet almaq mümkün deyil." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("SelectSeats", new { sessionId }) });
            }

            var takenSeatIds = await GetTakenSeatsForSession(sessionId);
            var conflictingSeats = selectedSeatIds.Intersect(takenSeatIds).ToList();


            if (conflictingSeats.Any())
            {
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var basket = await GetOrCreateUserBasket(user.Id);

                decimal sessionPrice = _pricingService.CalculateSessionPrice(session);

                var booking = new Booking
                {
                    UserId = user.Id,
                    SessionId = sessionId,
                    Status = BookingStatus.Reserved,
                    BookingDate = DateTime.Now,
                    TotalAmount = selectedSeatIds.Count * sessionPrice
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var ticketsToAdd = new List<Ticket>();
                var basketItemsToAdd = new List<BasketItem>();

                decimal seatPrice = session.HallPrice.Price;

                foreach (var seatId in selectedSeatIds)
                {
                    var seatTaken = await _context.Tickets.AnyAsync(t =>
                        t.SeatId == seatId &&
                        t.SessionId == sessionId &&
                        (t.Status == TicketStatus.Sold ||
                         (t.Status == TicketStatus.Reserved &&
                          t.PurchasedAt.AddMinutes(15) > DateTime.Now)));

                    if (seatTaken)
                    {
                        await transaction.RollbackAsync();
                        return RedirectToAction("SelectSeats",
                            new { sessionId, error = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib" });
                    }

                    var ticket = new Ticket
                    {
                        BookingId = booking.Id,
                        SeatId = seatId,
                        SessionId = sessionId,
                        Price = sessionPrice,
                        Status = TicketStatus.Reserved,
                        PurchasedAt = DateTime.Now
                    };
                    ticketsToAdd.Add(ticket);

                    var basketItem = new BasketItem
                    {
                        BasketId = basket.Id,
                        SessionId = sessionId,
                        SeatId = seatId,
                        Price = sessionPrice
                    };
                    basketItemsToAdd.Add(basketItem);
                }

                _context.Tickets.AddRange(ticketsToAdd);
                _context.BasketItems.AddRange(basketItemsToAdd);
                await _context.SaveChangesAsync();

                basket.TotalPrice = await _context.BasketItems
                    .Where(bi => bi.BasketId == basket.Id)
                    .SumAsync(bi => bi.Price);

                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"{selectedSeatIds.Count} oturacaq səbətə əlavə edildi. " +
                                    "15 dəqiqə ərzində ödəniş etməzsəniz rezerv ləğv olunacaq.";

                return RedirectToAction("Checkout");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return RedirectToAction("SelectSeats",
                    new { sessionId, error = "Xəta baş verdi. Yenidən cəhd edin." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await CleanExpiredReservations(user.Id);

            var basket = await _context.Baskets
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Cinema)
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Seat)
                        .ThenInclude(s => s.SeatType)
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Movie) 
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || basket.Items == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbətiniz boşdur.";
                return RedirectToAction("Index", "Home");
            }
            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Movie) 
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Cinema)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
                return BadRequest("Səbət boşdur");

            var domain = $"{Request.Scheme}://{Request.Host}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = basket.Items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "azn",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Type == BasketItemType.DirectMoviePurchase ?
                                   $"Film: {item.Movie.Name}" :
                                   item.Session.Movie.Name,
                            Description = item.Type == BasketItemType.DirectMoviePurchase ?
                                        "Filmi evdə izləmək üçün" :
                                        $"Kinoteatr: {item.Session.Cinema.Name}, Zal: {item.Session.Hall.Name}, Yer: {item.Seat.RowNumber}-{item.Seat.SeatNumber}, Vaxt: {item.Session.StartTime:dd.MM.yyyy HH:mm}"
                        }
                    },
                    Quantity = 1,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + Url.Action("PaymentSuccess", "Booking") + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + Url.Action("PaymentCancel", "Booking"),
                Metadata = new Dictionary<string, string>
        {
            { "userId", user.Id },
            { "basketId", basket.Id.ToString() }
        }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Json(new { id = session.Id });
        }
        public async Task<IActionResult> PaymentSuccess(string session_id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(session_id))
            {
                TempData["Error"] = "Ödəniş sessiyası tapılmadı.";
                return RedirectToAction("Checkout");
            }

            var service = new SessionService();
            Stripe.Checkout.Session session;

            try
            {
                session = service.Get(session_id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ödəniş sessiyası alınmadı: {ex.Message}";
                return RedirectToAction("Checkout");
            }

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] = "Ödəniş tamamlanmayıb.";
                return RedirectToAction("Checkout");
            }

            string userId = session.Metadata.GetValueOrDefault("userId");
            string basketIdStr = session.Metadata.GetValueOrDefault("basketId");

            if (userId == null || basketIdStr == null || user.Id != userId)
            {
                TempData["Error"] = "Ödəniş məlumatları uyğun gəlmir.";
                return RedirectToAction("Checkout");
            }

            int basketId = int.Parse(basketIdStr);

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .ThenInclude(i => i.Session)
                .FirstOrDefaultAsync(b => b.Id == basketId && b.UserId == userId);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbət boşdur.";
                return RedirectToAction("Checkout");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var processedTicketIds = new List<int>();
            var processedMoviePurchaseIds = new List<int>();
            var processedMovieIds = new List<int>();

            try
            {
                var sessionTickets = basket.Items.Where(i => i.Type == BasketItemType.SessionTicket).ToList();
                var directPurchases = basket.Items.Where(i => i.Type == BasketItemType.DirectMoviePurchase).ToList();

                if (sessionTickets.Any())
                {
                    var sessionGroups = sessionTickets.GroupBy(i => i.SessionId);

                    foreach (var group in sessionGroups)
                    {
                        Booking booking = null;

                        var existingBooking = await _context.Bookings
                            .FirstOrDefaultAsync(b => b.UserId == user.Id && b.SessionId == group.Key && b.Status == BookingStatus.Reserved);

                        if (existingBooking == null)
                        {
                            booking = new Booking
                            {
                                UserId = user.Id,
                                SessionId = group.Key.Value,
                                BookingDate = DateTime.Now,
                                TotalAmount = group.Sum(i => i.Price),
                                Status = BookingStatus.Paid
                            };

                            _context.Bookings.Add(booking);
                            await _context.SaveChangesAsync(); 

                            booking = await _context.Bookings.FindAsync(booking.Id);
                        }
                        else
                        {
                            existingBooking.Status = BookingStatus.Paid;
                            existingBooking.BookingDate = DateTime.Now;
                            existingBooking.TotalAmount = group.Sum(i => i.Price);

                            _context.Bookings.Update(existingBooking);
                            await _context.SaveChangesAsync();

                            booking = existingBooking;
                        }

                        if (booking?.Id <= 0)
                        {
                            throw new Exception("Booking ID əldə edilmədi");
                        }

                        var payment = new Payment
                        {
                            Method = Extensions.Enums.PaymentMethod.Stripe,
                            Amount = booking.TotalAmount,
                            PaidAt = DateTime.Now,
                            BookingId = booking.Id, 
                            Status = PaymentStatus.Completed
                        };

                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();

                        foreach (var item in group)
                        {
                            var ticket = await _context.Tickets
                                .FirstOrDefaultAsync(t => t.SeatId == item.SeatId && t.SessionId == item.SessionId && t.Status == TicketStatus.Reserved);

                            if (ticket == null)
                            {
                                ticket = new Ticket
                                {
                                    BookingId = booking.Id, 
                                    SeatId = item.SeatId.Value,
                                    SessionId = item.SessionId.Value,
                                    Price = item.Price,
                                    Status = TicketStatus.Sold,
                                    PurchasedAt = DateTime.Now
                                };
                                _context.Tickets.Add(ticket);
                            }
                            else
                            {
                                ticket.Status = TicketStatus.Sold;
                                ticket.BookingId = booking.Id; 
                                ticket.PurchasedAt = DateTime.Now;
                                _context.Tickets.Update(ticket);
                            }

                            var seat = await _context.Seats.FirstOrDefaultAsync(s => s.Id == item.SeatId);
                            var soldSeatType = await _context.SeatTypes.FirstOrDefaultAsync(st => st.Name == "Sold");
                            if (seat != null && soldSeatType != null)
                            {
                                seat.SeatTypeId = soldSeatType.Id;
                                _context.Seats.Update(seat);
                            }

                            if (item.Session?.MovieId != null)
                                processedMovieIds.Add(item.Session.MovieId);
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                foreach (var item in directPurchases)
                {
                    var moviePurchase = new MoviePurchase
                    {
                        UserId = user.Id,
                        MovieId = item.MovieId.Value,
                        Price = item.Price,
                        PurchaseDate = DateTime.Now,
                        ExpiresAt = DateTime.Now.AddDays(365),
                        Status = MoviePurchaseStatus.Active
                    };

                    _context.MoviePurchases.Add(moviePurchase);
                    await _context.SaveChangesAsync();

                    if (moviePurchase?.Id <= 0)
                    {
                        throw new Exception("MoviePurchase ID əldə edilmədi");
                    }

                    var moviePayment = new Payment
                    {
                        Method = Extensions.Enums.PaymentMethod.Stripe,
                        Amount = item.Price,
                        PaidAt = DateTime.Now,
                        Status = PaymentStatus.Completed,
                        MoviePurchaseId = moviePurchase.Id, 
                        BookingId = null, 
                        FailureReason = null
                    };

                    _context.Payments.Add(moviePayment);
                    await _context.SaveChangesAsync();

                    processedMoviePurchaseIds.Add(moviePurchase.Id);
                }

                foreach (var movieId in processedMovieIds.Distinct())
                {
                    var stat = await _context.MovieStats.FirstOrDefaultAsync(ms => ms.MovieId == movieId);
                    if (stat == null)
                    {
                        _context.MovieStats.Add(new MovieStats { MovieId = movieId, ViewCount = 1 });
                    }
                    else
                    {
                        stat.ViewCount += 1;
                        _context.MovieStats.Update(stat);
                    }
                }
                var basketItems = await _context.BasketItems.Where(bi => bi.BasketId == basket.Id).ToListAsync();
                _context.BasketItems.RemoveRange(basketItems);

                basket.TotalPrice = 0;
                _context.Baskets.Update(basket);

                await _context.SaveChangesAsync();

                if (sessionTickets.Any())
                {
                    foreach (var group in sessionTickets.GroupBy(i => i.SessionId))
                    {
                        var latestBooking = await _context.Bookings
                            .Include(b => b.Tickets)
                            .FirstOrDefaultAsync(b => b.UserId == user.Id && b.SessionId == group.Key && b.Status == BookingStatus.Paid);

                        if (latestBooking != null)
                        {
                            processedTicketIds.AddRange(latestBooking.Tickets.Select(t => t.Id));
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                    errorMessage += " | Inner Exception: " + ex.InnerException.Message;

                TempData["Error"] = $"Ödəniş zamanı xəta baş verdi: {errorMessage}";
                return RedirectToAction("Checkout");
            }

            if (processedTicketIds.Any())
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Seat)
                    .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                    .Where(t => processedTicketIds.Contains(t.Id))
                    .ToListAsync();

                foreach (var ticket in tickets)
                {
                    var pdf = await GenerateTicketPdfAsync(ticket);
                    await SendTicketEmailAsync(user, ticket, pdf);
                }
            }

            var messages = new List<string>();
            if (processedTicketIds.Any()) messages.Add($"{processedTicketIds.Count} bilet");
            if (processedMoviePurchaseIds.Any()) messages.Add($"{processedMoviePurchaseIds.Count} film alışı");

            TempData["Success"] = $"Ödəniş uğurla tamamlandı! {string.Join(", ", messages)}.";

            if (processedTicketIds.Any())
            {
                return RedirectToAction("MyTickets", "Account");
            }
            else if (processedMoviePurchaseIds.Any())
            {
                return RedirectToAction("MyPurchases", "Account");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult PaymentCancel()
        {
            TempData["Warning"] = "Ödəniş ləğv edildi. Rezervasiyanız səbətdə qalır.";
            return RedirectToAction("Checkout");
        }

     

        [HttpPost]
        public async Task<IActionResult> RemoveFromBasket(int basketItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });
            }

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == basketItemId && bi.Basket.UserId == user.Id);

            if (basketItem == null)
            {
                return Json(new { success = false, message = "Element tapılmadı" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var basket = basketItem.Basket;

                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.SeatId == basketItem.SeatId &&
                                            t.SessionId == basketItem.SessionId &&
                                            t.Status == TicketStatus.Reserved);

                if (ticket != null)
                {
                    _context.Tickets.Remove(ticket);
                }

                _context.BasketItems.Remove(basketItem);

                basket.TotalPrice = await _context.BasketItems
                    .Where(bi => bi.BasketId == basket.Id && bi.Id != basketItemId)
                    .SumAsync(bi => (decimal?)bi.Price) ?? 0;

                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Element səbətdən silindi və oturacaq sərbəst buraxıldı" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Xəta baş verdi" });
            }
        }

        private async Task<List<int>> GetTakenSeatsForSession(int sessionId)
        {
            var currentTime = DateTime.Now;

            return await _context.Tickets
                .Where(t => t.SessionId == sessionId &&
                           (t.Status == TicketStatus.Sold ||
                            (t.Status == TicketStatus.Reserved &&
                             t.PurchasedAt.AddMinutes(15) > currentTime)))
                .Select(t => t.SeatId)
                .ToListAsync();
        }

        private async Task<Basket> GetOrCreateUserBasket(string userId)
        {
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,
                    Items = new List<BasketItem>(),
                    TotalPrice = 0
                };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            return basket;
        }

        private async Task CleanExpiredReservations(string userId)
        {
            var expiredTime = DateTime.Now.AddMinutes(-15);

            var expiredTickets = await _context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.Booking.UserId == userId &&
                           t.Status == TicketStatus.Reserved &&
                           t.PurchasedAt < expiredTime)
                .ToListAsync();

            if (expiredTickets.Any())
            {
                _context.Tickets.RemoveRange(expiredTickets);

                var sessionSeatPairs = expiredTickets.Select(t => new { t.SessionId, t.SeatId }).ToList();

                foreach (var pair in sessionSeatPairs)
                {
                    var basketItem = await _context.BasketItems
                        .FirstOrDefaultAsync(bi => bi.SessionId == pair.SessionId &&
                                                 bi.SeatId == pair.SeatId &&
                                                 bi.Basket.UserId == userId);

                    if (basketItem != null)
                    {
                        _context.BasketItems.Remove(basketItem);
                    }
                }

                var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == userId);
                if (basket != null)
                {
                    basket.TotalPrice = await _context.BasketItems
                        .Where(bi => bi.BasketId == basket.Id)
                        .SumAsync(bi => (decimal?)bi.Price) ?? 0;
                    _context.Baskets.Update(basket);
                }

                await _context.SaveChangesAsync();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDirectPurchaseToBasket(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null || !movie.IsAvailableForDirectPurchase)
            {
                TempData["Error"] = "Bu film birbaşa satış üçün mövcud deyil.";
                return RedirectToAction("Details", new { id = movieId });
            }

            var existingPurchase = await _context.MoviePurchases
                .AnyAsync(mp => mp.UserId == user.Id &&
                               mp.MovieId == movieId &&
                               mp.Status == MoviePurchaseStatus.Active &&
                               (mp.ExpiresAt == null || mp.ExpiresAt > DateTime.Now));

            if (existingPurchase)
            {
                TempData["Error"] = "Bu filmi artıq almısınız.";
                return RedirectToAction("Details", new { id = movieId });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = user.Id,
                    TotalPrice = 0
                };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            var existingItem = basket.Items.FirstOrDefault(i => i.MovieId == movieId && i.Type == BasketItemType.DirectMoviePurchase);
            if (existingItem != null)
            {
                TempData["Error"] = "Bu film artıq səbətinizdə var.";
                return RedirectToAction("Details", new { id = movieId });
            }

            var basketItem = new BasketItem
            {
                BasketId = basket.Id,
                MovieId = movieId,
                Price = movie.DirectPurchasePrice ?? 0,
                Type = BasketItemType.DirectMoviePurchase
            };

            _context.BasketItems.Add(basketItem);
            basket.TotalPrice += basketItem.Price;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Film səbətə əlavə edildi.";
            return RedirectToAction("Checkout", "Booking");
        }


      


    }
}
