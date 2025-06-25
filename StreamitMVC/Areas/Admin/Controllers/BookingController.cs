using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Areas.Admin.Controllers
{
    [Area("admin")]

    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;

        public BookingController(AppDbContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }
        public async Task<IActionResult> SelectSeats(int sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Seats)
                        .ThenInclude(seat => seat.SeatType)
                .Include(s => s.Cinema)
                .Include(s => s.HallPrice)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
                return NotFound();

            var takenSeatIds = session.Tickets
                .Where(t => t.Status == TicketStatus.Reserved || t.Status == TicketStatus.Sold)
                .Select(t => t.SeatId)
                .ToList();

            var model = new SelectSeatsVM
            {
                SessionId = session.Id,
                HallName = session.Hall.Name,
                Rows = session.Hall.Rows,
                SeatsPerRow = session.Hall.SeatsPerRow,
                Movie = session.Movie,
                Session = session,
                Seats = session.Hall.Seats
                    .Select(seat => new SeatViewModel
                    {
                        Id = seat.Id,
                        Row = seat.RowNumber,
                        Number = seat.SeatNumber,
                        Type = seat.SeatType.Name,
                        Color = seat.SeatType.Color,
                        IsTaken = takenSeatIds.Contains(seat.Id)
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSeats(int sessionId, List<int> SelectedSeatIds)
        {
            if (SelectedSeatIds == null || !SelectedSeatIds.Any())
            {
                TempData["Error"] = "Zəhmət olmasa en azı bir oturacaq seçin";
                return RedirectToAction("SelectSeats", new { sessionId });
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.HallPrice)
                .Include(s => s.Movie)
                .Include(s => s.Tickets)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                TempData["Error"] = "Seans tapılmadı";
                return RedirectToAction("Index", "Home");
            }

            var selectedSeats = await _context.Seats
                .Where(s => SelectedSeatIds.Contains(s.Id))
                .ToListAsync();

            var alreadyTakenSeats = session.Tickets
                .Where(t => SelectedSeatIds.Contains(t.SeatId) &&
                           (t.Status == TicketStatus.Reserved || t.Status == TicketStatus.Sold))
                .Select(t => t.SeatId)
                .ToList();

            if (alreadyTakenSeats.Any())
            {
                TempData["Error"] = "Seçdiyiniz oturacaqlardan bəziləri artıq rezerv edilib";
                return RedirectToAction("SelectSeats", new { sessionId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Bilet rezerv etmək üçün login olmalısınız";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = user.Id,
                    Items = new List<BasketItem>(),
                    TotalPrice = 0
                };
                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            decimal seatPrice = session.HallPrice.Price;
            int addedSeatsCount = 0;

            foreach (var seatId in SelectedSeatIds)
            {
                var existingItem = await _context.BasketItems.AnyAsync(i =>
                    i.BasketId == basket.Id && i.SessionId == sessionId && i.SeatId == seatId);
                if (!existingItem) 
                {
                    var basketItem = new BasketItem
                    {
                        BasketId = basket.Id,
                        SessionId = sessionId,
                        SeatId = seatId,
                        Price = seatPrice
                    };

                    _context.BasketItems.Add(basketItem);
                    addedSeatsCount++;
                }
            }

            if (addedSeatsCount > 0)
            {
                await _context.SaveChangesAsync();

                basket.TotalPrice = await _context.BasketItems
                    .Where(bi => bi.BasketId == basket.Id)
                    .SumAsync(bi => bi.Price);

                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{addedSeatsCount} oturacaq səbətə əlavə edildi";
            }
            else
            {
                TempData["Info"] = "Seçilmiş oturacaqlar artıq səbətdə mövcuddur";
            }

            return RedirectToAction("Index", "Basket", new { area = "" });
        }

        public async Task<IActionResult> ViewBasket()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBasket(int basketItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Istifadeci tapılmadı" });
            }

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == basketItemId && bi.Basket.UserId == user.Id);

            if (basketItem == null)
            {
                return Json(new { success = false, message = "Element tapılmadı" });
            }

            var basket = basketItem.Basket;

            _context.BasketItems.Remove(basketItem);
            await _context.SaveChangesAsync();

            basket.TotalPrice = await _context.BasketItems
                .Where(bi => bi.BasketId == basket.Id)
                .Select(bi => (decimal?)bi.Price)
                .SumAsync() ?? 0;

            _context.Baskets.Update(basket);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Element səbətdən silindi" });
        }

        [HttpPost]
        public async Task<IActionResult> ClearBasket()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket != null)
            {
                _context.BasketItems.RemoveRange(basket.Items);
                basket.TotalPrice = 0;
                _context.Baskets.Update(basket);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        //public async Task<IActionResult> Checkout()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var basket = await _context.Baskets
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Session)
        //                .ThenInclude(s => s.Movie)
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Seat)
        //        .FirstOrDefaultAsync(b => b.UserId == user.Id);

        //    if (basket == null || !basket.Items.Any())
        //    {
        //        TempData["Error"] = "Səbətiniz boşdu";
        //        return RedirectToAction("Index", "Home");
        //    }

        //    return View(basket);
        //}

        [HttpPost]
        public async Task<IActionResult> CompleteOrder()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                return Json(new { success = false, message = "Səbət boşdur" });
            }

            var bookings = new List<Booking>();

            foreach (var item in basket.Items)
            {
                var existingBooking = bookings.FirstOrDefault(b => b.SessionId == item.SessionId);
                if (existingBooking == null)
                {
                    var newBooking = new Booking
                    {
                        UserId = user.Id,
                        SessionId = item.SessionId,
                        BookingDate = DateTime.Now,
                        Status = BookingStatus.Reserved,
                        TotalAmount = item.Price,
                        Tickets = new List<Ticket>
                {
                    new Ticket
                    {
                        SeatId = item.SeatId,
                        SessionId = item.SessionId,
                        Price = item.Price,
                        Status = TicketStatus.Reserved
                    }
                }
                    };

                    bookings.Add(newBooking);
                }
                else
                {
                    existingBooking.Tickets.Add(new Ticket
                    {
                        SeatId = item.SeatId,
                        SessionId = item.SessionId,
                        Price = item.Price,
                        Status = TicketStatus.Reserved
                    });
                    existingBooking.TotalAmount += item.Price;
                }
            }

            await _context.Bookings.AddRangeAsync(bookings);
            foreach (var booking in bookings)
            {
                _context.Tickets.AddRange(booking.Tickets);
            }

            _context.BasketItems.RemoveRange(basket.Items);
            basket.TotalPrice = 0;
            _context.Baskets.Update(basket);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Rezervasiya uğurla tamamlandı" });
        }
        

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var basket = await _context.Baskets
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbət boşdur";
                return RedirectToAction("Index", "Basket");
            }

            var seatIds = basket.Items.Select(i => i.SeatId).ToList();
            var sessionId = basket.Items.First().SessionId;

            var reservedSeats = await _context.Tickets
                .Where(t => seatIds.Contains(t.SeatId) &&
                            t.SessionId == sessionId &&
                            (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Reserved))
                .Select(t => t.SeatId)
                .ToListAsync();

            if (reservedSeats.Any())
            {
                TempData["Error"] = "Səbətdəki bəzi oturacaqlar artıq satılıb.";
                return RedirectToAction("Index", "Basket");
            }

            // Sold tipini tap
            var soldSeatType = await _context.SeatTypes.FirstOrDefaultAsync(st => st.Name == "Sold");
            if (soldSeatType == null)
            {
                TempData["Error"] = "Sistem xətası: 'Sold' oturacaq tipi tapılmadı.";
                return RedirectToAction("Index", "Basket");
            }

            // Booking obyektini yarat
            var booking = new Booking
            {
                UserId = user.Id,
                SessionId = sessionId,
                BookingDate = DateTime.UtcNow,
                Status = BookingStatus.Paid,
                TotalAmount = basket.TotalPrice
            };
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync(); // Booking.Id üçün

            // Hər bilet üçün Ticket yarat və SeatType dəyiş
            foreach (var item in basket.Items)
            {
                var ticket = new Ticket
                {
                    BookingId = booking.Id,
                    SeatId = item.SeatId,
                    SessionId = item.SessionId,
                    Price = item.Price,
                    Status = TicketStatus.Sold,
                    PurchasedAt = DateTime.UtcNow
                };

                await _context.Tickets.AddAsync(ticket);

                // Oturacağın statusunu dəyiş: SeatTypeId -> Sold
                item.Seat.SeatTypeId = soldSeatType.Id;
            }

            // Səbəti təmizlə
            _context.BasketItems.RemoveRange(basket.Items);
            basket.TotalPrice = 0;
            _context.Baskets.Update(basket);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Biletlər uğurla alındı!";
            return RedirectToAction("MyTickets", "Account", new { area = "" });
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateStripeSession()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null) return RedirectToAction("Login", "Account");

        //    var basket = await _context.Baskets
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Session)
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Seat)
        //        .FirstOrDefaultAsync(b => b.UserId == user.Id);

        //    if (basket == null || !basket.Items.Any())
        //    {
        //        TempData["Error"] = "Səbət boşdur";
        //        return RedirectToAction("Index", "Basket");
        //    }

        //    var reservedSeats = await _context.Tickets
        //        .Where(t => basket.Items.Select(i => i.SeatId).Contains(t.SeatId)
        //                    && t.SessionId == basket.Items.First().SessionId
        //                    && (t.Status == TicketStatus.Sold || t.Status == TicketStatus.Reserved))
        //        .ToListAsync();

        //    if (reservedSeats.Any())
        //    {
        //        TempData["Error"] = "Bəzi oturacaqlar artıq satılmışdır.";
        //        return RedirectToAction("Index", "Basket");
        //    }

        //    var domain = $"{Request.Scheme}://{Request.Host}";
        //    var options = new SessionCreateOptions
        //    {
        //        PaymentMethodTypes = new List<string> { "card" },
        //        LineItems = basket.Items.Select(item => new SessionLineItemOptions
        //        {
        //            PriceData = new SessionLineItemPriceDataOptions
        //            {
        //                UnitAmount = (long)(item.Price * 100),
        //                Currency = "usd",
        //                ProductData = new SessionLineItemPriceDataProductDataOptions
        //                {
        //                    Name = $"Film: {item.Session.Movie.Title}, Yer: {item.Seat.Row}-{item.Seat.Number}"
        //                }
        //            },
        //            Quantity = 1
        //        }).ToList(),
        //        Mode = "payment",
        //        SuccessUrl = domain + Url.Action("Success"),
        //        CancelUrl = domain + Url.Action("Cancel")
        //    };

        //    var service = new SessionService();
        //    var session = service.Create(options);

        //    return Redirect(session.Url);
        //}

        //public async Task<IActionResult> Success()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null) return RedirectToAction("Login", "Account");

        //    var basket = await _context.Baskets
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Session)
        //        .Include(b => b.Items)
        //            .ThenInclude(i => i.Seat)
        //        .FirstOrDefaultAsync(b => b.UserId == user.Id);

        //    if (basket == null || !basket.Items.Any())
        //    {
        //        TempData["Error"] = "Səbətdə bilet tapılmadı";
        //        return RedirectToAction("Index", "Basket");
        //    }

        //    var booking = new Booking
        //    {
        //        UserId = user.Id,
        //        SessionId = basket.Items.First().SessionId,
        //        BookingDate = DateTime.UtcNow,
        //        TotalAmount = basket.Items.Sum(i => i.Price),
        //        Status = BookingStatus.Paid
        //    };
        //    _context.Bookings.Add(booking);
        //    await _context.SaveChangesAsync();

        //    foreach (var item in basket.Items)
        //    {
        //        var ticket = new Ticket
        //        {
        //            BookingId = booking.Id,
        //            SeatId = item.SeatId,
        //            SessionId = item.SessionId,
        //            Price = item.Price,
        //            Status = TicketStatus.Sold,
        //            PurchasedAt = DateTime.UtcNow
        //        };
        //        _context.Tickets.Add(ticket);
        //        item.Seat.Status = SeatStatus.Sold;
        //    }

        //    _context.BasketItems.RemoveRange(basket.Items);
        //    basket.TotalPrice = 0;
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Ödəniş uğurla tamamlandı və bilet yaradıldı.";
        //    return RedirectToAction("MyTickets", "Account");
        //}

        //public IActionResult Cancel()
        //{
        //    TempData["Error"] = "Ödəniş ləğv edildi.";
        //    return RedirectToAction("Index", "Basket");
        //}
    }
}
