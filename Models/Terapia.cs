namespace FisioterapiaWeb.Models
{
    public class Terapia
    {
        public string? Id { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public bool EsPaquete { get; set; }
    }
}