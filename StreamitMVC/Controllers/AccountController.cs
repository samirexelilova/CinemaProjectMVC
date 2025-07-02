using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.DAL;
using StreamitMVC.Extensions.Enums;
using StreamitMVC.Models;
using StreamitMVC.Services.Interfaces;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;
using StreamitMVC.ViewModels.AccountVM;
using System.Security.Claims;

namespace StreamitMVC.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment WebHostEnvironment;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context,
             IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            WebHostEnvironment = webHostEnvironment;
            _emailService = emailService;
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                new { token, email = appUser.Email }, Request.Scheme);

            await _emailService.SendConfirmationEmailAsync(appUser.Email, confirmationLink);

            TempData["Message"] = "Qeydiyyat uğurla tamamlandı! Email ünvanınıza göndərilən linkə klik edərək hesabınızı təsdiqlənməlisiniz.";
            return RedirectToAction(nameof(RegisterConfirmation));
        }

        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "İstifadəçi tapılmadı";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["Success"] = "Email ünvanınız uğurla təsdiqləndi! İndi daxil ola bilərsiniz.";
                return RedirectToAction(nameof(Login));
            }

            TempData["Error"] = "Email təsdiqlənməsində xəta baş verdi";
            return RedirectToAction(nameof(HomeController.Index), "Home");
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

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Email ünvanınızı təsdiqlənməmisiniz. Zəhmət olmasa email ünvanınızı yoxlayın.");
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

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(nameof(ResetPassword), "Account",
                new { token, email = user.Email }, Request.Scheme);

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Token və ya email yoxdur.");
            }

            var model = new ResetPasswordVM
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Error"] = "İstifadəçi tapılmadı";
                return RedirectToAction(nameof(Login));
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["Info"] = "Email artıq təsdiqlənib";
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account",
                new { token, email = user.Email }, Request.Scheme);

            await _emailService.SendConfirmationEmailAsync(user.Email, confirmationLink);

            TempData["Success"] = "Təsdiqləmə emaili yenidən göndərildi";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme); 
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleResponse), "Account", new { returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);

            properties.Items["prompt"] = "select_account";

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string? returnUrl = null)
        {
            ExternalLoginInfo? loginInfo = await _signInManager.GetExternalLoginInfoAsync();

            if (loginInfo == null)
                return RedirectToAction(nameof(Login));

            var signInResult = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
                return RedirectToLocal(returnUrl);

            var email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            var name = loginInfo.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = loginInfo.Principal.FindFirstValue(ClaimTypes.Surname);

            AppUser user = new AppUser
            {
                UserName = email,
                Email = email,
                Name = name ?? "Google",
                Surname = surname ?? "User"
            };

            IdentityResult createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View("Login");
            }

            await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
            await _userManager.AddLoginAsync(user, loginInfo);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
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
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "İstifadəçi tapılmadı. Yenidən daxil olun.";
                    return RedirectToAction("Login");
                }

                var booking = await _context.Bookings
                    .Include(b => b.Session)
                    .Include(b => b.Tickets)
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

                if (booking == null)
                {
                    TempData["Error"] = "Sifariş tapılmadı və ya sizə aid deyil.";
                    return RedirectToAction("MyProfile");
                }

                if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Refunded)
                {
                    TempData["Error"] = "Bu sifariş artıq ləğv edilib və ya geri qaytarılıb.";
                    return RedirectToAction("MyProfile");
                }

                if (booking.Status != BookingStatus.Paid && booking.Status != BookingStatus.Reserved)
                {
                    TempData["Error"] = $"Yalnız ödənilmiş və ya rezerv edilmiş sifarişlər ləğv edilə bilər. Cari status: {booking.Status}";
                    return RedirectToAction("MyProfile");
                }

                if (booking.Session != null)
                {
                    if (booking.Status == BookingStatus.Paid)
                    {
                        var hoursSinceBooking = DateTime.Now - booking.BookingDate;
                        if (hoursSinceBooking.TotalHours > 24)
                        {
                            TempData["Error"] = $"Sifariş verildikdən sonra 24 saat keçib. Ləğv edilə bilməz.";
                            return RedirectToAction("MyProfile");
                        }
                    }
                }

                if (booking.Status == BookingStatus.Paid)
                {
                    var existingRefund = await _context.Refunds
                        .FirstOrDefaultAsync(r => r.BookingId == booking.Id);

                    if (existingRefund != null)
                    {
                        TempData["Error"] = "Bu sifariş üçün artıq geri ödəmə tələbi mövcuddur.";
                        return RedirectToAction("MyProfile");
                    }
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    booking.Status = BookingStatus.Cancelled;
                    _context.Bookings.Update(booking);

                    var ticketCount = 0;
                    foreach (var ticket in booking.Tickets)
                    {
                        ticket.Status = TicketStatus.Cancelled;
                        _context.Tickets.Update(ticket);
                        ticketCount++;
                    }

                    if (booking.Status == BookingStatus.Paid)
                    {
                        var currentUser = await _userManager.FindByIdAsync(user.Id);
                        if (currentUser == null)
                        {
                            throw new Exception("İstifadəçi yenidən tapılmadı");
                        }

                        if (currentUser.WalletBalance == null)
                        {
                            currentUser.WalletBalance = 0;
                        }

                        var originalBalance = currentUser.WalletBalance.Value;

                        currentUser.WalletBalance = (currentUser.WalletBalance ?? 0) + booking.TotalAmount;

                        var userUpdateResult = await _userManager.UpdateAsync(currentUser);
                        if (!userUpdateResult.Succeeded)
                        {
                            var errors = string.Join(", ", userUpdateResult.Errors.Select(e => e.Description));
                            throw new Exception($"İstifadəçi məlumatları yenilənmədi: {errors}");
                        }

                        var payment = await _context.Payments
                            .FirstOrDefaultAsync(p => p.BookingId == booking.Id);

                        var refund = new Refund
                        {
                            BookingId = booking.Id,
                            PaymentId = payment?.Id,
                            Amount = booking.TotalAmount,
                            Status = RefundStatus.Completed,
                            RequestedAt = DateTime.Now,
                            ProcessedAt = DateTime.Now,
                            Reason = "İstifadəçi tərəfindən ləğv edildi"
                        };

                        _context.Refunds.Add(refund);
                    }

                    var changesSaved = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (booking.Status == BookingStatus.Paid)
                    {
                        TempData["Success"] = $"Sifariş uğurla ləğv edildi. {booking.TotalAmount:F2} AZN hesabınıza əlavə edildi.";
                    }
                    else
                    {
                        TempData["Success"] = $"Rezervasiya uğurla ləğv edildi. Oturacaqlar azad edildi.";
                    }

                    return RedirectToAction("MyProfile");
                }
                catch (Exception innerEx)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Sifarişi ləğv etməkdə xəta baş verdi: {innerEx.Message}";
                    return RedirectToAction("MyProfile");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                TempData["Error"] = $"Ümumi xəta baş verdi: {ex.Message}";
                return RedirectToAction("MyProfile");
            }
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
                    PhoneNumber = user.PhoneNumber,
                    WalletBalance = user.WalletBalance ?? 0
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
            user.PhoneNumber = model.PhoneNumber ?? "Telefon nomresi yoxdur";

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
                        Gender = user.Gender ?? Gender.Others,
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
                    Gender = user.Gender ?? Gender.Others,
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
