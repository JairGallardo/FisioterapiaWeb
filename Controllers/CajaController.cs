using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
            }).OrderByDescending(x => x.Fecha).ToList();

            return View(lista);
        }

        [HttpGet]
        public async Task<JsonResult> HistorialSemanal()
        {
            var datos = await _client.Child("Caja").OnceAsync<Transaccion>();
            var hoy = DateTime.Today;

            int diasDesdeLunes = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
            if (diasDesdeLunes < 0) diasDesdeLunes += 7;
            var inicioSemana = hoy.AddDays(-diasDesdeLunes);

            var listaSemana = datos
                .Select(x => x.Object)
                .Where(x => x.Fecha >= inicioSemana)
                .OrderByDescending(x => x.Fecha)
                .Select(x => new {
                    concepto = x.Concepto,
                    metodoPago = x.MetodoPago,
                    paciente = x.PacienteNombre,
                    monto = x.Monto.ToString("N2"),
                    fecha = x.Fecha.ToString("dd/MM/yyyy")
                }).ToList();

            return Json(listaSemana);
        }

        [HttpGet]
        public async Task<JsonResult> HistorialMensual()
        {
            var datos = await _client.Child("Caja").OnceAsync<Transaccion>();

            var resumenMeses = datos
                .Select(x => x.Object)
                .GroupBy(x => new { x.Fecha.Year, x.Fecha.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Select(g => new {
                    mesAnio = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month).ToUpper()} {g.Key.Year}",
                    total = g.Sum(x => x.Monto).ToString("N2"),
                    detalles = g.Select(d => new {
                        concepto = d.Concepto,
                        paciente = d.PacienteNombre,
                        monto = d.Monto.ToString("N2"),
                        fecha = d.Fecha.ToString("dd/MM")
                    }).ToList()
                }).ToList();

            return Json(resumenMeses);
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
            t.Fecha = DateTime.Now;
            await _client.Child("Caja").PostAsync(t);
            return RedirectToAction("Index");
        }
    }
}