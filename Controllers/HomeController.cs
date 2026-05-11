using Firebase.Database;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index()
        {
            var pacientes = await _client.Child("Pacientes").OnceAsync<Paciente>();
            var citasRaw = await _client.Child("Citas").OnceAsync<Cita>();
            var transacciones = await _client.Child("Caja").OnceAsync<Transaccion>();

            ViewBag.TotalPacientes = pacientes.Count;

            var hoy = DateTime.Today;
            ViewBag.CitasHoy = citasRaw.Count(x => x.Object.Fecha.Date == hoy);

            ViewBag.TotalCaja = transacciones.Sum(x => x.Object.Monto);

            var eventos = citasRaw.Select(x => new {
                title = x.Object.NombrePaciente,
                start = ParsearFechaHora(x.Object.Fecha, x.Object.Hora),
                color = x.Object.Estado == "Completada" ? "#28a745" :
                        x.Object.Estado == "Cancelada" ? "#dc3545" : "#2c5d7d",
                description = x.Object.Tratamiento
            }).ToList();

            ViewBag.EventosJson = JsonSerializer.Serialize(eventos);

            var ultimasCitas = citasRaw.OrderByDescending(x => x.Object.Fecha)
                                       .ThenByDescending(x => x.Object.Hora)
                                       .Take(5)
                                       .Select(x => x.Object)
                                       .ToList();

            return View(ultimasCitas);
        }

        private string ParsearFechaHora(DateTime fecha, string hora)
        {
            try
            {
                return fecha.ToString("yyyy-MM-dd") + "T" + hora;
            }
            catch
            {
                return fecha.ToString("yyyy-MM-dd");
            }
        }
    }
}