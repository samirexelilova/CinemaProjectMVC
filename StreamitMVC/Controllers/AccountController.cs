using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;
using StreamitMVC.ViewModels.AccountVM;

namespace StreamitMVC.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment WebHostEnvironment;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            WebHostEnvironment = webHostEnvironment;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVM);
            }

            string normalizedName = registerVM.Name.NormalizeText();
            string normalizedSurname = registerVM.Surname.NormalizeText();

            if (normalizedName is null)
            {
                ModelState.AddModelError(nameof(registerVM.Name), "Adda rəqəm ola bilməz");
                return View(registerVM);
            }

            if (normalizedSurname is null)
            {
                ModelState.AddModelError(nameof(registerVM.Surname), "Soyadda rəqəm ola bilməz");
                return View(registerVM);
            }

            string? fileName = null;
            if (registerVM.ImageFile is not null)
            {
                if (!registerVM.ImageFile.ValidateType("image/"))
                {
                    ModelState.AddModelError(nameof(registerVM.ImageFile), "Yalnız şəkil faylı ola bilər");
                    return View(registerVM);
                }

                if (!registerVM.ImageFile.ValidateSize(FileSize.MB, 2))
                {
                    ModelState.AddModelError(nameof(registerVM.ImageFile), "Şəklin ölçüsü maksimum 2MB ola bilər");
                    return View(registerVM);
                }

                fileName = await registerVM.ImageFile.CreateFileAsync(
                    WebHostEnvironment.WebRootPath, "assets", "images", "user");
            }

            AppUser appUser = new AppUser
            {
                Name = normalizedName,
                Surname = normalizedSurname,
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                Gender = registerVM.Gender,
                BirthDate = registerVM.BirthDate,
                Address = registerVM.Address,
                Image = fileName,
                PhoneNumber = registerVM.PhoneNumber
            };

            IdentityResult result = await _userManager.CreateAsync(appUser, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(registerVM);
            }

            await _userManager.AddToRoleAsync(appUser, UserRole.Member.ToString());
            await _signInManager.SignInAsync(appUser, isPersistent: false);

            return RedirectToAction(nameof(HomeController.Index), "home");
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM, string? returnUrl)
        {
            if (!ModelState.IsValid) return View();

            AppUser? user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "İstifadəçi adı, email və ya şifrə yanlışdır");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistent, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Hesab bloklanıb. Zəhmət olmasa 10 dəqiqə sonra yenidən cəhd edin");
                return View();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "İstifadəçi adı, email və ya şifrə yanlışdır");
                return View();
            }

            if (returnUrl is not null)
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                if (!await _roleManager.RoleExistsAsync(role.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = role.ToString()
                    });
                }

            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> MyTickets()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var allUserTickets = await _context.Tickets
                .Include(t => t.Seat)
                    .ThenInclude(s => s.SeatType)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Booking)
                .Where(t => t.Booking.UserId == user.Id)
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();


            var soldTickets = allUserTickets.Where(t => t.Status == TicketStatus.Sold).ToList();


            var userBookings = await _context.Bookings
             .Where(b => b.UserId == user.Id)
             .ToListAsync();

            var bookingIds = userBookings.Select(b => b.Id).ToList();

            var payments = await _context.Payments
                .Where(p => bookingIds.Contains(p.BookingId))
                .ToListAsync();

            foreach (var booking in userBookings)
            {
                var relatedPayments = payments.Where(p => p.BookingId == booking.Id).ToList();
            }

            return View(soldTickets);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

            if (booking == null)
                return NotFound();

            booking.Status = BookingStatus.Cancelled;
            await _context.SaveChangesAsync();

            return RedirectToAction("MyProfile"); 
        }
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var bookings = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .Include(b => b.Session).ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingViewModel
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    TicketCount = b.Tickets.Count,
                    SessionName = b.Session.Movie.Name,
                    SessionDate = b.Session.StartTime
                })
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Movie)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    MovieName = r.Movie.Name,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var viewModel = new MyProfileVM
            {
                User = new UserProfileViewModel
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Gender = user.Gender,
                    Image = user.Image,
                    BirthDate = user.BirthDate,
                    Address = user.Address,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                AccountDetails = new AccountDetailsViewModel
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    DisplayName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                Bookings = bookings,
                Reviews = reviews
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                model.Message = "Please fill all required fields correctly.";
                model.IsSuccess = false;

                var bookings = await _context.Bookings
                    .Where(b => b.UserId == user.Id)
                    .Include(b => b.Session).ThenInclude(s => s.Movie)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        BookingDate = b.BookingDate,
                        Status = b.Status,
                        TotalAmount = b.TotalAmount,
                        TicketCount = b.Tickets.Count,
                        SessionName = b.Session.Movie.Name,
                        SessionDate = b.Session.StartTime
                    })
                    .ToListAsync();

                var reviews = await _context.Reviews
                    .Where(r => r.UserId == user.Id)
                    .Include(r => r.Movie)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        MovieName = r.Movie.Name,
                        Comment = r.Comment,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

                var fullModel = new MyProfileVM
                {
                    User = model,
                    Bookings = bookings,
                    Reviews = reviews
                };

                return View("MyProfile", fullModel);
            }

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Gender = model.Gender;
            user.Image = model.Image;
            user.BirthDate = model.BirthDate;
            user.Address = model.Address;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            model.Message = result.Succeeded
                ? "Profile updated successfully!"
                : "Failed to update profile. Please try again.";
            model.IsSuccess = result.Succeeded;

            var userBookings = await _context.Bookings
                .Where(b => b.UserId == user.Id)
                .Include(b => b.Session).ThenInclude(s => s.Movie)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new BookingViewModel
                {
                    Id = b.Id,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    TicketCount = b.Tickets.Count,
                    SessionName = b.Session.Movie.Name,
                    SessionDate = b.Session.StartTime
                })
                .ToListAsync();

            var userReviews = await _context.Reviews
                .Where(r => r.UserId == user.Id)
                .Include(r => r.Movie)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    MovieName = r.Movie.Name,
                    Comment = r.Comment,
                    Rating = r.Rating,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var finalModel = new MyProfileVM
            {
                User = model,
                Bookings = userBookings,
                Reviews = userReviews
            };

            return View("MyProfile", finalModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAccountDetails(AccountDetailsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                model.Message = "Zəhmət olmasa məlumatları düzgün doldurun.";
                model.IsSuccess = false;

                var viewModel = new MyProfileVM
                {
                    AccountDetails = model,
                    User = new UserProfileViewModel
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Gender = user.Gender,
                        Image = user.Image,
                        BirthDate = user.BirthDate,
                        Address = user.Address,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    },
                    Bookings = await _context.Bookings
                        .Where(b => b.UserId == user.Id)
                        .Include(b => b.Session).ThenInclude(s => s.Movie)
                        .OrderByDescending(b => b.BookingDate)
                        .Select(b => new BookingViewModel
                        {
                            Id = b.Id,
                            BookingDate = b.BookingDate,
                            Status = b.Status,
                            TotalAmount = b.TotalAmount,
                            TicketCount = b.Tickets.Count,
                            SessionName = b.Session.Movie.Name,
                            SessionDate = b.Session.StartTime
                        })
                        .ToListAsync(),
                    Reviews = await _context.Reviews
                        .Where(r => r.UserId == user.Id)
                        .Include(r => r.Movie)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => new ReviewViewModel
                        {
                            Id = r.Id,
                            MovieName = r.Movie.Name,
                            Comment = r.Comment,
                            Rating = r.Rating,
                            CreatedAt = r.CreatedAt
                        })
                        .ToListAsync()
                };

                return View("MyProfile", viewModel);
            }

            user.Name = model.Name;
            user.Surname = model.Surname;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.DisplayName;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (model.NewPassword != model.ConfirmPassword)
                {
                    model.Message = "Yeni şifrə və təsdiqi uyğun gəlmir.";
                    model.IsSuccess = false;
                    return await ReturnMyProfileVM(user, model);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    model.Message = "Cari şifrə düzgün deyil və ya yeni şifrə tələblərə cavab vermir.";
                    model.IsSuccess = false;
                    return await ReturnMyProfileVM(user, model);
                }
            }

            model.Message = updateResult.Succeeded ? "Hesab məlumatları yeniləndi." : "Xəta baş verdi.";
            model.IsSuccess = updateResult.Succeeded;

            return await ReturnMyProfileVM(user, model);
        }

        private async Task<IActionResult> ReturnMyProfileVM(AppUser user, AccountDetailsViewModel model)
        {
            var viewModel = new MyProfileVM
            {
                AccountDetails = model,
                User = new UserProfileViewModel
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Gender = user.Gender,
                    Image = user.Image,
                    BirthDate = user.BirthDate,
                    Address = user.Address,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                Bookings = await _context.Bookings
                    .Where(b => b.UserId == user.Id)
                    .Include(b => b.Session).ThenInclude(s => s.Movie)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new BookingViewModel
                    {
                        Id = b.Id,
                        BookingDate = b.BookingDate,
                        Status = b.Status,
                        TotalAmount = b.TotalAmount,
                        TicketCount = b.Tickets.Count,
                        SessionName = b.Session.Movie.Name,
                        SessionDate = b.Session.StartTime
                    })
                    .ToListAsync(),
                Reviews = await _context.Reviews
                    .Where(r => r.UserId == user.Id)
                    .Include(r => r.Movie)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        MovieName = r.Movie.Name,
                        Comment = r.Comment,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync()
            };

            return View("MyProfile", viewModel);
        }


        //[HttpPost]
        //public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        TempData["Error"] = "Please provide valid password information.";
        //        return RedirectToAction("MyProfile");
        //    }

        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        //    if (result.Succeeded)
        //    {
        //        await _signInManager.RefreshSignInAsync(user);
        //        TempData["Success"] = "Password changed successfully!";
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Failed to change password. Please check your current password.";
        //    }

        //    return RedirectToAction("MyProfile");
        //}

        //[HttpPost]
        //public async Task<IActionResult> CancelOrder(int orderId)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var order = await _context.Orders
        //        .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

        //    if (order != null && order.Status != "Completed" && order.Status != "Cancelled")
        //    {
        //        order.Status = "Cancelled";
        //        await _context.SaveChangesAsync();
        //        TempData["Success"] = "Order cancelled successfully!";
        //    }
        //    else
        //    {
        //        TempData["Error"] = "Unable to cancel this order.";
        //    }

        //    return RedirectToAction("MyProfile");
        //}

        //[HttpGet]
        //public async Task<IActionResult> OrderDetails(int id)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var order = await _context.Orders
        //        .Include(o => o.OrderItems)
        //        .ThenInclude(oi => oi.Product)
        //        .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(order);
        //}


    }
}
