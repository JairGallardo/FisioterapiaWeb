namespace FisioterapiaWeb.Models
{
    public class Cita
    {
        public string Id { get; set; }
        public string PacienteId { get; set; }
        public string NombrePaciente { get; set; }
        public DateTime Fecha { get; set; }
        public string Hora { get; set; }
        public string Tratamiento { get; set; }
        public string Especialista { get; set; }
        public string Lugar { get; set; }
        public string Estado { get; set; }
        public int DuracionMinutos { get; set; }
    }
}