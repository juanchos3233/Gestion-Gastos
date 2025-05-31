using ExpenseTracker.Models;
using System.Collections.Generic;

namespace ExpenseTracker.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalGastos { get; set; }
        public decimal TotalEntradas { get; set; }
        public List<Categoria> Categorias { get; set; } = new();
    }
}