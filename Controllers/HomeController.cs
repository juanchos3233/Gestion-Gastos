using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.ViewModels;


namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalGastos = await _context.Gastos.SumAsync(g => (decimal?)g.Monto) ?? 0,
                TotalEntradas = await _context.Entradas.SumAsync(e => (decimal?)e.Monto) ?? 0,
                Categorias = await _context.Categorias
                    .Where(c => !c.Eliminada)
                    .OrderBy(c => c.Titulo)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}