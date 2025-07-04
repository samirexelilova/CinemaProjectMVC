using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;

namespace StreamitMVC.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await CleanExpiredReservations(user.Id);

            var basket = await _context.Baskets
                .Include(b => b.Items.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Movie)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Hall)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Session)
                        .ThenInclude(s => s.Cinema)
                .Include(b => b.Items)
                    .ThenInclude(i => i.Seat)
                        .ThenInclude(s => s.SeatType)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbətiniz boşdur";
                return View(new Basket { Items = new List<BasketItem>(), TotalPrice = 0 });
            }

            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int? itemId)
        {
            if (itemId == null || itemId <= 0)
                return Json(new { success = false, message = "Yanlış məlumat" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });

            var basketItem = await _context.BasketItems
                .Include(bi => bi.Basket)
                .FirstOrDefaultAsync(bi => bi.Id == itemId && bi.Basket.UserId == user.Id);

            if (basketItem == null)
                return Json(new { success = false, message = "Element tapılmadı" });

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
                    .Where(bi => bi.BasketId == basket.Id && bi.Id != itemId)
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

        [HttpPost]
        public async Task<IActionResult> ClearBasket()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "İstifadəçi tapılmadı" });

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
                return Json(new { success = false, message = "Səbət artıq boşdur" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in basket.Items)
                {
                    var ticket = await _context.Tickets
                        .FirstOrDefaultAsync(t => t.SeatId == item.SeatId &&
                                                t.SessionId == item.SessionId &&
                                                t.Status == TicketStatus.Reserved);

                    if (ticket != null)
                    {
                        _context.Tickets.Remove(ticket);
                    }
                }

                _context.BasketItems.RemoveRange(basket.Items);
                basket.TotalPrice = 0;
                _context.Baskets.Update(basket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Səbət boşaldıldı" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Xəta baş verdi" });
            }
        }

        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == user.Id);

            if (basket == null || !basket.Items.Any())
            {
                TempData["Error"] = "Səbətiniz boşdur";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Checkout", "Booking");
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
    }
}
