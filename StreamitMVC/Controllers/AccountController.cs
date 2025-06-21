using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StreamitMVC.Models;

namespace StreamitMVC.Controllers
{
    public class AccountController : Controller
    {
      
            private readonly UserManager<AppUser> _userManager;
            private readonly SignInManager<AppUser> _signInManager;
            private readonly RoleManager<IdentityRole> _roleManager;

            public AccountController(
                UserManager<AppUser> userManager,
                SignInManager<AppUser> signInManager,
                RoleManager<IdentityRole> roleManager)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _roleManager = roleManager;
            }
            public IActionResult Register()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Register(RegisterVM registerVM)
            {

                if (!ModelState.IsValid) return View();

                AppUser user = new AppUser
                {
                    Name = registerVM.Name,
                    Surname = registerVM.Surname,
                    UserName = registerVM.UserName,
                    Email = registerVM.Email
                };

                IdentityResult result = await _userManager.CreateAsync(user, registerVM.Password);

                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
                await _signInManager.SignInAsync(user, false);


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

                AppUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginVM.UsernameOrEmail || u.Email == loginVM.UsernameOrEmail);
                if (user is null)
                {
                    ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                    return View();
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.IsPersistent, true);

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account is not available please try later");
                    return View();
                }


                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                    return View();
                }

                if (returnUrl is null) return RedirectToAction("Index", "Home");


                return Redirect(returnUrl);
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
