using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class CitaController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index(string buscar, string estado)
        {
            var lista = await _client.Child("Citas").OnceAsync<Cita>();
            var resultado = lista.Select(x => {
                var c = x.Object;
                c.Id = x.Key;
                return c;
            });

            if (!string.IsNullOrEmpty(buscar))
            {
                resultado = resultado.Where(x => x.NombrePaciente != null &&
                            x.NombrePaciente.Contains(buscar, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                resultado = resultado.Where(x => x.Estado == estado);
            }

            var listaPacientes = await _client.Child("Pacientes").OnceAsync<Paciente>();
            ViewBag.Pacientes = listaPacientes.Select(x => new Paciente
            {
                Id = x.Key,
                Nombre = x.Object.Nombre,
                Apellido = x.Object.Apellido
            }).ToList();

            var listaTerapias = await _client.Child("Terapias").OnceAsync<Terapia>();
            ViewBag.Terapias = listaTerapias.Select(x => new Terapia
            {
                Id = x.Key,
                Nombre = x.Object.Nombre,
                Precio = x.Object.Precio
            }).ToList();

            return View(resultado.OrderBy(x => x.Fecha).ThenBy(x => x.Hora).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Nueva(Cita nueva)
        {
            var paciente = await _client.Child("Pacientes").Child(nueva.PacienteId).OnceSingleAsync<Paciente>();
            if (paciente != null)
            {
                nueva.NombrePaciente = $"{paciente.Nombre} {paciente.Apellido}";
            }
            nueva.Estado = "Reservado";
            nueva.Lugar = "Consultorio 1";

            await _client.Child("Cita").PostAsync(nueva);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(string id)
        {
            var cita = await _client.Child("Citas").Child(id).OnceSingleAsync<Cita>();
            if (cita == null) return NotFound();
            cita.Id = id;

            var listaPacientes = await _client.Child("Pacientes").OnceAsync<Paciente>();
            ViewBag.Pacientes = listaPacientes.Select(x => new Paciente { Id = x.Key, Nombre = x.Object.Nombre, Apellido = x.Object.Apellido }).ToList();

            var listaTerapias = await _client.Child("Terapias").OnceAsync<Terapia>();
            ViewBag.Terapias = listaTerapias.Select(x => new Terapia { Id = x.Key, Nombre = x.Object.Nombre }).ToList();

            return View(cita);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(Cita cita)
        {
            await _client.Child("Citas").Child(cita.Id).PutAsync(cita);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            await _client.Child("Citas").Child(id).DeleteAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CambiarEstado(string id, string nuevoEstado)
        {
            await _client.Child("Citas").Child(id).Child("Estado").PutAsync($"\"{nuevoEstado}\"");
            return RedirectToAction("Index");
        }
    }
}