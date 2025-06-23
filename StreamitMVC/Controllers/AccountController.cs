using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StreamitMVC.Models;
using StreamitMVC.Utilities.Enums;
using StreamitMVC.Utilities.Extensions;
using StreamitMVC.ViewModels;

namespace StreamitMVC.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IWebHostEnvironment WebHostEnvironment;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                Image = fileName
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
    }
}
