using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

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
            ViewBag.Pacientes = listaPacientes
                .Where(x => x.Object.Estado == "Activo")
                .Select(x => new Paciente
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

        [HttpGet]
        public async Task<IActionResult> ObtenerSesionesPorPaciente(string pacienteId)
        {
            if (string.IsNullOrEmpty(pacienteId)) return BadRequest();

            var listaCitas = await _client.Child("Citas").OnceAsync<Cita>();
            var citasPaciente = listaCitas
                .Where(x => x.Object.PacienteId == pacienteId)
                .Select(x => new {
                    Id = x.Key,
                    Fecha = x.Object.Fecha.ToString("dd/MM/yyyy"),
                    Hora = x.Object.Hora,
                    Tratamiento = x.Object.Tratamiento,
                    Estado = x.Object.Estado ?? "Pendiente"
                })
                .ToList();

            return Json(citasPaciente);
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
            if (nueva.DuracionMinutos == 0) nueva.DuracionMinutos = 45;

            await _client.Child("Citas").PostAsync(nueva);
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

        [HttpGet]
        public async Task<IActionResult> ObtenerCitasPorFecha(string fecha)
        {
            if (!DateTime.TryParse(fecha, out DateTime fechaFiltro))
            {
                return BadRequest("Fecha inválida");
            }

            var lista = await _client.Child("Citas").OnceAsync<Cita>();
            var resultado = lista
                .Select(x => {
                    var c = x.Object;
                    c.Id = x.Key;
                    return c;
                })
                .Where(x => x.Fecha.Date == fechaFiltro.Date)
                .OrderBy(x => x.Hora)
                .ToList();

            return Json(resultado);
        }

        public async Task<IActionResult> CambiarEstado(string id, string nuevoEstado)
        {
            var cita = await _client.Child("Citas").Child(id).OnceSingleAsync<Cita>();
            if (cita == null) return NotFound();

            await _client.Child("Citas").Child(id).Child("Estado").PutAsync($"\"{nuevoEstado}\"");

            if (nuevoEstado == "Completada")
            {
                var transaccion = new Transaccion
                {
                    PacienteId = cita.PacienteId,
                    PacienteNombre = cita.NombrePaciente,
                    Concepto = $"Pago por: {cita.Tratamiento}",
                    Monto = cita.Monto,
                    MetodoPago = cita.MetodoPago ?? "Efectivo",
                    Fecha = DateTime.Now
                };
                await _client.Child("Caja").PostAsync(transaccion);
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            await _client.Child("Citas").Child(id).DeleteAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            return RedirectToAction("Index");
        }
    }
}