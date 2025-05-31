using System;
using System.Collections.Generic;
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

        public GastosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gastos
        public async Task<IActionResult> Index(string filtro = "total")
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

                case "total":
                default:
                    break;
            }

            var gastos = await gastosQuery.OrderByDescending(g => g.Fecha).ToListAsync();
            ViewBag.Filtro = filtro;
            return View(gastos);
        }

        // GET: Gastos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gasto == null) return NotFound();

            return View(gasto);
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

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => !c.Eliminada), "Id", "Titulo");
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
                TempData["Error"] = "No puedes registrar un gasto porque no se han registrado entradas en el mes seleccionado.";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                _context.Add(gasto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.Where(c => !c.Eliminada), "Id", "Titulo", gasto.CategoriaId);
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

        // POST: Gastos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gasto = await _context.Gastos
                .Include(g => g.Categoria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gasto == null) return NotFound();

            var categoriaNombre = gasto.Categoria.Titulo;
            var monto = gasto.Monto;

            _context.Gastos.Remove(gasto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Se eliminó el gasto de {monto:C} de la categoría '{categoriaNombre}'. El total de gastos se ha ajustado automáticamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Reporte por categoría
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

                case "total":
                default:
                    break;
            }

            var datos = await gastosQuery
                .GroupBy(g => g.Categoria.Titulo)
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

        private bool GastoExists(int id)
        {
            return _context.Gastos.Any(e => e.Id == id);
        }
    }
}
