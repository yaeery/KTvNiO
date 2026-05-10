using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KTvNiO.Models;
using KTvNiO.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace KTvNiO.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Поиск пользователя в БД по Email или Username
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Email);

                if (user != null && user.PasswordHash == model.Password)
                {
                    // Создание claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim("UserRole", user.UserRole)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties();

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Логируем вход
                    await LogActivity(user.Id, "Login", "User", user.Id, "Вход в систему");

                    // Перенаправляем в зависимости от роли
                    switch (user.UserRole)
                    {
                        case "Teacher":
                            return RedirectToAction("Index", "Courses");
                        case "Developer":
                        case "Support":
                            return RedirectToAction("Index", "Admin");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Проверка на существование
                if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Пользователь с таким именем или Email уже существует.");
                    return View(model);
                }

                var newUser = new User
                {
                    Username = model.Username ?? model.Email,
                    Email = model.Email,
                    PasswordHash = model.Password, // В демо без хэширования
                    FullName = model.FullName,
                    UserRole = "Student", // По умолчанию студент
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Автоматический вход после регистрации
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, newUser.Username),
                    new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                    new Claim("UserRole", newUser.UserRole)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                await LogActivity(newUser.Id, "Register", "User", newUser.Id, "Регистрация нового пользователя");

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                await LogActivity(int.Parse(userIdClaim.Value), "Logout", "User", int.Parse(userIdClaim.Value), "Выход из системы");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task LogActivity(int userId, string action, string entityName, int entityId, string details)
        {
            try
            {
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    ActionName = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    Details = details,
                    Timestamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
                await _context.SaveChangesAsync();
            }
            catch { /* Игнорируем ошибки логирования */ }
        }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email или логин")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "ФИО")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }
        
        [Required]
        [Display(Name = "Логин")]
        public string Username { get; set; }
    }
}
