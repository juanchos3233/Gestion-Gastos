using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Entrada
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }
    }
}
