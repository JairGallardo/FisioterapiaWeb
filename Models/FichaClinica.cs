using System;

namespace FisioterapiaWeb.Models
{
    public class FichaClinica
    {
        public string? Id { get; set; }
        public string? PacienteId { get; set; }
        public DateTime Fecha { get; set; }
        public string? NombrePaciente { get; set; }
        public string? MotivoConsulta { get; set; }
        public string? Diagnostico { get; set; }
        public string? Evolucion { get; set; }
        public string? Especialista { get; set; }

        public string? Edad { get; set; }
        public string? Genero { get; set; }
        public string? Telefono { get; set; }
        public string? AspectoFuncional { get; set; }
        public string? Tratamiento { get; set; }
    }
}