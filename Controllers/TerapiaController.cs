using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class TerapiaController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index()
        {
            var lista = await _client.Child("Terapias").OnceAsync<Terapia>();
            var resultado = lista.Select(x => new Terapia
            {
                Id = x.Key,
                Nombre = x.Object.Nombre,
                Descripcion = x.Object.Descripcion,
                Precio = x.Object.Precio,
                DuracionMinutos = x.Object.DuracionMinutos,
                EsPaquete = x.Object.EsPaquete
            }).ToList();
            return View(resultado);
        }

        [HttpPost]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> Guardar(Terapia terapia)
        {
            if (string.IsNullOrEmpty(terapia.Id))
            {
                await _client.Child("Terapias").PostAsync(new Terapia
                {
                    Nombre = terapia.Nombre,
                    Descripcion = terapia.Descripcion,
                    Precio = terapia.Precio,
                    DuracionMinutos = terapia.DuracionMinutos,
                    EsPaquete = terapia.EsPaquete
                });
            }
            else
            {
                await _client.Child("Terapias").Child(terapia.Id).PutAsync(new Terapia
                {
                    Nombre = terapia.Nombre,
                    Descripcion = terapia.Descripcion,
                    Precio = terapia.Precio,
                    DuracionMinutos = terapia.DuracionMinutos,
                    EsPaquete = terapia.EsPaquete
                });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> Eliminar(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await _client.Child("Terapias").Child(id).DeleteAsync();
            }
            return RedirectToAction("Index");
        }
    }
}