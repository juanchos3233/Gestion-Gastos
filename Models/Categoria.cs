using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres")]
        public string? Descripcion { get; set; }

        [Range(0, 100, ErrorMessage = "Debe estar entre 0% y 100%")]
        public decimal PorcentajeMaximo { get; set; }

        public bool Eliminada { get; set; } = false;

        public ICollection<Gasto>? Gastos { get; set; }
    }
}
