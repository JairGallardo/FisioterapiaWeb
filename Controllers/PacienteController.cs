using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class PacienteController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index()
        {
            var lista = await _client.Child("Pacientes").OnceAsync<Paciente>();
            var resultado = lista.Select(x => new Paciente
            {
                Id = x.Key,
                Nombre = x.Object.Nombre,
                Apellido = x.Object.Apellido,
                Dni = x.Object.Dni,
                Telefono = x.Object.Telefono,
                Correo = x.Object.Correo
            }).ToList();

            return View(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(Paciente nuevoPaciente)
        {
            await _client.Child("Pacientes").PostAsync(nuevoPaciente);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(string id)
        {
            var p = await _client.Child("Pacientes").Child(id).OnceSingleAsync<Paciente>();
            if (p == null) return NotFound();
            p.Id = id;
            return View(p);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(Paciente p)
        {
            await _client.Child("Pacientes").Child(p.Id).PutAsync(p);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                await _client.Child("Pacientes").Child(id).DeleteAsync();
            }
            return RedirectToAction("Index");
        }
    }
}