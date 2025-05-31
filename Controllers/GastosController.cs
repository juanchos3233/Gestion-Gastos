using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class GastosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GastosController(ApplicationDbContext context) => _context = context;

        // GET: Gastos
        public async Task<IActionResult> Index()
        {
            var gastos = await _context.Gastos
                .Include(g => g.Categoria)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();

            return View(gastos); // Debes tener Views/Gastos/Index.cshtml
        }


        // GET: Gastos/Create
        public async Task<IActionResult> Create()
        {
            DateTime hoy = DateTime.Today;
            var hayEntradaEsteMes = await _context.Entradas
                .AnyAsync(e => e.Fecha.Month == hoy.Month && e.Fecha.Year == hoy.Year);

            if (!hayEntradaEsteMes)
            {
                TempData["Error"] = "No puedes registrar un gasto porque no se han registrado entradas este mes.";
                return RedirectToAction("Index");
            }

            ViewData["CategoriaId"] = _context.Categorias
                .Where(c => !c.Eliminada)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Titulo
                }).ToList();

            return View();
        }

        // POST: Gastos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoriaId,Monto,Fecha,Descripcion")] Gasto gasto)
        {
            var hayEntradaEsteMes = await _context.Entradas
                .AnyAsync(e => e.Fecha.Month == gasto.Fecha.Month && e.Fecha.Year == gasto.Fecha.Year);

            if (!hayEntradaEsteMes)
            {
                ModelState.AddModelError(string.Empty, "❌ No puedes registrar un gasto porque no hay entradas registradas en el mismo mes.");
            }

            if (gasto.CategoriaId == 0)
            {
                ModelState.AddModelError("CategoriaId", "La categoría es obligatoria");
            }

            if (ModelState.IsValid)
            {
                _context.Add(gasto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "✅ Gasto registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = _context.Categorias
                .Where(c => !c.Eliminada)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Titulo
                }).ToList();

            return View(gasto);
        }

        // GET: Gastos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gasto = await _context.Gastos.FindAsync(id);
            if (gasto == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => !c.Eliminada), "Id", "Titulo", gasto.CategoriaId);
            return View(gasto);
        }

        // POST: Gastos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoriaId,Monto,Fecha,Descripcion")] Gasto gasto)
        {
            if (id != gasto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gasto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GastoExists(gasto.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => !c.Eliminada), "Id", "Titulo", gasto.CategoriaId);
            return View(gasto);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var gasto = await _context.Gastos
                .Include(g => g.Categoria) // MUY IMPORTANTE
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gasto == null)
                return NotFound();

            return View(gasto);
        }


        // GET: Gastos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gasto == null) return NotFound();

            return View(gasto);
        }

        // Updated code to fix CS8602: Desreferencia de una referencia posiblemente NULL.
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gasto == null || gasto.Categoria == null) // Added null check for Categoria
            {
                return NotFound();
            }

            string categoriaNombre = gasto.Categoria.Titulo; // Safe access after null check
            var monto = gasto.Monto;

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se eliminó el gasto de {monto:C} de la categoría '{categoriaNombre}'.";

            return RedirectToAction(nameof(Index));
        }

        // Updated code to fix CS8602: Desreferencia de una referencia posiblemente NULL.
        public async Task<IActionResult> ReportePorCategoria(string filtro = "total")
        {
            var gastosQuery = _context.Gastos.Include(g => g.Categoria).AsQueryable();
            DateTime hoy = DateTime.Today;

            switch (filtro.ToLower())
            {
                case "hoy":
                case "diario":
                    gastosQuery = gastosQuery.Where(g => g.Fecha.Date == hoy);
                    break;
                case "semanal":
                    DateTime inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek + 1);
                    DateTime finSemana = inicioSemana.AddDays(6);
                    gastosQuery = gastosQuery.Where(g => g.Fecha >= inicioSemana && g.Fecha <= finSemana);
                    break;
                case "mensual":
                    gastosQuery = gastosQuery.Where(g => g.Fecha.Month == hoy.Month && g.Fecha.Year == hoy.Year);
                    break;
            }

            var datos = await gastosQuery
                .Where(g => g.Categoria != null) // Added null check for Categoria
                .GroupBy(g => g.Categoria!.Titulo) // Safe access using null-forgiving operator (!)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(x => x.Monto)
                })
                .ToListAsync();

            ViewBag.Categorias = datos.Select(d => d.Categoria).ToList();
            ViewBag.Totales = datos.Select(d => d.Total).ToList();
            ViewBag.Filtro = filtro;

            return View();
        }

        private bool GastoExists(int id) => _context.Gastos.Any(e => e.Id == id);
    }
}
