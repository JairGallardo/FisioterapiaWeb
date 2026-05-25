using Firebase.Database;
using Firebase.Database.Query;
using FisioterapiaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            if (string.IsNullOrEmpty(ficha.Id))
            {
                ficha.Fecha = DateTime.Now;
                ficha.NombrePaciente = nombrePaciente;
                ficha.RecepcionistaEmail = User.Identity?.Name;
                ficha.Recepcionista = User.IsInRole("Gerente") ? "Gerente" : "Recepcionista";

                await _client.Child("Historias").Child(ficha.PacienteId).PostAsync(ficha);
            }
            else
            {
                var notaDb = await _client.Child("Historias").Child(ficha.PacienteId).Child(ficha.Id).OnceSingleAsync<FichaClinica>();

                if (User.IsInRole("Gerente") || notaDb.RecepcionistaEmail == User.Identity?.Name)
                {
                    notaDb.Edad = ficha.Edad;
                    notaDb.Genero = ficha.Genero;
                    notaDb.Telefono = ficha.Telefono;
                    notaDb.Diagnostico = ficha.Diagnostico;
                    notaDb.AspectoFuncional = ficha.AspectoFuncional;
                    notaDb.Tratamiento = ficha.Tratamiento;

                    await _client.Child("Historias").Child(ficha.PacienteId).Child(ficha.Id).PutAsync(notaDb);
                }
            }

            return RedirectToAction("Index", new { pacienteId = ficha.PacienteId, nombre = nombrePaciente });
        }

        [HttpPost]
        [Authorize(Roles = "Gerente")]
        public async Task<IActionResult> GestionarGerente(string pacienteId, string notaId, string comentario, bool resaltar, string nombrePaciente)
        {
            if (!string.IsNullOrEmpty(pacienteId) && !string.IsNullOrEmpty(notaId))
            {
                var nota = await _client.Child("Historias").Child(pacienteId).Child(notaId).OnceSingleAsync<FichaClinica>();
                if (nota != null)
                {
                    nota.ComentarioGerente = comentario;
                    nota.EstaResaltado = resaltar;

                    await _client.Child("Historias").Child(pacienteId).Child(notaId).PutAsync(nota);
                }
            }
            return RedirectToAction("Index", new { pacienteId = pacienteId, nombre = nombrePaciente });
        }
    }
}