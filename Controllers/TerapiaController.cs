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
    }
}