using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FisioterapiaWeb.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisioterapiaWeb.Controllers
{
    [Authorize(Roles = "Gerente")]
    public class UsuarioController : Controller
    {
        private readonly FirebaseClient _client = new FirebaseClient("https://centrobienestar-d226d-default-rtdb.firebaseio.com/");

        public async Task<IActionResult> Index(string searchString, string estado = "Todos")
        {
            var datos = await _client.Child("Usuarios").OnceAsync<Usuario>();

            var listaUsuarios = datos.Select(x => {
                var u = x.Object;

                if (int.TryParse(x.Key, out int idNumerico))
                {
                    u.Id = idNumerico;
                }
                else
                {
                    u.Id = Math.Abs(x.Key.GetHashCode());
                }

                return u;
            }).ToList();

            var filtrados = listaUsuarios.AsEnumerable();

            if (!string.IsNullOrEmpty(searchString))
            {
                filtrados = filtrados.Where(u => (!string.IsNullOrEmpty(u.NombreCompleto) && u.NombreCompleto.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                                             || (!string.IsNullOrEmpty(u.DNI) && u.DNI.Contains(searchString)));
            }

            if (estado != "Todos")
            {
                filtrados = filtrados.Where(u => !string.IsNullOrEmpty(u.Estado) && u.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.SearchString = searchString;
            ViewBag.EstadoActual = estado;
            ViewBag.TotalUsuarios = listaUsuarios.Count;

            return View(filtrados.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Usuario nuevoUsuario)
        {
            if (ModelState.IsValid)
            {
                nuevoUsuario.FechaIngreso = DateTime.Now;
                await _client.Child("Usuarios").PostAsync(nuevoUsuario);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Usuario usuarioEditado)
        {
            if (usuarioEditado.Id > 0)
            {
                string idString = usuarioEditado.Id.ToString();
                await _client.Child("Usuarios").Child(idString).PutAsync(usuarioEditado);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            if (id > 0)
            {
                string idString = id.ToString();
                await _client.Child("Usuarios").Child(idString).DeleteAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}