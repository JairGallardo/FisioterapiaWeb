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
                Correo = x.Object.Correo,
                Edad = x.Object.Edad,
                Genero = x.Object.Genero ?? "Masculino",
                Condicion = x.Object.Condicion ?? "Fisioterapia general",
                Estado = (x.Object.Estado ?? "Activo").Trim(),
                FechaIngreso = x.Object.FechaIngreso ?? DateTime.Now.ToString("dd/MM/yy")
            }).ToList();

            return View(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> Guardar(Paciente nuevoPaciente)
        {
            if (string.IsNullOrEmpty(nuevoPaciente.Condicion)) nuevoPaciente.Condicion = "Fisioterapia general";
            if (string.IsNullOrEmpty(nuevoPaciente.Estado)) nuevoPaciente.Estado = "Activo";
            nuevoPaciente.FechaIngreso = DateTime.Now.ToString("dd/MM/yy");

            await _client.Child("Pacientes").PostAsync(nuevoPaciente);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPaciente(string id)
        {
            var p = await _client.Child("Pacientes").Child(id).OnceSingleAsync<Paciente>();
            if (p == null) return NotFound();
            p.Id = id;
            return Json(p);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCitasPaciente(string pacienteId)
        {
            var listaCitas = await _client.Child("Citas").OnceAsync<Cita>();
            var citasFiltradas = listaCitas
                .Where(x => x.Object.PacienteId == pacienteId)
                .Select(x => new Cita
                {
                    Id = x.Key,
                    PacienteId = x.Object.PacienteId,
                    NombrePaciente = x.Object.NombrePaciente,
                    Fecha = x.Object.Fecha,
                    Hora = x.Object.Hora,
                    Tratamiento = x.Object.Tratamiento,
                    Recepcionista = x.Object.Recepcionista,
                    Lugar = x.Object.Lugar,
                    Estado = x.Object.Estado ?? "Finalizado",
                    DuracionMinutos = x.Object.DuracionMinutos
                })
                .OrderByDescending(c => c.Fecha)
                .ToList();

            return Json(citasFiltradas);
        }

        [HttpPost]
        public async Task<IActionResult> Actualizar(Paciente p)
        {
            if (string.IsNullOrEmpty(p.FechaIngreso)) p.FechaIngreso = DateTime.Now.ToString("dd/MM/yy");
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