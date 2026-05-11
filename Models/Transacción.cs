using System;

namespace FisioterapiaWeb.Models
{
    public class Transaccion
    {
        public string? Id { get; set; }
        public string? PacienteId { get; set; }
        public string? PacienteNombre { get; set; }
        public string? Concepto { get; set; }
        public decimal Monto { get; set; }
        public string? MetodoPago { get; set; }
    }
}