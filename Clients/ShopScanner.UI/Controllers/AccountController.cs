using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ShopScanner.UI.Handlers;
using ShopScanner.UI.Model.LoginModel;
using ShopScanner.UI.Utils;

namespace ShopScanner.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly DefaultClient _client;

        public AccountController(DefaultClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var response = await _client.PostAsync<LoginRequest, LoginResponse>(DefaultClientEndpoint.Authentice.Login, model);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                HttpContext.Session.SetString("JWToken", response.Token);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Login failed");
            return View(model);
        }
    }
}
