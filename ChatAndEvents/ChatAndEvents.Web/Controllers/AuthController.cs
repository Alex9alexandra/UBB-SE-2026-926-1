using Microsoft.AspNetCore.Mvc;
using ChatAndEvents.Web.Models;
using ChatAndEvents.Data.ChatData.services;
using System.Threading.Tasks;
using System;

namespace ChatAndEvents.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthentificationService _authService;

        // Serviciul este injectat automat prin Dependency Injection
        public AuthController(IAuthentificationService authService)
        {
            _authService = authService;
        }

        // --- Rutele pentru LOGIN ---

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _authService.LoginAsync(model.Username, model.Password);

                if (user != null)
                {
                    // Aici colegul tău (sau tu mai târziu) va adăuga logica de Cookie / Salvare Sesiune
                    // Deocamdată, dacă e succes, îl trimitem pe pagina principală (Home)
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // --- Rutele pentru FORGOT PASSWORD ---

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.ChangePasswordAsync(model.Email, model.NewPassword);

                // Transmitem mesajul de succes către View
                ViewBag.SuccessMessage = "Password updated successfully!";
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // --- Register routes ---

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _authService.RegisterAsync(
                    model.Username,
                    model.Email,
                    model.Password,
                    model.Phone,
                    model.Birthday,
                    avatarUrl: null);

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}