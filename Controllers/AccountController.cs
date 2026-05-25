using Firebase.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FisioterapiaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            string rol = "";

            if (usuario == "admin@gmail.com" && password == "1234")
            {
                rol = "Gerente";
            }
            else
            {
                var datosFirebase = await _client.Child("Usuarios").OnceAsync<dynamic>();
                var usuarioRegistrado = datosFirebase.FirstOrDefault(x =>
                    x.Object.Correo == usuario &&
                    x.Object.Contrasena == password &&
                    x.Object.Estado == "Activo");

                if (usuarioRegistrado != null)
                {
                    rol = usuarioRegistrado.Object.Rol;
                }
            }

            if (!string.IsNullOrEmpty(rol))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario),
                    new Claim(ClaimTypes.Role, rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Credenciales incorrectas o usuario inactivo";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}