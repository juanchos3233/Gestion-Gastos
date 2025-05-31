using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .Where(c => !c.Eliminada)
                .ToListAsync();

            return View(categorias);
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titulo,Descripcion,PorcentajeMaximo,Eliminada")] Categoria categoria)
        {
            var totalPorcentaje = _context.Categorias
                .Where(c => !c.Eliminada)
                .Sum(c => c.PorcentajeMaximo);

            if ((totalPorcentaje + categoria.PorcentajeMaximo) > 100)
            {
                ModelState.AddModelError("PorcentajeMaximo", "El total de los porcentajes no puede superar el 100%");
            }

            if (ModelState.IsValid)
            {
                categoria.Eliminada = false; // aseguramos que se cree como activa
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Descripcion,PorcentajeMaximo,Eliminada")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            var otrasCategorias = _context.Categorias
                .Where(c => c.Id != categoria.Id && !c.Eliminada)
                .Sum(c => c.PorcentajeMaximo);

            if ((otrasCategorias + categoria.PorcentajeMaximo) > 100)
            {
                ModelState.AddModelError("PorcentajeMaximo", "La suma de los porcentajes activos no puede superar el 100%");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FirstOrDefaultAsync(m => m.Id == id);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                categoria.Eliminada = true; // eliminación lógica
                _context.Update(categoria);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"La categoría '{categoria.Titulo}' fue eliminada visualmente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.Id == id);
        }
    }
}
