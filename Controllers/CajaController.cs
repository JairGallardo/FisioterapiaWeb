using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index()
        {
            var datos = await _client.Child("Caja").OnceAsync<Transaccion>();
            var lista = datos.Select(x => {
                var t = x.Object;
                t.Id = x.Key;
                return t;
            }).ToList();

            return View(lista);
        }

        public async Task<IActionResult> NuevoPago(string? pacienteId, string? nombre)
        {
            if (!string.IsNullOrEmpty(nombre))
            {
                ViewBag.PacienteId = pacienteId;
                ViewBag.PacienteNombre = nombre;
            }
            else
            {
                var pacientes = await _client.Child("Pacientes").OnceAsync<Paciente>();
                ViewBag.Pacientes = pacientes.Select(x => new Paciente
                {
                    Id = x.Key,
                    Nombre = x.Object.Nombre,
                    Apellido = x.Object.Apellido
                }).ToList();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPago(Transaccion t)
        {
            await _client.Child("Caja").PostAsync(t);
            return RedirectToAction("Index");
        }
    }
}