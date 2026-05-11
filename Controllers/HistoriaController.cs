using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FisioterapiaWeb.Controllers
{
    [Authorize]
    public class HistoriaController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index(string pacienteId, string nombre)
        {
            if (string.IsNullOrEmpty(pacienteId))
            {
                return RedirectToAction("Index", "Cita");
            }

            try
            {
                var datos = await _client.Child("Historias").Child(pacienteId).OnceAsync<FichaClinica>();

                var lista = datos.Select(x => {
                    var f = x.Object;
                    f.Id = x.Key;
                    return f;
                }).OrderByDescending(x => x.Fecha).ToList();

                ViewBag.PacienteId = pacienteId;
                ViewBag.Nombre = nombre ?? "Paciente";

                return View(lista);
            }
            catch
            {
                ViewBag.PacienteId = pacienteId;
                ViewBag.Nombre = nombre;
                return View(new List<FichaClinica>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> GuardarNota(FichaClinica ficha, string nombrePaciente)
        {
            if (ficha == null || string.IsNullOrEmpty(ficha.PacienteId))
            {
                return RedirectToAction("Index", "Cita");
            }

            ficha.Fecha = DateTime.Now;
            ficha.NombrePaciente = nombrePaciente;

            await _client.Child("Historias").Child(ficha.PacienteId).PostAsync(ficha);

            return RedirectToAction("Index", new { pacienteId = ficha.PacienteId, nombre = nombrePaciente });
        }
    }
}